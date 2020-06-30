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
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PeakMapWPF.ViewModels
{
    class MainWindowLibraryViewModel : MainWindowViewModel
    {

        private DataRowView _selectedLibrayNuclide;

        public DataRowView SelectedLibrayNuclide
        {
            get { return _selectedLibrayNuclide; }
            set {
                _selectedLibrayNuclide = value;
                OnPropertyChanged("SelectedLibrayNuclide");
                if (value != null)
                {
                    HighlightWrittenLines();
                }
            }
        }
        public string SearchNuclide {
            set {
                if (string.IsNullOrEmpty(value))
                {
                    matches.ClearMatches();
                    return;
                }
                string sort = Nuclides.Sort;
                matches.SetNuclides(value);
                Nuclides.Sort = sort;
            } 
        }
        public override DataView Nuclides
        {
            get {return base.Nuclides; }

        }

        public override string LibraryFile { get => base.LibraryFile; set => base.LibraryFile = value; }

        public DataRowView SelectedLine
        { get; set; }


        public DataView LibraryNuclides
        {
            get {
                if (libGen != null)
                    return libGen.NuclideLibrary.Tables["MATCHEDNUCLIDES"].DefaultView;
                else
                    return null;
            }
            set { OnPropertyChanged("LibraryNuclides"); }

        }


        public MainWindowLibraryViewModel(IDialogService dialogService, IFileDialogService fileDialog ) 
            : base(dialogService, fileDialog)
        {

            PastedTextCommand = new RelayCommand(PasteCommand_Executed, CanPasteExecute);
            FileMenuCommand = new RelayCommand(FileMenuCommand_Executed, CanFileMenuExecute);
            LinesMenuCommand = new RelayCommand(LinesMenuCommand_Executed, CanLinesMenuExecute);
            SettingsMenuCommand = new RelayCommand(SettingsMenuCommand_Executed, CanSettingsMenuExecute);

            CurrentModeViewModel = this;
            Lines.Sort = "[ENERGY]";

        }
        protected async override Task GetLibraryFileAsync(FileOperation operation)
        {
            await base.GetLibraryFileAsync(operation);

            OnPropertyChanged("LibraryNuclides");

        }

        /// <summary>
        /// Get a list of pasted nuclides and put them in the nuclides grid
        /// </summary>
        /// <param name="text"></param>
        private void ImportPastedText(string text)
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
                DialogViewModel dialogViewModel = new DialogViewModel($"The values were not in a format that is recognized", "Could not read pasted values", true);
                dialogService.ShowDialog(dialogViewModel);
                return;
            }

            //apply the clean array
            //clean up
            matches.ClearMatches();
            //loop through the pasted array and add them to the matchedNuclides Grid
            for (int i = 0; i < splitText.Length; i++)
            {
                //check for empites
                if (string.IsNullOrEmpty(splitText[i]) || string.IsNullOrWhiteSpace(splitText[i]))
                    continue;
            
                try
                {
                    matches.SetNuclides(splitText[i], matches.Nuclides.Rows.Count);
                }
                catch (Exception ex)
                {
                   DialogViewModel dialogViewModel = new DialogViewModel(ex.Message + ":\n" + ex.StackTrace, "General Exception", true);
                   dialogService.ShowDialog(dialogViewModel);
               }
            }          

        }

        /// <summary>
        /// Highlight lines already written in the lines table when selected
        /// </summary>
        /// <param name="name">Nuclide name</param>
        private void HighlightWrittenLines()
        {
            //select the nuclide
            string name = (string)SelectedLibrayNuclide.Row["NAME"];

            matches.SetNuclides(name,0,true);
            

            SelectedNuclide = Nuclides.OfType<DataRowView>().Where(r => r["NAME"] != null && r["NAME"].ToString().Equals(name.Trim())).FirstOrDefault();
            //OnPropertyChanged("SelectedNuclide");
            
            base.GetLines();

            DataRow[] foundLines = libGen.NuclideLibrary.Tables["MATCHEDLINES"].Select("NAME = '" + name.Trim() + "'");
            //ArrayList selectedLines = new ArrayList();
            //SelectedLines = null;
            //highlight the matched lines
            foreach (DataRow row in foundLines)
            {
                var matchedLines = matches.Lines.DefaultView.OfType<DataRowView>().Where(r => Math.Abs((double)row["ENERGY"] - (double)r.Row["ENERGY"]) < libGen.ResolutionLimit);
                SelectedLine = matchedLines.FirstOrDefault<DataRowView>();
                //loop through all the lines that match
                foreach (DataRowView rw in matchedLines)
                {
                    SelectedLines.Add(rw);
                }
            }
            //SelectedLines = selectedLines;
            OnPropertyChanged("SelectedLines");
        }

        protected override void PasteCommand_Executed(object context)
        {
            ImportPastedText(Clipboard.GetText());
        }

        protected override  void FileMenuCommand_Executed(object context)
        {
            base.FileMenuCommand_Executed(context);
        }
        protected override bool CanFileMenuExecute(object context)
        {
            return base.CanFileMenuExecute(context);
        }

    }
}
