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

using PeakMap;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace PeakMapWPF.ViewModels
{

    class MainWindowMatchingViewModel : MainWindowViewModel
    {
        private SpectralData specdata;

        private DataRowView _selectedPeak;
        public DataRowView SelectedPeak
        {
            get { return _selectedPeak; }
            set
            {
                _selectedPeak = value;
                OnPropertyChanged("SelectedPeak");
                if (value != null)
                {
                    SearchForNucs();
                    FillSelectedPeakMatches();
                }
            }
        }

        public DataView Peaks
        {
            get { return matches.Peaks.DefaultView; }
            set
            {
                OnPropertyChanged("Peaks");
            }
        }

        public string InputFile
        {
            get
            {
                if (specdata == null)
                    return "";
                return specdata.SpectralFile;
            }
        }

        private ObservableCollection<ContextAction> _selectedPeakMatches;

        public ObservableCollection<ContextAction> SelectedPeakMatches
        {
            get { return _selectedPeakMatches; }
            set
            {
                _selectedPeakMatches = value;
                OnPropertyChanged("SelectedPeakMatches");
            }
        }

        public MainWindowMatchingViewModel(IDialogService dialogService, IFileDialogService fileDialog)
            : base(dialogService, fileDialog)
        {
            UserInputCommand = new RelayCommand(UserInputCommand_Executed, CanUserInputExecute);
            PeaksContextMenuCommand = new RelayCommand(PeaksContextMenuCommand_Executed, CanPeaksContextMenuExecute);
            PastedTextCommand = new RelayCommand(PasteCommand_Executed, CanPasteExecute);
            FileMenuCommand = new RelayCommand(FileMenuCommand_Executed, CanFileMenuExecute);
            LinesMenuCommand = new RelayCommand(LinesMenuCommand_Executed, CanLinesMenuExecute);
            SettingsMenuCommand = new RelayCommand(SettingsMenuCommand_Executed, CanSettingsMenuExecute);

            SelectedPeakMatches = new ObservableCollection<ContextAction>
            {
                new ContextAction { Name = $"Paste", Action = PastedTextCommand }
            };

            CurrentModeViewModel = this;
            Lines.Sort = "[YIELD] DESC";
        }

        /// <summary>
        /// Fill the slected peak matches
        /// </summary>
        private void FillSelectedPeakMatches()
        {
            if (SelectedPeak == null)
                return;
            if (SelectedPeak["MATCHNAME"] == DBNull.Value)
                return;

            SelectedPeakMatches.Clear();
            SelectedPeakMatches.Add(new ContextAction { Name = $"Paste", Action = PastedTextCommand });

            //get the nucldes and add them to a collection of commands
            string[] matches = ((string)SelectedPeak["MATCHNAME"]).Split(',');
            foreach (string nucName in matches)
            {
                SelectedPeakMatches.Add(new ContextAction { Name = $"Clear { nucName } from Peak {SelectedPeak["ID"]}", Action = PeaksContextMenuCommand });
                SelectedPeakMatches.Add(new ContextAction { Name = $"Clear {nucName} from all Peaks", Action = PeaksContextMenuCommand });
                SelectedPeakMatches.Add(new ContextAction { Name = $"Clear all matches", Action = PeaksContextMenuCommand });
            }
        }
        /// <summary>
        /// Set the lines filter and get highlighted peaks
        /// </summary>
        protected override void SetLinesFilter()
        {
            base.SetLinesFilter();

            GetHighlightedPeaks();
        }
        /// <summary>
        /// Get the highlighed peaks
        /// </summary>
        private void GetHighlightedPeaks()
        {
            if (SelectedNuclide == null)
                return;
            matches.ClearTenativeMatches((string)SelectedNuclide["NAME"]);
            //loop thorough the lines, set the MDA, and get and display the matched lines 
            foreach (DataRowView line in Lines)
            {
                matches.SetPeakMatches(line.Row, (string)SelectedNuclide["NAME"]);
            }

            FillSelectedPeakMatches();
        }

        /// <summary>
        /// Asynchroniously get the spectal data from the file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task GetSpecDataAsync(string file)
        {
            //check the input
            if (String.IsNullOrWhiteSpace(file))
                return;

            try
            {
                //get the type of spectral data
                Type specType = SpectralData.GetSpectralDataType(file);
                //create an instance so we can actually use it
                specdata = Activator.CreateInstance(specType, new string[] { file }) as SpectralData;

                //initilize the spectral data object
                await specdata.LoadDataAsync(matches.Peaks);

                //apply the peaks to the lines
                matches.SpecData = specdata;
                OnPropertyChanged("InputFile");
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                DialogViewModel dialogViewModel = new DialogViewModel("Could not open: " + file, "Exeption: Bad File", true,false);
                dialogService.ShowDialog(dialogViewModel);
            }
            catch (IndexOutOfRangeException)
            {
                DialogViewModel dialogViewModel = new DialogViewModel("File: " + file + " did not contain any peak inforamtion", "Exeption: Bad File", true, false);
                dialogService.ShowDialog(dialogViewModel);
            }
            catch (System.IO.IOException)
            {
                DialogViewModel dialogViewModel = new DialogViewModel("File: " + file + " is in use by another program", "Exeption: File in Use", true, false);
                dialogService.ShowDialog(dialogViewModel);
            }
            catch (System.IO.FileFormatException ex) 
            {
                DialogViewModel dialogViewModel = new DialogViewModel($"File: {file} \n\n{ex.Message}", "Exeption: File Format", true, false);
                dialogService.ShowDialog(dialogViewModel);
            }
            catch (Exception ex)
            {
                DialogViewModel dialogViewModel = new DialogViewModel(ex.Message + ":\n" + ex.StackTrace, "General Exception", true, false);
                dialogService.ShowDialog(dialogViewModel);
            }

        }

        /// <summary>
        /// Search for nuclides associated with a specific energy. 
        /// </summary>
        /// <param name="energy"></param>
        private async void SearchForNucs()
        {
            if (specdata == null)
                await GetSpecDataAsync("User");
            if(SelectedNuclide != null)
                matches.ClearTenativeMatches((string)SelectedNuclide["NAME"]);

            string sort = Nuclides.Sort;
            //clear all the matches
            matches.ClearMatches();

            try
            {
                //check the inputs
                if (SelectedPeak["ENERGY"] == DBNull.Value)
                    return;

                matches.SetNuclides((double)SelectedPeak["ENERGY"], specdata.ElapsedWait, (double)SelectedPeak["FWHM"]);

                Nuclides.Sort = sort;
            }
            catch (Exception ex)
            {
                DialogViewModel dialogViewModel = new DialogViewModel(ex.Message, "General Exception", true);
                dialogService.ShowDialog(dialogViewModel);
            }
        }

        protected override void GetLines()
        {
            if (!base.CanGetLines())
                return;

            //set the lines
            matches.SetLines(SelectedNuclide.Row, SelectedPeak.Row);

            //highlight the matched peaks
            GetHighlightedPeaks();

            //display the line match socre
            LineMatchScore = matches.ScoreLineMatch();
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
                DialogViewModel dialogViewModel = new DialogViewModel($"The pasted values are not in a recongized format", "Could not read pasted values", true);
                dialogService.ShowDialog(dialogViewModel);
                return;
            }

            //apply the clean array
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
                    DataRow row = matches.Peaks.NewRow();
                    row["ENERGY"] = energy;
                    matches.Peaks.Rows.Add(row);
                }
                else
                {
                    continue;
                }
            }
        }

        private void PeaksContextMenuCommand_Executed(object context)
        {
            string menuName = context.ToString().ToLowerInvariant();
            //clear the all peaks with a matched nuclide 
            if (menuName.Contains("peaks") && menuName.Contains("all"))
            {
                //get the nuclide name
                string nucName = menuName.Split(' ')[1].ToUpper();

                //get the peak we need
                DataRow[] matchedPeaks = Peaks.Table.Select($"MATCHNAME LIKE '*{nucName}*'");
                foreach (DataRow peak in matchedPeaks)
                {
                    libGen.ClearNuclide(nucName);
                    matches.ClearPersistentMatch(peak, nucName);
                }
            }
            //clear all the matches
            else if (menuName.Contains("matches") && menuName.Contains("all"))
            {
                //get the peaks with matches
                DataRow[] matchedPeaks = Peaks.Table.Select($"MATCHNAME NOT ISNULL");
                foreach (DataRow peak in matchedPeaks)
                {
                    libGen.ClearNuclide(peak);
                    matches.ClearPersistentMatch(peak);
                }
            }
            //clear 1 nuclide from one peak
            else
            {
                string[] splitStr = context.ToString().Split(' ');
                string nucName = splitStr[1];
                int peakNo = int.Parse(splitStr[splitStr.Length]);
                DataRow peak = Peaks.Table.Rows[peakNo];
                libGen.ClearLines((double)peak["ENERGY"]);
                matches.ClearPersistentMatch(peak, nucName);
            }

        }

        private bool CanPeaksContextMenuExecute(object context)
        {
            if (context == null)
                return false;

            string menuName = context.ToString().ToLowerInvariant();
            if (SelectedPeak != null)
                return SelectedPeak["MATCHNAME"] != DBNull.Value;

            return false;

        }
        /// <summary>
        /// Handle the file drop event
        /// </summary>
        /// <param name="files"></param>
        public override async void OnFileDrop(string[] files)
        {
            if (files.Length > 0)
            {
                if (Path.GetExtension(files[0]).Equals(".cnf", StringComparison.OrdinalIgnoreCase) ||
                    Path.GetExtension(files[0]).Equals(".csv", StringComparison.OrdinalIgnoreCase) ||
                    Path.GetExtension(files[0]).Equals(".txt", StringComparison.OrdinalIgnoreCase))
                    await GetSpecDataAsync(files[0]);
                else
                    base.OnFileDrop(files);
            }
        }
        
        private async void UserInputCommand_Executed(object context)
        {
            if (InputFile != null || specdata != null)
            {
                DialogViewModel dialogView = new DialogViewModel($"Do you wish to close the current input and switch to a user input", "Close Input", false);
                if (dialogService.ShowDialog(dialogView) != true)
                    return;
            }

            OnPropertyChanged("InputFile");
            IsInputUser = false;
            await GetSpecDataAsync("User");
        }
        private bool CanUserInputExecute(object context)
        {
            return true;
        }
        /// <summary>
        /// Open an input file
        /// </summary>
        public async Task OpenInput()
        {
            fileDialogService.Filter = "All Spectral Types|*.csv;*.cnf;*.txt|(*.csv)|*.csv|(*.cnf)|*.cnf|(*.txt)|*.txt|All Files (*.*)|*.*";
            string filename = fileDialogService.OpenFileDialog(FileOperation.Open);

            if (filename != null)
            {
                CloseInput();
                await GetSpecDataAsync(filename);
            }
        }
        /// <summary>
        /// close the open input
        /// </summary>
        private void CloseInput()
        {
            if (specdata == null)
                return;

            specdata.CloseFile();
            matches.Clear();
            IsInputUser = false;
            specdata = null;
            OnPropertyChanged("InputFile");
        }

        protected override void PasteCommand_Executed(object context)
        {
            ImportPastedTextAsync(Clipboard.GetText());
        }
        protected override bool CanPasteExecute(object context)
        {
            return base.CanPasteExecute(context);
        }

        protected override async void FileMenuCommand_Executed(object context)
        {
            string menuName = context.ToString().ToLowerInvariant();
            if (menuName.Contains("open") && menuName.Contains("input"))
            {
               
                await OpenInput();
            }

            else if (menuName.Contains("close") && menuName.Contains("input"))
                CloseInput();
            else if (menuName.Contains("spectral"))
            {
                SpectralSettingsViewModel dialogViewModel = new SpectralSettingsViewModel(specdata);
                dialogService.ShowDialog(dialogViewModel);
            }
            else
                base.FileMenuCommand_Executed(context);
        }
        protected override bool CanFileMenuExecute(object context)
        {
            string menuName = context.ToString().ToLowerInvariant();
            if (menuName.Contains("close") && menuName.Contains("input"))
                return specdata != null;
            else if (menuName.Contains("input"))
                return true;
            else
                return base.CanFileMenuExecute(context);
        }

        protected override void LinesMenuCommand_Executed(object context)
        {
            
            string menuName = context.ToString().ToLowerInvariant();
            if (menuName.Contains("write") && menuName.Contains("matched"))
            {
                WriteLines(WriteType.Matched);
                IsWriteMatchedChecked = true;
            }
            else
                base.LinesMenuCommand_Executed(context);
        }

        protected override bool CanLinesMenuExecute(object context)
        {
            string menuName = context.ToString().ToLowerInvariant();
            if (menuName.Contains("write") && menuName.Contains("selected"))
            {
                return SelectedNuclide != null && LibraryFile != null;
            }
            
            return base.CanLinesMenuExecute(context);
        }
        protected override void SettingsMenuCommand_Executed(object context)
        {
            string menuName = context.ToString().ToLowerInvariant();
            if (menuName.Contains("spectral"))
            {
                SpectralSettingsViewModel dialogViewModel = new SpectralSettingsViewModel(specdata);
                dialogService.ShowDialog(dialogViewModel);
            }

            base.SettingsMenuCommand_Executed(context);
        }
        protected override bool CanSettingsMenuExecute(object context)
        {
            string menuName = context.ToString().ToLowerInvariant();
            if (menuName.Contains("spectral"))
                return specdata != null;

            else
                return base.CanSettingsMenuExecute(context);
        }
    }
}
