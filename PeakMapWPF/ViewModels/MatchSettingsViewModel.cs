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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;


namespace PeakMapWPF.ViewModels
{
    class MatchSettingsViewModel : INotifyPropertyChanged, IDialogRequestClose
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

        private Matches matches;

        

        public MatchSettingsViewModel(Matches matches)
        {
            this.matches = matches;
            OkCommand = new RelayCommand(P => CloseRequested?.Invoke(this, new DialogCloseRequestEventArgs(true)));
        }


        #region properites
        
        public bool EnableHalfLifeScore
        { 
            get { return matches.EnableHalfLifeScore; }
            set 
            { 
                matches.EnableHalfLifeScore = value;
                OnPropertyChanged("EnableHalfLifeScore");
            } 
        }

        public double HalfLifeConstant 
        { 
            get { return matches.HalfLifeScoreConstant; }
            set 
            { 
                matches.HalfLifeScoreConstant = value;
                OnPropertyChanged("HalfLifeConstant");
            }
        }
        public double LineDeviationConstant
        {
            get { return matches.LineDeviationContant; }
            set 
            {
                matches.LineDeviationContant = value;
                OnPropertyChanged("LineDeviationConstant");
            }
        }

        public double SumPeakPenalty
        { 
            get { return matches.SumPeakPenalty; }
            set 
            {
                matches.SumPeakPenalty = value;
                OnPropertyChanged("SumPeakPenalty");
            }
        }
        public double UnmatchedLineConstant 
        {
            get { return matches.UnmatchedLineConstant; }
            set
            {
                matches.UnmatchedLineConstant = value;
                OnPropertyChanged("UnmatchedLineConstant");
            }
        }
        public double ParentDaughterRatio 
        {
            get { return matches.PDHalfLifeRatio; } 
            set
            {
                matches.PDHalfLifeRatio = value;
                OnPropertyChanged("ParentDaughterRatio");
            }
        }
        public double ScoreLimit
        {
            get { return matches.ScoreLimit; }
            set
            {
                matches.ScoreLimit = value;
                OnPropertyChanged("ScoreLimit");
            }
        }
        public double YieldLimit
        {
            get { return matches.YeildLimit; }
            set 
            {
                matches.YeildLimit = value;
                OnPropertyChanged("YieldLimit");
            }
        }
        #endregion
    }
}
