using System;
using PeakMapWPF.Commands;
using System.Windows.Input;

namespace PeakMapWPF.ViewModels
{
    class YieldSelectorViewModel : IDialogRequestClose
    {
        public event EventHandler<DialogCloseRequestEventArgs> CloseRequested;

        public YieldSelectorViewModel()
        {

            OkCommand = new RelayCommand(P => CloseRequested?.Invoke(this, new DialogCloseRequestEventArgs(true)));
            CancelCommand = new RelayCommand(P => CloseRequested?.Invoke(this, new DialogCloseRequestEventArgs(false)));

            _minYield = 1.00;
        }

        private double _minYield;

        public double MinimumYield
        {
            get { return _minYield; }
            set { _minYield = value; }
        }


        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

    }

}
