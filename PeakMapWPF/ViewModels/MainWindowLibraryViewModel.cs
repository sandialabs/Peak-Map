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
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows;
using System.Collections;

namespace PeakMapWPF.ViewModels
{
    class MainWindowLibraryViewModel : MainWindowViewModel
    {

        private DataRowView _selectedLibrayNuclide;

        public DataRowView SelectedLibrayNuclide
        {
            get { return _selectedLibrayNuclide; }
            set
            {
                _selectedLibrayNuclide = value;
                OnPropertyChanged("SelectedLibrayNuclide");
                if (value != null)
                {
                    HighlightWrittenLines();
                    FillNuclideContextManu();
                }
            }
        }

        public string SearchNuclide
        {
            set
            {
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
            get { return base.Nuclides; }

        }

        public override string LibraryFile { get => base.LibraryFile; set => base.LibraryFile = value; }

        public DataRowView SelectedLine
        { get; set; }


        public DataView LibraryNuclides
        {
            get
            {
                if (libGen != null)
                    return libGen.NuclideLibrary.Tables["MATCHEDNUCLIDES"].DefaultView;
                else
                    return null;
            }
            set { OnPropertyChanged("LibraryNuclides"); }

        }
        private ObservableCollection<ContextAction> _selectedNuclideContextItems;

        public ObservableCollection<ContextAction> SelectedNuclideContextItems
        {
            get { return _selectedNuclideContextItems; }
            set
            { _selectedNuclideContextItems = value; }
        }

        public RelayCommand LinesContextMenuCommand { get; protected set; }
        private ObservableCollection<ContextAction> _selectedLinesContextItems;

        public ObservableCollection<ContextAction> SelectedLinesContextItems
        {
            get { return _selectedLinesContextItems; }
            set
            { _selectedLinesContextItems = value; }
        }
        public MainWindowLibraryViewModel(IDialogService dialogService, IFileDialogService fileDialog)
           : base(dialogService, fileDialog) 
        {
            Initialize();
        }

        public MainWindowLibraryViewModel(IDialogService dialogService, IFileDialogService fileDialog, Matches matches)
            : base(dialogService, fileDialog, matches)
        {
            Initialize();
        }
        /// <summary>
        /// Initilize all the constructor variables
        /// </summary>
        private void Initialize() 
        {
            PastedTextCommand = new RelayCommand(PasteCommand_Executed, CanPasteExecute);
            FileMenuCommand = new RelayCommand(FileMenuCommand_Executed, CanFileMenuExecute);
            LinesMenuCommand = new RelayCommand(LinesMenuCommand_Executed, CanLinesMenuExecute);
            SettingsMenuCommand = new RelayCommand(SettingsMenuCommand_Executed, CanSettingsMenuExecute);
            MatchesContextMenuCommand = new RelayCommand(NuclidesContextMenuCommand_Executed, CanNuclidesPeaksContextMenuExecute);
            LinesContextMenuCommand = new RelayCommand(LinesContextMenuCommand_Executed, CanLinesContextMenuExecute);

            SelectedNuclideContextItems = new ObservableCollection<ContextAction>();
            SelectedLinesContextItems = new ObservableCollection<ContextAction>()
            {
                new ContextAction { Name = $"Write Selected",  Action = LinesContextMenuCommand },
                new ContextAction { Name = $"Write All",  Action = LinesContextMenuCommand },
            };

            CurrentModeViewModel = this;
            Lines.Sort = "[ENERGY]";
        }

        protected async override Task GetLibraryFileAsync(FileOperation operation)
        {
            await base.GetLibraryFileAsync(operation);

            OnPropertyChanged("LibraryNuclides");

        }

        private void FillNuclideContextManu()
        {
            if (SelectedLibrayNuclide == null)
                return;
            if (SelectedLibrayNuclide["NAME"] == DBNull.Value)
                return;

            SelectedNuclideContextItems.Clear();
            SelectedNuclideContextItems.Add(new ContextAction { Name = $"Delete {SelectedLibrayNuclide["NAME"]}", Action = MatchesContextMenuCommand });
            SelectedNuclideContextItems.Add(new ContextAction { Name = $"Delete entire libary", Action = MatchesContextMenuCommand });
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

            matches.SetNuclides(name, 0, true);


            SelectedNuclide = Nuclides.OfType<DataRowView>().Where(r => r["NAME"] != null && r["NAME"].ToString().Equals(name.Trim())).FirstOrDefault();
            //OnPropertyChanged("SelectedNuclide");

            base.GetLines();

            DataRow[] foundLines = libGen.NuclideLibrary.Tables["MATCHEDLINES"].Select("NAME = '" + name.Trim() + "'");
            //ArrayList selectedLines = new ArrayList();
            //highlight the matched lines
            foreach (DataRow row in foundLines)
            {
                var matchedLines = matches.Lines.DefaultView.OfType<DataRowView>().Where(r => Math.Abs((double)row["ENERGY"] - (double)r.Row["ENERGY"]) < libGen.ResolutionLimit);
                SelectedLine = matchedLines.FirstOrDefault<DataRowView>();
                //loop through all the lines that match
                foreach (DataRowView rw in matchedLines)
                {
                    rw["MATCHED"] = true;
                   // SelectedLines.Add(rw);
                }

            }
            if(SelectedLinesContextItems.Count > 2)
            {
                SelectedLinesContextItems.RemoveAt(2);
                SelectedLinesContextItems.RemoveAt(2);
            }
            SelectedLinesContextItems.Add(new ContextAction { Name = $"Remove selected lines from {SelectedLibrayNuclide["NAME"]}", Action = LinesContextMenuCommand });
            SelectedLinesContextItems.Add(new ContextAction { Name = $"Remove all lines from {SelectedLibrayNuclide["NAME"]}", Action = LinesContextMenuCommand });
            //SelectedLines = selectedLines;
            OnPropertyChanged("SelectedLines");
        }

        /// <summary>
        /// Write the Lines to Library
        /// </summary>
        /// <param name="type">Write Type</param>
        protected override void WriteLines(WriteType type) 
        {
            base.WriteLines(type);
            //HighlightWrittenLines();
        }
        protected override void PasteCommand_Executed(object context)
        {
            ImportPastedText(Clipboard.GetText());
        }

        protected override void FileMenuCommand_Executed(object context)
        {
            base.FileMenuCommand_Executed(context);
        }
        protected override bool CanFileMenuExecute(object context)
        {
            return base.CanFileMenuExecute(context);
        }
        /// <summary>
        /// Handle the file drop event
        /// </summary>
        /// <param name="files"></param>
        public override async Task OnFileDrop(string[] files)
        {
                await base.OnFileDrop(files);
                OnPropertyChanged("LibraryNuclides");
        }
        protected bool CanNuclidesPeaksContextMenuExecute(object context)
        {
            if (context == null)
                return false;

            if (SelectedLibrayNuclide != null)
                return SelectedLibrayNuclide["NAME"] != DBNull.Value;

            return false;
        }

        protected void NuclidesContextMenuCommand_Executed(object context)
        {
            string menuName = context.ToString().ToLowerInvariant();
            if (menuName.Contains("entire"))
            {
                foreach (DataRowView nuc in LibraryNuclides)
                {
                    libGen.ClearNuclide(nuc.Row);
                }
            }
            else
            {
                libGen.ClearNuclide(SelectedLibrayNuclide.Row);
            }
        }

        protected bool CanLinesContextMenuExecute(object context)
        {
            if (context == null)
                return false;

            base.CanLinesMenuExecute(context);

            if (SelectedLine == null)
                return false;
            if (Lines != null)
                return SelectedLine["ENERGY"] != DBNull.Value;

            return false;
        }

        protected void LinesContextMenuCommand_Executed(object context)
        {
            string menuName = context.ToString().ToLowerInvariant();
            if (menuName.Contains("selected"))
            {
                foreach (DataRowView line in SelectedLines)
                {
                    line["MATCHED"] = false;
                    libGen.ClearLines((double)line["ENERGY"]);
                }
            }
            if (menuName.Contains("all"))
            {
                //var matchedLines = matches.Lines.DefaultView.OfType<DataRowView>().Where(r => (bool)r["MATCHED"]);
                //foreach (DataRowView line in matchedLines)
                //{}
                libGen.ClearNuclide(SelectedLibrayNuclide.Row);
            }
            else
            {
                base.LinesMenuCommand_Executed(context);
            }

        }
    }
}
