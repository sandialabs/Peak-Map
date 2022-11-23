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
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Collections;
using System.Text;
using System.Reflection;
using System.Linq;

using System.Collections.ObjectModel;

namespace PeakMapWPF.ViewModels
{        
    
    public class MainWindowViewModel :  INotifyPropertyChanged , IFileDragDropTarget
    {
        #region Properties

        protected enum WriteType { All, Yield, Matched, Selected };
        public enum WindowMode { matching = 1200, library=500 };

        
        protected readonly IDataLibrary library;
        protected readonly Matches matches;
        protected SpectralData specdata;

        protected SpectralLibraryGenerator libGen;


        private string _libraryFile;

        private DataRowView _selectedNuclide;
        private IList _selectedLines;
        private double _lineMatchScore;

        //private string _linesSort;

        private bool _gammaFilter;
        private bool _xrayFilter;
        private bool _daughtersFilter;
        private double _lowerEnergy; 
        private double _upperEnergy;

        protected IDialogService dialogService;
        protected IFileDialogService fileDialogService;

        public event PropertyChangedEventHandler PropertyChanged;

        private MainWindowViewModel _currentModeViewModel;
        public MainWindowViewModel CurrentModeViewModel 
        {
            get { return _currentModeViewModel; }
            set 
            {
                if (_currentModeViewModel != value)
                {
                    _currentModeViewModel = value;                  
                    if(_currentModeViewModel != null)
                        WindowWidth = CurrentModeViewModel.GetType() == typeof(MainWindowMatchingViewModel) ? 1200 : 500;

                    OnPropertyChanged("CurrentModeViewModel");
                }
            }
        }

        private double _windowWidth;

        public double WindowWidth
        {
            get { return _windowWidth; }
            set { 
                _windowWidth = value; 
                OnPropertyChanged("WindowWidth"); 
            }
        }

        public virtual string LibraryFile
        {
            get { return _libraryFile; }
            set {
                _libraryFile = value;
                OnPropertyChanged("LibraryFile");
            }
        }

        private readonly DataView _nuclides;
        public virtual DataView Nuclides
        {
            get
            {
                return _nuclides;
            }
        }

        public DataRowView SelectedNuclide 
        { get { return _selectedNuclide; }
            set 
            {
                if (matches != null && _selectedNuclide != null && _selectedNuclide.Row.RowState != DataRowState.Detached)
                    matches.ClearTenativeMatches(_selectedNuclide["NAME"].ToString());

                _selectedNuclide = value;
                OnPropertyChanged("SelectedNuclide");
                if (value != null)
                    GetLines();

            } 
        }

        public IList SelectedLines
        {
            get { return _selectedLines; }
            set
            {
                _selectedLines = value;
                OnPropertyChanged("SelectedLines");
            }
        }
        private readonly DataView _lines;
        public DataView Lines 
        {
            get { 
                //DataView sortedView = matches.Lines.DefaultView;
                //sortedView.Sort = "ENERGY ASC";
                return _lines; }
        }


        //public string LinesSort { 
        //    get { return _linesSort; }
        //    set { _linesSort = value; }
        //}

        public double LineMatchScore
        {
            get { return _lineMatchScore; }
            set { 
                _lineMatchScore = value;
                OnPropertyChanged("LineMatchScore");
            }
        }

        public bool GammaFilter
        {
            get { return _gammaFilter; }
                    //string.IsNullOrEmpty(_gammaFilter); }
            set {
                _gammaFilter = value;
                //_gammaFilter = value ? " AND TYPE = 'G'" : "";
                OnPropertyChanged("GammaFilter");
                SetLinesFilter();
            }
        }

        public bool XRayFilter
        {
            get { return _xrayFilter; }
            set {
                _xrayFilter = value;
                //_xrayFilter = value ? " AND TYPE = 'X'" : "";
                OnPropertyChanged("XRayFilter");
                SetLinesFilter();
            }
        }

        public bool DaughtersFilter
        {
            get { return _daughtersFilter; }
            set {
                _daughtersFilter = value;
                //_daughtersFilter = value && SelectedNuclide != null ? " AND NAME = '" + SelectedNuclide.Row["NAME"] +"'": "";
                OnPropertyChanged("DaughterFilter");
                SetLinesFilter();
            }
        }

        public double LowerEnergy
        {
            get { return _lowerEnergy; }
            set { 
                _lowerEnergy = value;
                OnPropertyChanged("LowerEnergy");
                SetLinesFilter();
            }
        }

        public double UpperEnergy
        {
            get { return _upperEnergy; }
            set
            {
                _upperEnergy = value;
                OnPropertyChanged("UpperEnergy");
                SetLinesFilter();
            }
        }

        public string LinesFilter
        {
            get { return Lines.RowFilter; }
        }

        private double _lowerYield;

        public double LowerYield
        {
            get { return _lowerYield; }
            set { 
                _lowerYield = value;
                OnPropertyChanged("LowerYield");
                SetLinesFilter();
            }
        }
        private WriteType _writeType;
        private bool _isWriteAll;
        public bool IsWriteAllChecked
        {
            get { return _isWriteAll; }
            set {
                _isWriteAll = value;

                if (value)
                {
                    IsWriteMatchedChecked = false;
                    IsWriteSelChecked = false;
                }

                _writeType = WriteType.All;

                OnPropertyChanged("IsWriteAllChecked");
            }
        }

        private bool _isWriteSel = false;
        public bool IsWriteSelChecked
        {
            get { return _isWriteSel; }
            set
            {
                _isWriteSel = value;
                if (value)
                {
                    IsWriteMatchedChecked = false;
                    IsWriteAllChecked = false;
                }

                _writeType = WriteType.Selected;

                OnPropertyChanged("IsWriteSelChecked");
            }
        }

        private bool _isWriteMatched = false;
        public bool IsWriteMatchedChecked
        {
            get { return _isWriteMatched; }
            set
            {
                _isWriteMatched = value;
                if (value)
                {
                    IsWriteAllChecked = false;
                    IsWriteSelChecked = false;
                }
                _writeType = WriteType.Matched;

                OnPropertyChanged("IsWriteMatchedChecked");
            }
        }

        private bool _isInputUser = false;

        public bool IsInputUser
        {
            get { return _isInputUser; }
            set { 
                _isInputUser = value;
                OnPropertyChanged("IsInputUser");
            }
        }

        private Visibility _menuItemVisibility;
        public Visibility MenuItemVisibility
        {
            get { return _menuItemVisibility; }
            set {
                _menuItemVisibility = value;
                //_columnVisibility = Mode == WindowMode.library ? Visibility.Hidden : Visibility.Visible;
                OnPropertyChanged("ColumnVisibility");
            }
        }
        private ObservableCollection<ContextAction> _selectedLinesContextItems;

        public ObservableCollection<ContextAction> SelectedLinesContextItems
        {
            get { return _selectedLinesContextItems; }
            set
            { _selectedLinesContextItems = value; }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindowViewModel(IDialogService dialogService, IFileDialogService fileDialogService, Matches matches)
        {
            
            library = new ICRPData();
            this.matches = matches;
            InitilizeCommands();

            _lowerEnergy = Properties.Settings.Default.LOWERENERGY;
            _upperEnergy = Properties.Settings.Default.UPPERENERGY;
            _lowerYield = Properties.Settings.Default.LOWERYEILD;

            matches.YeildLimit = _lowerYield;

            _xrayFilter = true;
            _gammaFilter = true;
            _daughtersFilter = true;

            this.dialogService = dialogService;
            this.fileDialogService = fileDialogService;

            _selectedLines = new ArrayList();
            _nuclides = matches.Nuclides.DefaultView;
            _nuclides.Sort = "SCORE DESC";
            _lines = matches.Lines.DefaultView;


            SetLinesFilter();

        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindowViewModel(IDialogService dialogService, IFileDialogService fileDialogService)
        {

            library = new ICRPData();

            matches = new Matches(library);

            InitilizeCommands();

            _lowerEnergy = Properties.Settings.Default.LOWERENERGY;
            _upperEnergy = Properties.Settings.Default.UPPERENERGY;
            _lowerYield = Properties.Settings.Default.LOWERYEILD;

            matches.YeildLimit = _lowerYield;

            _xrayFilter = true;
            _gammaFilter = true;
            _daughtersFilter = true;

            this.dialogService = dialogService;
            this.fileDialogService = fileDialogService;

            _selectedLines = new ArrayList();
            _nuclides = matches.Nuclides.DefaultView;
            _nuclides.Sort = "SCORE DESC";
            _lines = matches.Lines.DefaultView;

            _selectedLinesContextItems = new ObservableCollection<ContextAction>()
            {
                new ContextAction { Name = $"Write Selected",  Action = LinesContextMenuCommand },
                new ContextAction { Name = $"Write All",  Action = LinesContextMenuCommand },
            };
            
            SetLinesFilter();

        }
        protected void InitilizeSelectedLinesContextMenu() 
        {
            _selectedLinesContextItems = new ObservableCollection<ContextAction>()
            {
                new ContextAction { Name = $"Write Selected",  Action = LinesContextMenuCommand },
                new ContextAction { Name = $"Write All",  Action = LinesContextMenuCommand },
            };
        }
        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        /// <summary>
        /// Set the Lines Filter
        /// </summary>
        protected virtual void SetLinesFilter()
        {
            Lines.RowFilter = "ENERGY > " + _lowerEnergy + " AND ENERGY < " + _upperEnergy
                + " AND YIELD > " + _lowerYield +
                (_gammaFilter ? "" : " AND TYPE ='G'") +
                (_xrayFilter ? "" : " AND TYPE = 'X'") +
                (!_daughtersFilter && SelectedNuclide != null ? " AND NAME = '" + SelectedNuclide.Row["NAME"] + "'" : "");

            //always set the lower limit
            matches.YeildLimit = _lowerYield;

            //save the lines filters as persistent user settings
            Properties.Settings.Default.LOWERENERGY = _lowerEnergy;
            Properties.Settings.Default.UPPERENERGY = _upperEnergy;
            Properties.Settings.Default.LOWERYEILD = _lowerYield;
            Properties.Settings.Default.Save();
        }

        protected virtual void GetLines() 
        {
            if (!CanGetLines())
                return;
            //_linesSort = _lines.Sort;
            SetLinesFilter();
            matches.SetLines(SelectedNuclide.Row);
           // _lines.Sort = _linesSort;
        }

        protected virtual string GetLibraryFilename(FileOperation operation) 
        {
            fileDialogService.IntialDirectory = @"C:\GENIE2K\CAMFILES";
            fileDialogService.Filter = "(*.nlb)|*.nlb|(*.cnf)|*.cnf|(*.tlb)|*.tlb|All Files (*.*)|*.*";
            fileDialogService.DefaultExt = ".nlb";
            return fileDialogService.OpenFileDialog(operation);
           
        }
        /// <summary>
        /// Load the data or creates a new file to save the library
        /// </summary>
        protected virtual async Task GetLibraryFileAsync(FileOperation operation)
        {
            //put the defualts in to make browsing eaiser
            string fileName = GetLibraryFilename(operation);
            bool overwrite = operation == FileOperation.New;
            if (fileName != null)
            {
                matches.ClearPersistentMatches();
                await GetLibrayDataAsync(fileName, overwrite);
            }

        }

        protected async Task GetLibrayDataAsync(string fileName, bool overwrite = false) 
        {
            if (String.IsNullOrWhiteSpace(fileName))
                return;

            try
            {
                Type libType = SpectralLibraryGenerator.GetSpectralDataType(fileName);
                //create an instance so we can actually use it
                libGen = Activator.CreateInstance(libType, new string[] { fileName }) as SpectralLibraryGenerator;

                //initilize the library data object
                if(!overwrite)
                    await libGen.LoadLibraryAsync(fileName);

                LibraryFile = fileName;
                //FillWithLibrary();

                //OnPropertyChanged("LibraryNuclides");
            }
            catch (Exception ex)
            {
                DialogViewModel dialogViewModel = new DialogViewModel(ex.Message, "Exception", true);
                dialogService.ShowDialog(dialogViewModel);
            }

        }
        /// <summary>
        /// Get the Lines associated with the select nuclide
        /// </summary>
        protected bool CanGetLines()
        {
            if (matches == null)
                return false;
            if (matches.Nuclides.Rows.Count < 1)
                return false;

            return true;
        }
        /// <summary>
        /// Write the Lines to Library
        /// </summary>
        /// <param name="type">Write Type</param>
        protected virtual void WriteLines(WriteType type)
        {
            ////check if a nuclide is selected
            //if (matchedNuclidesGrid.SelectedRows.Count <= 0)
            //    return;

            try
            {
                //get the nuclides to write to the library
                DataRow nuc = SelectedNuclide.Row;
                bool isPhotoPeak = (string)nuc["NAME"] != "S.E." && (string)nuc["NAME"] != "D.E." && !((string)nuc["NAME"]).Contains("Sum");

                //check if the nuclide has a half-life that is too long
                if (nuc["HALF_LIFE_UNIT"].ToString().Equals("y") && (double)nuc["HALF_LIFE"] / (31557600*1E6) > Int32.MaxValue)
                    throw new ArgumentException($"The Half-life for "+ (string)nuc["NAME"] + " is too large to be supported. It will not be added to the library" );

                //get the lines to write to the library
                    if (isPhotoPeak)
                {
                    DataTable writeLines;
                    switch (type)
                    {
                        case WriteType.All:
                            writeLines = Lines.ToTable();
                            break;
                        case WriteType.Matched:
                            writeLines = Lines.ToTable().Select("MATCHED = True").CopyToDataTable();
                            break;
                        case WriteType.Selected:
                            writeLines = matches.Lines.Clone();
                            foreach (DataRowView line in SelectedLines)
                                writeLines.ImportRow(line.Row);
                            break;
                        default:
                            writeLines = matches.Lines.Select().CopyToDataTable();
                            break;
                    }
                    //write the nuclides to the library
                    libGen.WriteNuclide(nuc, writeLines, CombineLinesCallback);
                }
                //set the persistent matches
                matches.SetPersistentMatches();

            }
            catch (Exception ex)
            {
                DialogViewModel dialogViewModel = new DialogViewModel($"There was an exeption while writing to the library:\n {ex.Message}", "Library Exception", true);
                dialogService.ShowDialog(dialogViewModel);
            }

        }

        private bool? CombineLinesCallback(string energies)
        {
            DialogViewModel dialogViewModel = new DialogViewModel($"Lines of energies:  { energies } keV are possibly unresovable, do you wish to combine them?", "Possible Unresolvable Lines", false, true);
            return dialogService.ShowDialog(dialogViewModel);

        }

        private void SaveLibrary() 
        {
            try
            {
                libGen.SaveFile();
                this.LibraryFile = libGen.FileName;
            }
            catch (Exception ex)
            {
                DialogViewModel dialogViewModel = new DialogViewModel($"There was an exeption while writing to the library:\n{ex.Message}", "Library Exception", true);
                dialogService.ShowDialog(dialogViewModel);
            }
        }
    

        #region Commands


        //public RelayCommand MenuCommand { get; protected set; }
        public RelayCommand FileMenuCommand { get; protected set; }
        public RelayCommand LinesMenuCommand { get; protected set; }
        public RelayCommand SettingsMenuCommand { get; protected set; }
        public RelayCommand OtherMenuCommands { get; protected set; }
        public RelayCommand ModeCommand { get; protected set; }
        public RelayCommand ExitCommand { get; private set; }
        public RelayCommand UserInputCommand { get; protected set; }
        public RelayCommand MatchesContextMenuCommand { get; protected set; }
        public RelayCommand PastedTextCommand { get; protected set; }
        public RelayCommand LinesContextMenuCommand { get; protected set; }
        private void InitilizeCommands() 
        {
            ExitCommand = new RelayCommand(ExitCommand_Executed, CanExitExecute);
            ModeCommand = new RelayCommand(ModeMenuCommand_Executed, CanModeMenuExecute);
            OtherMenuCommands = new RelayCommand(OtherMenuCommand_Executed, CanOtherMenuExecute);
            
        }

        protected virtual async void FileMenuCommand_Executed(object context)
        {
            string menuName = context.ToString().ToLowerInvariant();

            if (menuName.Contains("open") && menuName.Contains("library"))
                await GetLibraryFileAsync(FileOperation.Open);
            else if (menuName.Contains("new") && menuName.Contains("library"))
                await GetLibraryFileAsync(FileOperation.New);
            else if (menuName.Contains("as") && menuName.Contains("library"))
            {
                libGen.FileName = GetLibraryFilename(FileOperation.New);

                if(libGen != null)
                    SaveLibrary();
            }
            else if (menuName.Contains("save") && menuName.Contains("library"))
            {
                SaveLibrary();
            }

                
        }
        protected virtual bool CanFileMenuExecute(object context)
        {
            string menuName = context.ToString().ToLowerInvariant();
            if (menuName.Contains("save"))
                return LibraryFile != null;
            if (menuName.Contains("library") && (menuName.Contains("new") || menuName.Contains("open")))
                return true;

            return false;
        }

        protected virtual void LinesMenuCommand_Executed(object context) 
        {
            string menuName = context.ToString().ToLowerInvariant();
            if (menuName.Contains("write") && menuName.Contains("all"))
            {
                WriteLines(WriteType.All);
                IsWriteAllChecked = true;
            }
            else if (menuName.Contains("write") && menuName.Contains("selected"))
            {
                WriteLines(WriteType.Selected);
                IsWriteSelChecked = true;
            }
            else if (menuName.Contains("write"))
                WriteLines(_writeType);
        }

        protected virtual bool CanLinesMenuExecute(object context) 
        {
            string menuName = context.ToString().ToLowerInvariant();

            if (menuName.Contains("all"))
                return SelectedNuclide != null && LibraryFile != null;
            else if (menuName.Contains("selected"))
                return SelectedLines != null && LibraryFile != null;
            else if (menuName.Contains("write"))
                return SelectedNuclide != null && LibraryFile != null &&
                    (IsWriteAllChecked || IsWriteMatchedChecked || IsWriteSelChecked);

            return false;
        }

        protected virtual void SettingsMenuCommand_Executed(object context)
        {
            string menuName = context.ToString().ToLowerInvariant();
            if (menuName.Contains("matching"))
            {
                MatchSettingsViewModel dialogViewModel = new MatchSettingsViewModel(matches);
                dialogService.ShowDialog(dialogViewModel);
            }
            else if (menuName.Contains("library"))
            {
                LibrarySettingsViewModel dialogViewModel = new LibrarySettingsViewModel(libGen);
                dialogService.ShowDialog(dialogViewModel);
            }
        }
        protected virtual bool CanSettingsMenuExecute(object context)
        {
            string menuName = context.ToString().ToLowerInvariant();

            if (menuName.Contains("matching"))
                return matches != null;
            if (menuName.Contains("library"))
                return libGen != null;

            return false; 
        }

        private void ModeMenuCommand_Executed(object context)
        {
            //string menuName = context.ToString().ToLowerInvariant();
            Window win = context as Window;

            MainWindowViewModel model;
            if (win.DataContext.GetType() == typeof(MainWindowLibraryViewModel))
            {
                model = new MainWindowMatchingViewModel(dialogService, fileDialogService, matches);
            }
            else
            {
                model = new MainWindowLibraryViewModel(dialogService, fileDialogService, matches);
            }
            if (libGen != null) 
            { 
                model.libGen = libGen;
                model.LibraryFile = libGen.FileName;
            }
            if (specdata != null)
            {
                model.specdata = specdata;
                //model.LibraryFile = libGen.FileName;
            }
            CurrentModeViewModel = model;
            win.DataContext = CurrentModeViewModel;
        }
        private bool CanModeMenuExecute(object context) 
        {
            return true;
        }

        private void ExitCommand_Executed(object context)
        {

            if (libGen != null)
            {
                DialogViewModel dialogViewModel = new DialogViewModel($"Do you wisth to save {libGen.FileName} before exiting?", "Exit", false, true);
                bool? dialogResult = dialogService.ShowDialog(dialogViewModel);
                if (dialogResult == true)
                    libGen.SaveFile();
                else if (dialogResult == false)
                    return;
            }

            Window win = context as Window;
            win.Close();
        }

        private bool CanExitExecute(object context)
        {
            return true;
        }
        protected virtual void PasteCommand_Executed(object context)
        {

        }

        private async void OtherMenuCommand_Executed(object context)
        {
            string menuName = context.ToString().ToLowerInvariant();
            if (menuName.Contains("rebuild"))
            {
                DialogViewModel dialogViewModel = new DialogViewModel("Are you sure you want to rebuild the library database? This operation may take several hours", "Confirmation", false,false);
                if (dialogService.ShowDialog(dialogViewModel) == true)
                {
                    await library.InitializeAsync();
                    library.WriteToDatabase();
                }
            }
            else if (menuName.Contains("about"))
            {
                StringBuilder message = new StringBuilder();
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();

                object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                foreach (AssemblyProductAttribute attrbute in attributes)
                {
                    message.AppendLine(attrbute.Product);
                    message.AppendLine();
                }
                object[] version = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
                foreach (AssemblyFileVersionAttribute attrbute in version)
                {
                    message.AppendLine("Version: "+attrbute.Version);
                    message.AppendLine();
                }
                object[] copyright = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                foreach (AssemblyCopyrightAttribute attrbute in copyright)
                {
                    message.AppendLine(attrbute.Copyright);
                    message.AppendLine();
                }
                object[] company = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                foreach (AssemblyCompanyAttribute attrbute in company)
                {
                    message.AppendLine(attrbute.Company);
                    message.AppendLine();
                }

                object[] description = assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                foreach (AssemblyDescriptionAttribute attrbute in description)
                {
                    int chunkSize = 60;
                    IEnumerable chunkString = Enumerable.Range(0, attrbute.Description.Length / chunkSize)
                        .Select(i => attrbute.Description.Substring(i * chunkSize, chunkSize));
                    
                    foreach(string chunk in chunkString)
                    {  
                        message.AppendLine(chunk);
                    }
                       
                }

                DialogViewModel dialogViewModel = new DialogViewModel(message.ToString(), "About", false);
                dialogService.ShowDialog(dialogViewModel);
            }
        }
        private bool CanOtherMenuExecute(object context)
        {
            string menuName = context.ToString().ToLowerInvariant();
            if (menuName.Contains("rebuild"))
                return library != null;
            return true;

        }
        protected virtual bool CanPasteExecute(object context)
        {
            return true && Clipboard.ContainsText();
        }

        public async virtual Task OnFileDrop(string[] items)
        {
            if (items.Length > 0)
                await GetLibrayDataAsync(items[0]);
        }

        protected virtual bool CanLinesContextMenuExecute(object context)
        {
            if (context == null)
                return false;

            return CanLinesMenuExecute(context);
        }

        protected virtual void LinesContextMenuCommand_Executed(object context)
        {
            //string menuName = context.ToString().ToLowerInvariant();
            //if (menuName.Contains("selected"))
            //{
            //    foreach (DataRowView line in SelectedLines)
            //    {
            //        line["MATCHED"] = false;
            //        libGen.ClearLines((double)line["ENERGY"]);
            //    }
            //}
            //else
            //{
                LinesMenuCommand_Executed(context);
            //}

        }
    }


    #endregion

    public class ContextAction 
    {
        public string Name { get; set; }
        public ICommand Action { get; set; }

    }
}
