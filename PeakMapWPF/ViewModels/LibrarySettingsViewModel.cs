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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PeakMap;


namespace PeakMapWPF.ViewModels
{
    class LibrarySettingsViewModel : IDialogRequestClose, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<DialogCloseRequestEventArgs> CloseRequested;

        public ICommand OkCommand { get; }

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private readonly SpectralLibraryGenerator libraryGenerator;

        public LibrarySettingsViewModel(SpectralLibraryGenerator libraryGenerator) 
        {
            this.libraryGenerator = libraryGenerator;
            OkCommand = new RelayCommand(P => CloseRequested?.Invoke(this, new DialogCloseRequestEventArgs(true)));
        }

        public bool LineCombination 
        { 
            get { return libraryGenerator.PerfomLineCombination; }
            set
            {
                libraryGenerator.PerfomLineCombination = value;
                OnPropertyChanged("LineCombination");
            }
        }

        public double ResolutionLimit 
        {
            get { return libraryGenerator.ResolutionLimit; }
            set
            {
                libraryGenerator.ResolutionLimit = value;
                OnPropertyChanged("ResolutionLimit");
            }
        }

        public double  KeyLineInterference 
        {
            get { return libraryGenerator.KeyInterferenceLimit; }
            set
            {
                libraryGenerator.KeyInterferenceLimit = value;
                OnPropertyChanged("KeyLineInterference");
            }
        }
    }
}
