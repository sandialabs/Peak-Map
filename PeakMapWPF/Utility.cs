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

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PeakMapWPF
{
    public enum FileOperation { New, Open };
    #region DialogServices
    public interface IDialog
    {
        object DataContext { get; set; }
        bool? DialogResult { get; set; }
        Window Owner { get; set; }
        void Close();
        bool? ShowDialog();
    }

    public interface IDialogService
    {
        void Register<TViewModel, TView>() where TViewModel : IDialogRequestClose where TView : IDialog;

        bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : IDialogRequestClose;
    }
    public interface IFileDialogService
    {

        string Filter { get; set; }
        string IntialDirectory { get; set; }
        string DefaultExt { get; set; }

        string OpenFileDialog(FileOperation operation);
    }

    public interface IDialogRequestClose
    {
        event EventHandler<DialogCloseRequestEventArgs> CloseRequested;
    }

    public class DialogCloseRequestEventArgs : EventArgs
    {
        public bool? DialogResult { get; }

        public DialogCloseRequestEventArgs(bool? dialogResult)
        {
            DialogResult = dialogResult;
        }
    }

    public class DialogService : IDialogService
    {
        private readonly Window owner;
        public IDictionary<Type, Type> Mappings { get; }
        public DialogService(Window owner)
        {
            this.owner = owner;
            Mappings = new Dictionary<Type, Type>();
        }

        public void Register<TViewModel, TView>()
            where TViewModel : IDialogRequestClose
            where TView : IDialog
        {
            if (Mappings.ContainsKey(typeof(TViewModel)))
            {
                throw new ArgumentException($"Type  {typeof(TViewModel)} is already mapped to type {typeof(TView)}");
            }
            Mappings.Add(typeof(TViewModel), typeof(TView));
        }

        public bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : IDialogRequestClose
        {
            Type viewType = Mappings[typeof(TViewModel)];

            IDialog dialog = (IDialog)Activator.CreateInstance(viewType);

            EventHandler<DialogCloseRequestEventArgs> handler = null;

            handler = (sender, e) =>
            {
                viewModel.CloseRequested -= handler;

                if (e.DialogResult.HasValue)
                {
                    dialog.DialogResult = e.DialogResult;
                }
                else
                {
                    dialog.Close();
                }
            };
            viewModel.CloseRequested += handler;

            dialog.DataContext = viewModel;
            dialog.Owner = owner;

            return dialog.ShowDialog();
        }
    }
    public class FileDialogService : IFileDialogService
    {
        private string _filter;
        private string _initialDirectory;
        private readonly Window owner;
        public string Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }

        public string IntialDirectory
        {
            get { return _initialDirectory; }
            set { _initialDirectory = value; }
        }
        private string _defualtExt;

        public string DefaultExt
        {
            get { return _defualtExt; }
            set { _defualtExt = value; }
        }


        public FileDialogService(Window owner)
        {
            this.owner = owner;
        }
        public string OpenFileDialog(FileOperation operation)
        {

            Type fileDialogType;
            switch (operation)
            {
                case FileOperation.New:
                    fileDialogType = typeof(SaveFileDialog);
                    break;
                case FileOperation.Open:
                    fileDialogType = typeof(OpenFileDialog);
                    break;
                default:
                    return null;
            }

            FileDialog fileDialog = (FileDialog)Activator.CreateInstance(fileDialogType);
            fileDialog.Filter = _filter;
            fileDialog.InitialDirectory = _initialDirectory;
            fileDialog.DefaultExt = _defualtExt;


            if (fileDialog.ShowDialog(owner) == true)
            {
                return fileDialog.FileName;
            }
            else
            {
                return null;
            }
        }
    }
    #endregion
    #region DragandDropSerivce

    public interface IFileDragDropTarget
    {
        System.Threading.Tasks.Task OnFileDrop(string[] items);
    }

    public class FileDragDropService
    {
        public static bool GetIsFileDragDropEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFileDragDropEnabledProperty);
        }

        public static void SetIsFileDragDropEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFileDragDropEnabledProperty, value);
        }

        public static bool GetFileDragDropTarget(DependencyObject obj)
        {
            return (bool)obj.GetValue(FileDragDropTargetProperty);
        }

        public static void SetFileDragDropTarget(DependencyObject obj, bool value)
        {
            obj.SetValue(FileDragDropTargetProperty, value);
        }

        public static readonly DependencyProperty IsFileDragDropEnabledProperty =
                DependencyProperty.RegisterAttached("IsFileDragDropEnabled", typeof(bool), typeof(FileDragDropService), new PropertyMetadata(OnFileDragDropEnabled));

        public static readonly DependencyProperty FileDragDropTargetProperty =
                DependencyProperty.RegisterAttached("FileDragDropTarget", typeof(object), typeof(FileDragDropService), null);

        private static void OnFileDragDropEnabled(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == e.OldValue) return;
            if (d is Control control) control.Drop += OnDrop;
        }

        private static void OnDrop(object _sender, DragEventArgs _dragEventArgs)
        {
            if (!(_sender is DependencyObject d)) return;
            Object target = d.GetValue(FileDragDropTargetProperty);
            if (target is IFileDragDropTarget fileTarget)
            {
                if (_dragEventArgs.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    fileTarget.OnFileDrop((string[])_dragEventArgs.Data.GetData(DataFormats.FileDrop));
                }
            }
            else
            {
                throw new Exception("FileDragDropTarget object must be of type IFileDragDropTarget");
            }
        }
    }
    #endregion
    #region DataPiping

    public class DataPiping
    {
        public static readonly DependencyProperty DataPipeProperty = DependencyProperty.RegisterAttached(
            "DataPipe", typeof(DataPipeCollection), typeof(DataPiping), new UIPropertyMetadata(null));

        public static void SetDataPipes(DependencyObject obj, DataPipeCollection pipeCollection) 
        {
            obj.SetValue(DataPipeProperty, pipeCollection);
        }

        public static DataPipeCollection GetDataPipes(DependencyObject obj)
        {
            return (DataPipeCollection)obj.GetValue(DataPipeProperty);
        }
    }
    public class DataPipeCollection : FreezableCollection<DataPipe> { }

    public class DataPipe : Freezable
    {
        public object Source
        {
            get { return (object)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof(object), typeof(DataPipe), 
            new FrameworkPropertyMetadata(null,new PropertyChangedCallback(OnSourceChanged)));

        private static void OnSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((DataPipe)obj).OnSourceChanged(e);
        }

        protected virtual void OnSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            Target = e.NewValue;
        }

        public object Target {
            get { return (object)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }
        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(
            "Target", typeof(object), typeof(DataPipe), new FrameworkPropertyMetadata(null));

        protected override Freezable CreateInstanceCore()
        {
            return new DataPipe();
        }
    }
    #endregion
    #region RelayCommand
    public class RelayCommand : ICommand
    {

        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            _execute = execute ?? throw new NullReferenceException("No action defined");
            _canExecute = canExecute;

        }
        public RelayCommand(Action<object> execute) : this(execute, null) { }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute.Invoke(parameter);
        }
    }
    #endregion
}






