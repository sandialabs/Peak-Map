/********************************************************************************************************************************************************************************************************************************************************                          
NOTICE:

For five (5) years from 1/21/2020 the United States Government is granted for itself and others acting on its behalf a paid-up, nonexclusive, irrevocable worldwide license in this data to reproduce, prepare derivative works, and perform 
publicly and display publicly, by or on behalf of the Government. There is provision for the possible extension of the term of this license. Subsequent to that period or any extension granted, the United States Government is granted for itself
and others acting on its behalf a paid-up, nonexclusive, irrevocable worldwide license in this data to reproduce, prepare derivative works, distribute copies to the public, perform publicly and display publicly, and to permit others to do so. The
specific term of the license can be identified by inquiry made to National Technology and Engineering Solutions of Sandia, LLC or DOE.
 
NEITHER THE UNITED STATES GOVERNMENT, NOR THE UNITED STATES DEPARTMENT OF ENERGY, NOR NATIONAL TECHNOLOGY AND ENGINEERING SOLUTIONS OF SANDIA, LLC, NOR ANY OF THEIR EMPLOYEES, MAKES ANY WARRANTY, 
EXPRESS OR IMPLIED, OR ASSUMES ANY LEGAL RESPONSIBILITY FOR THE ACCURACY, COMPLETENESS, OR USEFULNESS OF ANY INFORMATION, APPARATUS, PRODUCT, OR PROCESS DISCLOSED, OR REPRESENTS THAT ITS USE WOULD
NOT INFRINGE PRIVATELY OWNED RIGHTS.
 
Any licensee of this software has the obligation and responsibility to abide by the applicable export control laws, regulations, and general prohibitions relating to the export of technical data. Failure to obtain an export control license or other 
authority from the Government may result in criminal liability under U.S. laws.
 
                                             (End of Notice)
*********************************************************************************************************************************************************************************************************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PeakMap
{
    public partial class Main : Form
    {
        SpectralData specdata;
        SpectralLibraryGenerator libGen;
        readonly LineMatch lines;
        private readonly IDataLibrary library;
        int winWidth;
        int gridWidth;
        int peakWidth;
        Point gridLocation;
        WindowMode mode;
        Point clickPoint;

        private enum FileOperation { New, Open };
        private enum WriteType { All, Yield, Matched, Selected };
        private enum WindowMode { matching, library };
        /// <summary>
        /// public constructor
        /// </summary>
        public Main()
        {
            InitializeComponent();
            library = new ICRPData();
            lines = new LineMatch(library);
            mode = WindowMode.matching;
            //initilize the binding sources

            //Peaks grid
            using (BindingSource bindingSource = new BindingSource())
            {
                this.peaksGrid.DataSource = bindingSource.DataSource = lines.Peaks;
                System.Collections.Specialized.StringCollection peakCols = Properties.Settings.Default.PEAKS;
                FormatColumns(this.peaksGrid, peakCols);

                //Matches Grid
                this.matchedNuclidesGrid.DataSource = bindingSource.DataSource = lines.Matches;
                this.matchedNuclidesGrid.Sort(matchedNuclidesGrid.Columns["SCORE"], ListSortDirection.Descending);
                System.Collections.Specialized.StringCollection nucCols = Properties.Settings.Default.NUCLIDES;
                FormatColumns(this.matchedNuclidesGrid, nucCols);

                //Lines Grid
                this.matchedLinesGrid.DataSource = bindingSource.DataSource = lines.Lines;
                this.matchedLinesGrid.Sort(matchedLinesGrid.Columns["YIELD"], ListSortDirection.Descending);
                System.Collections.Specialized.StringCollection lineCols = Properties.Settings.Default.LINES;
                FormatColumns(this.matchedLinesGrid, lineCols);
            }
        }

        //TODO add visulation
        //TODO add ORTEC
        //TODO add other spectral types and peak search

        /// <summary>
        /// Asynchroniously get the spectal data from the file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private async Task GetSpecDataAsync(string file)
        {
            //disable event raising for loading
            peaksGrid.SelectionChanged -= this.PeaksGrid_SelectionChanged;
            statusLabel.Text = "Loading: " + file;
            statusStrip.Update();
            spectralToolStripMenuItem.Enabled = true;
            try
            {
                //get the type of spectral data
                Type specType = SpectralData.GetSpectralDataType(file);
                //create an instance so we can actually use it
                specdata = Activator.CreateInstance(specType, new string[] { file }) as SpectralData;

                //initilize the spectral data object
                await specdata.LoadDataAsync(lines.Peaks);

                this.peaksGrid.Refresh();

                statusLabel.Text = "Loaded: " + file;
                statusStrip.Update();

                //throw some info up
                CntTimeLabel.Text = "Start Time: " + specdata.AcquisitionTime;
                AqTimeLabel.Text = "Acquisition Time: " + specdata.CountTime;

                //apply the peaks to the lines
                lines.SpecData = specdata;
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                MessageBox.Show("Could not open: " + file, "Bad File", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("File: " + file + " did not contain any peak inforamtion", "Bad File", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException)
            {
                MessageBox.Show("File: " + file + " is in use by another program", "File in Use", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ":\n" + ex.StackTrace, "General Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                //enable event raising for loading
                peaksGrid.SelectionChanged += this.PeaksGrid_SelectionChanged;
                statusLabel.Text = file;
            }

        }
        /// <summary>
        /// Search for nuclides associated with a specific energy. 
        /// </summary>
        /// <param name="energy"></param>
        private void SearchForNucs(float energy)
        {

            //clean up before starting
            ClearPeaksHighlighting();
            lines.ClearMatches();
            //turn off the event firing
            matchedNuclidesGrid.SelectionChanged -= this.MatchedNuclides_SelectionChanged;

            //get all the lines th match the energy
            if (peaksGrid.SelectedRows.Count < 1)
                return;
            if (peaksGrid.SelectedRows[0].Cells["FWHM"].Value == null)
                return;
            try
            {
                lines.SetNuclides(energy, specdata.ElapsedWait, (double)peaksGrid.SelectedRows[0].Cells["FWHM"].Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception");
            }


            if (matchedNuclidesGrid.RowCount > 0)
            {
                this.matchedNuclidesGrid.Rows[0].Selected = true;
                this.matchedNuclidesGrid.CurrentCell = matchedNuclidesGrid.Rows[0].Cells[1];
                this.MatchedNuclides_SelectionChanged(matchedNuclidesGrid, EventArgs.Empty);
            }
            //allow event raising again
            matchedNuclidesGrid.SelectionChanged += this.MatchedNuclides_SelectionChanged;
            this.matchedNuclidesGrid.Refresh();

        }
        /// <summary>
        /// Get the Lines associated with the select nuclide
        /// </summary>
        private void GetLines()
        {
            if (lines == null)
                return;
            if (lines.Matches.Rows.Count < 1)
                return;

            ClearPeaksHighlighting();

            //get the highlighterd nuclide and peak
            DataRow nuc = ((DataRowView)matchedNuclidesGrid.CurrentRow.DataBoundItem).Row;
            DataRow peak = ((DataRowView)peaksGrid.CurrentRow.DataBoundItem).Row;

            lines.SetLines(nuc, peak);
            //loop thorough the lines, set the MDA, and get and display the matched lines 
            for (int i = 0; i < lines.Lines.Rows.Count; i++)
            {
                DataRow line = lines.Lines.Rows[i];
                //if ((bool)line["MATCHED"])
                //    continue;


                //get the peaks that matches the lines
                DataRow[] foundPeaks = lines.GetPeakMatches(line);
                //highlight the matched lines
                foreach (DataRow row in foundPeaks)
                {
                    int rowIndex = peaksGrid.Rows.IndexOf(peaksGrid.Rows.OfType<DataGridViewRow>().FirstOrDefault(r => (int)row["ID"] == (int)r.Cells["ID"].Value));

                    peaksGrid.Rows[rowIndex].DefaultCellStyle.BackColor = Color.Orange;
                    peaksGrid.Rows[rowIndex].DefaultCellStyle.ForeColor = SystemColors.HighlightText;
                }
            }

            //display the line match socre
            lineMatchScore.Text = (lines.ScoreLineMatch()).ToString("P3");
            //refresh.
            this.matchedLinesGrid.Refresh();

        }
        /// <summary>
        /// Get the Library lines from the current nuc in Nucs Grid
        /// </summary>
        private void GetLibraryLines()
        {
            if (lines == null)
                return;
            if (lines.Matches.Rows.Count < 1 || (DataRowView)matchedNuclidesGrid.CurrentRow.DataBoundItem == null)
                return;
            DataRow nuc = ((DataRowView)matchedNuclidesGrid.CurrentRow.DataBoundItem).Row;
            lines.SetLines(nuc);
            this.matchedLinesGrid.Refresh();
        }
        /// <summary>
        /// Get the highlighted rows from the peaks grid view
        /// </summary>
        /// <returns>Array of highlighted rows</returns>
        private DataGridViewRow[] GetHighlightedPeakRows()
        {
            List<DataGridViewRow> highligthedRows = new List<DataGridViewRow>(); ;
            foreach (DataGridViewRow row in peaksGrid.Rows)
            {
                if (row.DefaultCellStyle.BackColor != Color.White || row.Selected)
                    highligthedRows.Add(row);
            }
            return highligthedRows.ToArray();
        }
        /// <summary>
        /// Clear the highlighing from a peak
        /// </summary>
        private void ClearPeaksHighlighting()
        {
            DataGridViewRow[] rows = GetHighlightedPeakRows();
            foreach (DataGridViewRow row in rows)
            {
                row.DefaultCellStyle.BackColor = Color.White;
                row.DefaultCellStyle.ForeColor = SystemColors.ControlText;
            }
        }
        /// <summary>
        /// Write the Lines to a CAM Library
        /// </summary>
        /// <param name="type"></param>
        private void WriteLinesAsync(WriteType type)
        {
            //check if a nuclide is selected
            if (matchedNuclidesGrid.SelectedRows.Count <= 0)
                return;

            try
            {
                //get the nuclides to write to the library
                DataRow nuc = ((DataRowView)matchedNuclidesGrid.CurrentRow.DataBoundItem).Row;
                bool isPhotoPeak = (string)nuc["NAME"] != "S.E." && (string)nuc["NAME"] != "D.E." && !((string)nuc["NAME"]).Contains("Sum");


                //get the lines to write to the library
                if (isPhotoPeak)
                {
                    DataTable writeLines;
                    switch (type)
                    {
                        case WriteType.All:
                            if (showDaughtersToolStripMenuItem.Checked)
                                writeLines = lines.Lines.Select().CopyToDataTable();
                            else
                                writeLines = lines.Lines.Select(" NAME ='" + nuc["NAME"] + "'").CopyToDataTable();
                            break;
                        case WriteType.Matched:
                            writeLines = lines.Lines.Select("MATCHED = True").CopyToDataTable();
                            break;
                        case WriteType.Selected:
                            //get the selected rows
                            if (matchedLinesGrid.SelectedRows.Count < 1)
                            {
                                MessageBox.Show("Please select lines to add to the library", "No Lines found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                return;
                            }
                            ////loop through the rows and add them to the array. 
                            writeLines = lines.Lines.Clone();
                            for (int i = 0; i < matchedLinesGrid.SelectedRows.Count; i++)
                                writeLines.ImportRow(((DataRowView)matchedLinesGrid.SelectedRows[i].DataBoundItem).Row);
                            writeLines.AcceptChanges();
                            break;
                        case WriteType.Yield:
                            using (YieldSelector selector = new YieldSelector())
                            {
                                //get the minimum yeild from the user
                                if (selector.ShowDialog() == DialogResult.OK)
                                    if (showDaughtersToolStripMenuItem.Checked)
                                        writeLines = lines.Lines.Select("YIELD >= " + selector.Yield.ToString()).CopyToDataTable();
                                    else
                                        writeLines = lines.Lines.Select("YIELD >= " + selector.Yield.ToString() + " AND NAME ='" + nuc["NAME"] + "'").CopyToDataTable();
                                else
                                    return;
                            }
                            break;
                        default:
                            writeLines = lines.Lines.Select().CopyToDataTable();
                            break;
                    }
                    //write the nuclides to the library
                    libGen.WriteNuclide(nuc, writeLines);
                }

                //write the matched nuclide to the peaks grid
                if (mode == WindowMode.matching)
                {
                    //only write everthing if it is a normal peak
                    if (isPhotoPeak)
                    {
                        DataGridViewRow[] mathchedRows = GetHighlightedPeakRows();
                        foreach (DataGridViewRow row in mathchedRows)
                        {
                            DataRow peak = ((DataRowView)row.DataBoundItem).Row;
                            peak["MATCHNAME"] = (peak["MATCHNAME"] == DBNull.Value) ?
                                 (string)nuc["NAME"] : (string)peak["MATCHNAME"] + "," + (string)nuc["NAME"];
                        }
                    }
                    //do single and double escape peaks
                    else
                    {
                        DataRow peak = ((DataRowView)peaksGrid.SelectedRows[0].DataBoundItem).Row;
                        peak["MATCHNAME"] = (peak["MATCHNAME"] == DBNull.Value) ?
                                 (string)nuc["NAME"] : (string)peak["MATCHNAME"] + "," + (string)nuc["NAME"];

                        return;
                    }
                }
                //if we are in library mode just keep a running list of nuclides added
                else
                {
                    DataRow addedNuc = lines.Peaks.NewRow();
                    //don't add repat nuclides
                    if (lines.Peaks.Select("MATCHNAME = '" + nuc["NAME"] + "'").Length > 0)
                        return;
                    addedNuc["MATCHNAME"] = nuc["NAME"];
                    lines.Peaks.Rows.Add(addedNuc);
                }

                peaksGrid.Refresh();

            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an exeption while writing to the library:\n" + ex.Message, "Error while writing to library", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        /// <summary>
        /// Format columns based off the settings file
        /// </summary>
        /// <param name="view">The grid view</param>
        /// <param name="formatCollection">A collecton of comma seperate strings represtnign the formatting values</param>
        private void FormatColumns(DataGridView view, System.Collections.Specialized.StringCollection formatCollection)
        {
            string[] formatStrings = formatCollection.Cast<string>().ToArray();
            //hide all the columns in the view
            view.Columns.OfType<DataGridViewColumn>().ToList().ForEach(col => col.Visible = false);

            //foreach(DataGridViewColumn viewCol in view.Columns)
            for (int index = 0; index < formatStrings.Length; index++)
            {
                string[] t_col = formatCollection[index].Split(',');
                DataGridViewColumn viewCol = view.Columns[t_col[0]];
                //get the index of the columns
                //int index = Array.FindIndex<string>(formatStrings, s => s.StartsWith(viewCol.Name));
                if (index >= 0)
                {
                    viewCol.Visible = true;
                    viewCol.HeaderText = t_col[1];
                    viewCol.DisplayIndex = index;
                    if (!string.IsNullOrEmpty(t_col[2]))
                        viewCol.DefaultCellStyle.Format = t_col[2];
                }
                else
                    viewCol.Visible = false;
            }
        }

        /// <summary>
        /// Creates and open file Dialog so users can search for a CAM file
        /// </summary>
        private async void GetInputFile()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = @"C:\GENIE2K\CAMFILES";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //make sure the input is closed
                    CloseInput();
                    this.peaksGrid.ReadOnly = true;
                    await GetSpecDataAsync(openFileDialog.FileName);
                }

            }
        }
        /// <summary>
        /// Load the data or creates a new file to save the library
        /// </summary>
        private async void GetLibraryFileAsync(FileOperation operation)
        {
            FileDialog dialog;
            //get the type of dialog based on the FileOperation Enum
            switch (operation)
            {
                case FileOperation.New:
                    dialog = new SaveFileDialog();
                    break;
                case FileOperation.Open:
                    dialog = new OpenFileDialog();
                    break;
                default:
                    return;
            }

            dialog.InitialDirectory = @"C:\GENIE2K\CAMFILES";
            dialog.Filter = "(*.nlb)|*.nlb|(*.cnf)|*.cnf|(*.tlb)|*.tlb|All Files (*.*)|*.*";
            dialog.DefaultExt = ".nlb";
            dialog.CheckFileExists = false;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                //delete the file if we are overwriting with the save dialog
                if (dialog.GetType() == typeof(SaveFileDialog) && File.Exists(dialog.FileName))
                    File.Delete(dialog.FileName);

                libLabel.Text = "Library: Loading";
                statusLabel.Text = "Loading Library";
                try
                {
                    Type libType = SpectralLibraryGenerator.GetSpectralDataType(dialog.FileName);
                    //create an instance so we can actually use it
                    libGen = Activator.CreateInstance(libType, new string[] { dialog.FileName }) as SpectralLibraryGenerator;

                    //initilize the library data object
                    await libGen.LoadLibraryAsync(dialog.FileName);
                    statusLabel.Text = "Idle";
                    libLabel.Text = "Library: " + libGen.FileName;
                    librarySettingsToolStripMenuItem.Enabled = true;
                    FillWithLibrary();
                }
                catch (Exception ex)
                {
                    libLabel.Text = "Library: ";
                    MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    dialog.Dispose();
                }
            }
            else
            {
                dialog.Dispose();
                return;
            }

            //turn on writing matchedLinesGrid.Rows.Count > 0 &&
            SaveLibraryToolStripMenuItem.Enabled = true;
            saveAsToolStripMenuItem.Enabled = true;

            ToggleLinesToolStripMenuItems(libGen != null);

            rightClickWriteToolStripMenuItem.Enabled = libGen != null;
        }
        /// <summary>
        /// Fill the peaks grid the loaded library nuclides
        /// </summary>
        private void FillWithLibrary()
        {                    //fill the peaks grid with the nuclides
            if (mode == WindowMode.library && libGen != null) 
            {
                foreach (DataRow nuc in libGen.NuclideLibrary.Tables["MATCHEDNUCLIDES"].Rows)
                {
                    DataRow addedNuc = lines.Peaks.NewRow();
                    //don't add repat nuclides
                    if (lines.Peaks.Select("MATCHNAME = '" + nuc["NAME"] + "'").Length > 0)
                        return;
                    addedNuc["MATCHNAME"] = nuc["NAME"];
                    lines.Peaks.Rows.Add(addedNuc);
                }

            }
        }
        /// <summary>
        /// Allow only one tool strip item to be check at a time
        /// </summary>
        /// <param name="selectedMenuItem"></param>
        public void UncheckOtherToolStripMenuItems(ToolStripMenuItem selectedMenuItem)
        {
            selectedMenuItem.Checked = true;

            // Select the other MenuItens from the ParentMenu(OwnerItens) and unchecked this,
            // The current Linq Expression verify if the item is a real ToolStripMenuItem
            // and if the item is a another ToolStripMenuItem to uncheck this.
            foreach (var ltoolStripMenuItem in (from object
                                                    item in selectedMenuItem.Owner.Items
                                                let ltoolStripMenuItem = item as ToolStripMenuItem
                                                where ltoolStripMenuItem != null
                                                where !item.Equals(selectedMenuItem)
                                                select ltoolStripMenuItem))
                (ltoolStripMenuItem).Checked = false;
        }

        /// <summary>
        /// Get a list of pasted nuclides and put them in the nuclides grid
        /// </summary>
        /// <param name="text"></param>
        private async void ImportPastedTextAsync(string text)
        {
            //try to split the 
            string[] delims = { Environment.NewLine, ",", "|", " ", "\t", ";" };
            string[] splitText = { "" };
            foreach (string delim in delims)
            {
                splitText = text.Split(delim.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (splitText.Length > 1)
                    break;
            }
            //check if we can use this array
            if (splitText.Length <= 1)
            {
                MessageBox.Show("The values were not in a format that is recognized", "Could not read pasted values", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            //apply the clean array
            switch (mode)
            {
                case WindowMode.library:
                    //clean up
                    lines.ClearMatches();
                    //loop through the pasted array and add them to the matchedNuclides Grid
                    for (int i = 0; i < splitText.Length; i++)
                    {
                        //check for empites
                        if (string.IsNullOrEmpty(splitText[i]) || string.IsNullOrWhiteSpace(splitText[i]))
                            continue;

                        try
                        {
                            lines.SetNuclides(splitText[i], lines.Matches.Rows.Count);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Exception");
                        }
                    }
                    matchedNuclidesGrid.Refresh();
                    break;
                case WindowMode.matching:
                    if (specdata == null)
                        await GetSpecDataAsync("User");
                    //loop through the pasted array and add them to the matchedNuclides Grid
                    for (int i = 0; i < splitText.Length; i++)
                    {
                        //check for empites
                        if (string.IsNullOrEmpty(splitText[i]) || string.IsNullOrWhiteSpace(splitText[i]))
                            continue;
                        //only add doubles
                        if (double.TryParse(splitText[i], out double energy))
                        {
                            DataRow row = lines.Peaks.NewRow();
                            row["ENERGY"] = energy;
                            lines.Peaks.Rows.Add(row);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    peaksGrid.Refresh();
                    break;
            }

        }
        /// <summary>
        /// Apply the filter for the daughter lines 
        /// </summary>
        private void ApplyDaughterLinesFilter()
        {
            if (matchedNuclidesGrid.RowCount < 1)
                return;
            if (matchedNuclidesGrid.CurrentRow == null)
                return;
            string nucName = matchedNuclidesGrid.CurrentRow.Cells["NUCLIDE"].Value.ToString();
            //only show the lines from the selected nuclide
            if (!showDaughtersRightClickMenuItem.Checked && !String.IsNullOrEmpty(nucName))
                (matchedLinesGrid.DataSource as DataTable).DefaultView.RowFilter = "NAME = '" + nucName + "'";
            else if (showDaughtersRightClickMenuItem.Checked)
                (matchedLinesGrid.DataSource as DataTable).DefaultView.RowFilter = "";

        }
        /// <summary>
        /// Closes the input file
        /// </summary>
        private void CloseInput()
        {
            if (specdata != null)
                specdata.CloseFile();
            lines.Clear();
            //refresh the data grid views
            peaksGrid.Refresh();
            matchedNuclidesGrid.Refresh();
            matchedLinesGrid.Refresh();

            //allow user entry into happen again
            peaksGrid.ReadOnly = false;
            specdata = null;
            statusLabel.Text = "Idle";
            CntTimeLabel.Text = "Start Time: ";
            AqTimeLabel.Text = "Acquisition Time: ";
            spectralToolStripMenuItem.Enabled = false;
        }
        /// <summary>
        /// Toggle the Lines tool strip menu items
        /// </summary>
        /// <param name="enabled">enable</param>
        private void ToggleLinesToolStripMenuItems(bool enabled)
        {
            foreach (ToolStripItem item in linesToolStripMenuItem.DropDownItems)
            {
                //only enable matched writing for matching mode
                if ((string)item.Tag == "matched")
                    item.Enabled = mode == WindowMode.matching && enabled;
                else if ((string)item.Tag == "daughters")
                    item.Enabled = true;
                else
                    item.Enabled = enabled;
            }

        }
        /// <summary>
        /// Highlight lines already written in the lines table when selected
        /// </summary>
        /// <param name="name">Nuclide name</param>
        private void HighlightWrittenLines(string name)
        {
            matchedNuclidesGrid.ClearSelection();
            //select the nuclide
            if (matchedNuclidesGrid.Rows.Count > 0)
                matchedNuclidesGrid.CurrentCell = matchedNuclidesGrid.Rows.OfType<DataGridViewRow>().Where(r => r.Cells["NAME"].Value != null && r.Cells["NAME"].Value.ToString().Equals(name.Trim(' '))).FirstOrDefault().Cells["NAME"];

            GetLibraryLines();

            DataRow[] foundLines = libGen.NuclideLibrary.Tables["MATCHEDLINES"].Select("NAME = '" + name.Trim() + "'");
            //highlight the matched lines
            foreach (DataRow row in foundLines)
            {
                //loop through all the lines that match
                foreach (DataGridViewRow gridrow in matchedLinesGrid.Rows.OfType<DataGridViewRow>().Where(r => Math.Abs((double)row["ENERGY"] - (double)r.Cells["ENERGY"].Value) < libGen.ResolutionLimit))
                {
                    //change the colors
                    gridrow.DefaultCellStyle.BackColor = Color.Orange;
                    gridrow.DefaultCellStyle.ForeColor = SystemColors.HighlightText;
                }
            }
        }
        #region event handling
        /// <summary>
        /// Open button event handler, open a CAM files
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        private void OpenBtn_Click(object sender, EventArgs e)
        {
            GetInputFile();
        }

        /// <summary>
        /// Handle the openn tool stip click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetInputFile();
        }
        /// <summary>
        /// Event handle for closing a form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //close the spectral data
            if (specdata != null)
                specdata.CloseFile();
            //close the libary genertation file
            if (libGen != null && libGen.CanWrite)
                libGen.SaveFile();
        }
        /// <summary>
        /// Load Drag and drop file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Form1_DragDrop(object sender, DragEventArgs e)
        {
            //only allow one type of spec data
            lines.Clear();
            this.peaksGrid.ReadOnly = true;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            await GetSpecDataAsync(files[0]);
        }
        /// <summary>
        /// Contol ponter on a drag and drop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }
        /// <summary>
        /// Handle the Cell Enter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PeaksGrid_CellEnter(object sender, DataGridViewCellEventArgs e)
        {

        }
        /// <summary>
        /// Handle a row selection change in the peaksgrid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PeaksGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (peaksGrid.CurrentRow == null || peaksGrid.CurrentRow.IsNewRow)
                return;
            if (peaksGrid.CurrentRow.Index != -1)
            {
                if (lines != null)
                {
                    if (mode == WindowMode.matching)
                        lines.Lines.Clear();
                    matchedLinesGrid.Refresh();
                }

                if (float.TryParse(peaksGrid.CurrentRow.Cells["ENERGY"].Value.ToString(), out float energy))
                    SearchForNucs(energy);
                //Load the nuclides in the peaks grid
                else if (mode == WindowMode.library && !String.IsNullOrEmpty(peaksGrid.CurrentRow.Cells["MATCHNAME"].Value.ToString()))
                {
                    try
                    {
                        //lines.ClearMatches();
                        matchedLinesGrid.Refresh();
                        string nucName = peaksGrid.CurrentRow.Cells["MATCHNAME"].Value.ToString();
                        //get the exact name nuclides
                        lines.SetNuclides(nucName, lines.Matches.Rows.Count, true);
                        if(matchedNuclidesGrid.Rows.Count > 1) { 
                        //    int index = matchedNuclidesGrid.Rows.Cast<DataGridViewRow>().Where(r => r.Cells["NAME"].Value.ToString().Equals(nucName)).First().Index;
                        //    matchedNuclidesGrid.Rows[index].Selected = true;
                        }
                        //highlight the lines that were written to the library
                        HighlightWrittenLines(nucName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Exception");
                    }

                }

            }
        }
        /// <summary>
        /// Handle the cell end edit event on the peaks grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void PeaksGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (peaksGrid.CurrentRow.Index != -1)
            {
                if (specdata == null)
                    await GetSpecDataAsync("User");

                if (lines != null)
                {
                    lines.Lines.Clear();
                    matchedLinesGrid.Refresh();
                }
                if (peaksGrid["ENERGY", e.RowIndex].Value == null)
                    return;
                if (float.TryParse(peaksGrid["ENERGY", e.RowIndex].Value.ToString(), out float energy))
                    SearchForNucs(energy);
            }

        }
        /// <summary>
        /// Handle a row selection change in matched lines
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MatchedLines_SelectionChanged(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// Handle when the from loads
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            //set the width and locations
            winWidth = this.Width;
            gridWidth = this.matchedNuclidesGrid.Width;
            gridLocation = this.matchedNuclidesGrid.Location;
            peakWidth = this.peaksGrid.Width;
        }
        /// <summary>
        /// Get the lines for the selected nuclide
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MatchedNuclides_SelectionChanged(object sender, EventArgs e)
        {

            if (matchedNuclidesGrid.CurrentRow == null)
                return;

            if ( mode != WindowMode.library)
                GetLines();
            else if (mode == WindowMode.library)
                GetLibraryLines();

            //enable the library matching matchedLinesGrid.Rows.Count > 0 &&

            //linesToolStripMenuItem.Enabled =  libGen != null;
            ToggleLinesToolStripMenuItems(libGen != null);

            rightClickWriteToolStripMenuItem.Enabled = libGen != null;
            ApplyDaughterLinesFilter();
        }
        /// <summary>
        /// Write the lines to the library based on the selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WriteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //ToolStrip pctrl = ((ToolStripMenuItem)sender).Owner;
            ToolStrip pctrl = writeToolStripMenuItem.Owner;
            //get the checked item
            ToolStripMenuItem[] selections = pctrl.Items.OfType<ToolStripMenuItem>().Where(c => c.Checked).ToArray();
            ToolStripMenuItem selection = selections.Length > 1 ? selections[1] : selections[0];

            //Cast<ToolStripMenuItem>().Where(c => c.Checked).ToArray()[0];

            //decide which lines to write and do so
            switch (selection.Tag.ToString())
            {
                case "selected":
                    WriteLinesAsync(WriteType.Selected);
                    break;
                case "matched":
                    WriteLinesAsync(WriteType.Matched);
                    break;
                case "yield":
                    WriteLinesAsync(WriteType.Yield);
                    break;
                default:
                    WriteLinesAsync(WriteType.All);
                    break;
            }
        }
        /// <summary>
        /// Create a new library file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetLibraryFileAsync(FileOperation.New);
        }
        /// <summary>
        /// open a library file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            GetLibraryFileAsync(FileOperation.Open);
        }
        /// <summary>
        /// Close the input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearInputToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseInput();
        }

        /// <summary>
        /// Write selected lines to the library
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WriteSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UncheckOtherToolStripMenuItems((ToolStripMenuItem)sender);
            rightClickWriteToolStripMenuItem.Text = "Write Selected";
            WriteLinesAsync(WriteType.Selected);
        }
        /// <summary>
        /// Write matched lines to the library
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WriteMatchedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UncheckOtherToolStripMenuItems((ToolStripMenuItem)sender);
            rightClickWriteToolStripMenuItem.Text = "Write Matched";
            WriteLinesAsync(WriteType.Matched);
        }
        /// <summary>
        /// Select yield lines to the library
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WriteYieldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UncheckOtherToolStripMenuItems((ToolStripMenuItem)sender);
            rightClickWriteToolStripMenuItem.Text = "Write Yield...";
            WriteLinesAsync(WriteType.Yield);
        }
        /// <summary>
        /// Write all lines to the library
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WriteAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UncheckOtherToolStripMenuItems((ToolStripMenuItem)sender);
            rightClickWriteToolStripMenuItem.Text = "Write All";
            WriteLinesAsync(WriteType.All);
        }

        /// <summary>
        /// Handle the library toolStrip click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Generate a Library
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GenerateDataLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {

            DialogResult result = MessageBox.Show("Are you sure you want to rebuild the library database? This operation may take several hours", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                await library.InitializeAsync();
                library.WriteToDatabase();
            }
        }

        /// <summary>
        /// Close the Library CAM file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                libGen.SaveFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Library Save Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handle the SaveAsToolStripMenuItem Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileDialog fdialog = new SaveFileDialog()
            {
                InitialDirectory = @"C:\GENIE2K\CAMFILES",
                Filter = "(*.nlb)|*.nlb|(*.cnf)|*.cnf|(*.tlb)|*.tlb|All Files (*.*)|*.*",
                DefaultExt = ".nlb",
                CheckFileExists = false,
            };
            if (fdialog.ShowDialog() == DialogResult.OK && libGen != null)
                libGen.FileName = fdialog.FileName;
        }

        /// <summary>
        /// Close the input file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseInputToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseInput();
        }
        /// <summary>
        /// Open the setting dialog for line match settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LineMatchSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (Settings settingDialog = new Settings(lines))
            {
                settingDialog.ShowDialog();
            }
        }
        /// <summary>
        /// Open the setting dialog for Library Generation settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (Settings settingDialog = new Settings(libGen))
            {
                settingDialog.ShowDialog();
            }
        }

        /// <summary>
        /// Open the setting dialog for sepctral settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SpectralToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (Settings settingDialog = new Settings(specdata))
            {
                settingDialog.ShowDialog();
            }
            //update the info
            CntTimeLabel.Text = "Start Time: " + specdata.AcquisitionTime;
            AqTimeLabel.Text = "Acquisition Time: " + specdata.CountTime;
            //await GetSpecDataAsync(statusLabel.Text.TrimStart("Loading: ".ToCharArray()));
        }

        /// <summary>
        /// Handle the change of mode to matching
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MatchingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mode == WindowMode.matching)
                return;
            lines.Clear();
            mode = WindowMode.matching;
            UncheckOtherToolStripMenuItems((ToolStripMenuItem)sender);
            modeStatusLabel.Text = "Mode: Matching";

            System.Collections.Specialized.StringCollection peakCols = Properties.Settings.Default.PEAKS;
            FormatColumns(this.peaksGrid, peakCols);
            System.Collections.Specialized.StringCollection nucCols = Properties.Settings.Default.NUCLIDES;
            FormatColumns(this.matchedNuclidesGrid, nucCols);
            System.Collections.Specialized.StringCollection lineCols = Properties.Settings.Default.LINES;
            FormatColumns(this.matchedLinesGrid, lineCols);
            label2.Visible = true;
            statusLabel.Visible = true;
            CntTimeLabel.Visible = true;
            AqTimeLabel.Visible = true;
            writeMatchedToolStripMenuItem.Enabled = true;
            openToolStripMenuItem.Enabled = true;
            lineMatchScore.Visible = true;

            //rearrange the window and controls
            this.peaksGrid.Width = peakWidth;
            this.matchedNuclidesGrid.ReadOnly = true;
            this.matchedNuclidesGrid.AllowUserToAddRows = false;
            this.matchedNuclidesGrid.Location = new Point(gridLocation.X, this.matchedNuclidesGrid.Location.Y);
            this.matchedNuclidesGrid.Anchor = AnchorStyles.Left;
            this.matchedNuclidesGrid.Width = gridWidth;

            this.matchedLinesGrid.Location = new Point(gridLocation.X, this.matchedLinesGrid.Location.Y);
            this.matchedLinesGrid.Anchor = AnchorStyles.Left;
            this.matchedLinesGrid.Width = gridWidth;

            this.Size = new Size(winWidth, this.Height);
            this.matchedNuclidesGrid.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
            this.matchedLinesGrid.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);

            this.peaksGrid.ReadOnly = false;
            this.matchedLinesGrid.Sort(matchedLinesGrid.Columns["YIELD"], ListSortDirection.Descending);
        }
        /// <summary>
        /// Handle the change of mpode to library only
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LibrayOnlyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mode == WindowMode.library)
                return;
            //check if we should close the input
            if (specdata != null)
            {
                DialogResult result = MessageBox.Show("Switching to Library mode will close the Input, do you wish to continue?", "Close Input?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                    CloseInput();
                else
                    return;
            }
            mode = WindowMode.library;
            UncheckOtherToolStripMenuItems((ToolStripMenuItem)sender);
            modeStatusLabel.Text = "Mode: Library Only";


            System.Collections.Specialized.StringCollection nucCols = Properties.Settings.Default.NUCLIDESLIBRARY;
            FormatColumns(this.matchedNuclidesGrid, nucCols);
            System.Collections.Specialized.StringCollection lineCols = Properties.Settings.Default.LINESLIBRARY;
            FormatColumns(this.matchedLinesGrid, lineCols);
            System.Collections.Specialized.StringCollection peakCols = Properties.Settings.Default.PEAKSLIBRARY;
            FormatColumns(this.peaksGrid, peakCols);
            //hide the lables
            label2.Visible = false;
            statusLabel.Visible = false;
            CntTimeLabel.Visible = false;
            AqTimeLabel.Visible = false;
            writeMatchedToolStripMenuItem.Enabled = false;
            openToolStripMenuItem.Enabled = false;
            lineMatchScore.Visible = false;

            //rearrange the window and controls
            this.matchedNuclidesGrid.ReadOnly = false;
            this.matchedNuclidesGrid.AllowUserToAddRows = true;
            foreach (DataGridViewColumn col in this.matchedNuclidesGrid.Columns)
                col.ReadOnly = true;
            this.peaksGrid.Width = peaksGrid.Width / 4;
            this.matchedNuclidesGrid.Columns["NAME"].ReadOnly = false;
            //this.matchedNuclidesGrid.Location = new Point(this.peaksGrid.Location.X, this.matchedNuclidesGrid.Location.Y);
            this.matchedNuclidesGrid.Location = new Point(peaksGrid.Location.X + peaksGrid.Width + 4 * matchedNuclidesGrid.Margin.Left + 4 * peaksGrid.Margin.Right, this.matchedNuclidesGrid.Location.Y);
            this.matchedLinesGrid.Location = new Point(this.matchedNuclidesGrid.Location.X, this.matchedLinesGrid.Location.Y);
            int libWidth = this.matchedNuclidesGrid.Location.X + peakWidth + 3 * (peaksGrid.Location.X) - peaksGrid.Margin.Left;
            this.Size = new Size(libWidth, this.Height);

            this.matchedNuclidesGrid.Width = peakWidth;
            this.matchedLinesGrid.Width = this.matchedNuclidesGrid.Width;

            this.matchedNuclidesGrid.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
            this.matchedLinesGrid.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);

            this.peaksGrid.ReadOnly = true;
            this.matchedLinesGrid.Sort(matchedLinesGrid.Columns["ENERGY"], ListSortDirection.Ascending);
            FillWithLibrary();

        }

        /// <summary>
        /// Handle the cell validating event in library mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MatchedNuclidesGrid_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            //can only be edited in library mode
            if (mode == WindowMode.library && e.RowIndex >= 0)
            {
                //get the value and pre-validation value
                string nuc = this.matchedNuclidesGrid["NAME", e.RowIndex].EditedFormattedValue.ToString();
                string nucV = this.matchedNuclidesGrid["NAME", e.RowIndex].Value.ToString();
                //perform the checks to see if we need to do anything
                if (String.IsNullOrEmpty(nuc) || nuc == null || nuc == nucV)
                    return;

                //clear the previous matches
                lines.ClearMatches();
                //Find the nuclides assoicated with the entry;
                try
                {
                    lines.SetNuclides(nuc);
                }
                catch (Exception ex)
                {

                    Debug.WriteLine(ex.Message + ":\n" + ex.StackTrace);
                    this.matchedNuclidesGrid["NAME", e.RowIndex].ToolTipText = ex.Message;
                }
            }
        }
        /// <summary>
        /// Handle the mouse click event 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Main_MouseClick(object sender, MouseEventArgs e)
        {
            //check if the right mouse button was pressed;
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                pasteToolStripMenuItem.Enabled = Clipboard.ContainsText();
                clearMatchesToolStripMenuItem.Enabled = peaksGrid.RowCount > 1;
                clearSelectionToolStripMenuItem.Enabled = peaksGrid.SelectedRows != null;

                //place the menu at the point of the click
                rightClickMenu.Show(Cursor.Position);

                //check if the user clicked on the peaks grid
                DataGridView.HitTestInfo hit = peaksGrid.HitTest(e.X, e.Y);
                clearCurrentRowToolStripMenuItem.Enabled = hit.Type == DataGridViewHitTestType.Cell;
                if (clearCurrentRowToolStripMenuItem.Enabled)
                    clickPoint = new Point(e.X, e.Y);

            }
        }
        //Handle the paste item 
        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportPastedTextAsync(Clipboard.GetText());
            Debug.WriteLine(Clipboard.GetText());
        }

        /// <summary>
        /// Handle the clear matches tool strip menu click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearMatchesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow[] rows = lines.Peaks.Select("MATCHNAME IS NOT NULL");
            foreach (DataRow row in rows)
            {
                //lines.ClearMatches();
                row["MATCHNAME"] = DBNull.Value;
            }
            //clear the library
            if (libGen != null && libGen.NuclideLibrary != null)
                libGen.NuclideLibrary.Clear();

            if (mode == WindowMode.library)
                CloseInput();
        }
        /// <summary>
        /// Clear the Match Names from selected peaks 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Get the Selected Rows
            DataGridViewSelectedRowCollection selRows = this.peaksGrid.SelectedRows;
            if (selRows.Count < 1)
                return;

            //loop through the selectedRows in peaksGrid and null out the MATCHNAME value
            foreach (DataGridViewRow row in selRows)
            {
                DataRow boundRow = (DataRow)((DataRowView)row.DataBoundItem).Row;
                //check for nulls
                if (boundRow == null)
                    continue;
                //lines.ClearMatches((string)boundRow["MATCHNAME"]);

                //clear the library
                if (libGen != null)
                    libGen.ClearNuclide((string)boundRow["MATCHNAME"]);

                if (mode == WindowMode.matching)
                    boundRow["MATCHNAME"] = DBNull.Value;

                else
                    boundRow.Delete();

                peaksGrid.Refresh();
                

            }
        }
        /// <summary>
        /// Clear the clicked row MATCHNAME 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearCurrentRowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (clickPoint == null)
                return;

            DataGridView.HitTestInfo hit = peaksGrid.HitTest(clickPoint.X, clickPoint.Y);
            if (hit.Type == DataGridViewHitTestType.Cell)
            {
                DataRow boundRow = (DataRow)((DataRowView)(peaksGrid.Rows[hit.RowIndex]).DataBoundItem).Row;
                //lines.ClearMatches((string)boundRow["MATCHNAME"]);

                //clear the library
                if (libGen != null)
                    libGen.ClearNuclide((string)boundRow["MATCHNAME"]);

                if (mode == WindowMode.matching)
                    boundRow["MATCHNAME"] = DBNull.Value;
                else
                    boundRow.Delete();

                peaksGrid.Refresh();

            }
        }
        /// <summary>
        /// Handle the keyDown event and past the values if crtl+v is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.V && e.Modifiers == Keys.Control) && Clipboard.ContainsText())
                ImportPastedTextAsync(Clipboard.GetText());
        }
        /// <summary>
        /// Handle the data grid view data error events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridView_DataError(object sender, DataGridViewDataErrorEventArgs err)
        {

            if (err.Context.ToString().Contains(DataGridViewDataErrorContexts.Commit.ToString()))
            {
                MessageBox.Show("Commit error");
            }
            if (err.Context.ToString().Contains(DataGridViewDataErrorContexts.CurrentCellChange.ToString()))
            {
                MessageBox.Show("Cell change");
            }
            if (err.Context.ToString().Contains(DataGridViewDataErrorContexts.Parsing.ToString()))
            {
                MessageBox.Show("parsing error, data is not the correct type");
            }
            if (err.Context.ToString().Contains(DataGridViewDataErrorContexts.LeaveControl.ToString()))
            {
                MessageBox.Show("leave control error");
            }
            if ((err.Exception) is ConstraintException)
            {
                DataGridView view = (DataGridView)sender;
                view.Rows[err.RowIndex].ErrorText = "an error";
                view.Rows[err.RowIndex].Cells[err.ColumnIndex].ErrorText = "an error";

                err.ThrowException = true;
            }
            err.Cancel = true;
        }
        /// <summary>
        /// show the about box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (AboutBox1 box = new AboutBox1())
            {
                box.ShowDialog();
            }
        }
        /// <summary>
        /// Handle the show daughters click by applying a filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowDaughtersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //match the right click menu and the tool strip menu
            if ((sender as ToolStripMenuItem).Name.Equals("showDaughtersToolStripMenuItem"))
            {
                showDaughtersRightClickMenuItem.Checked = showDaughtersToolStripMenuItem.Checked;
                showDaughtersRightClickMenuItem.CheckState = showDaughtersToolStripMenuItem.CheckState;
            }
            else
            {
                showDaughtersToolStripMenuItem.Checked = showDaughtersRightClickMenuItem.Checked;
                showDaughtersToolStripMenuItem.CheckState = showDaughtersRightClickMenuItem.CheckState;
            }
            ApplyDaughterLinesFilter();


        }
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }


        #endregion


    }

}
