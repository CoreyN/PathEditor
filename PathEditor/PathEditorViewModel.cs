using Prism.Commands;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace PathEditor
{
    class PathEditorViewModel : ViewModelBase
    {
        ObservableCollection<string> _systemPath;
        PathEditorService _pathEditorService;
        int _selectedIndex;
        string _newPath = "";
        bool _dirty = false;

        public ObservableCollection<string> SystemPath { get { return _systemPath; } }
        public string NewPath
        {
            get
            {
                return _newPath;
            }
            set
            {
                _newPath = value;
                (AddCommand as DelegateCommand).RaiseCanExecuteChanged();
                NotifyPropertyChanged(nameof(NewPath));
            }
        }

        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                _selectedIndex = value;
                (RemoveCommand as DelegateCommand).RaiseCanExecuteChanged();
                NotifyPropertyChanged(nameof(SelectedIndex));
            }
        }

        public ICommand AddCommand { get; set; }
        public ICommand RemoveCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        public PathEditorViewModel(PathEditorService pathEditorService)
        {
            _pathEditorService = pathEditorService;

            var systemPath = _pathEditorService.GetSystemPath();
            _systemPath = new ObservableCollection<string>(systemPath);

            AddCommand = new DelegateCommand(OnAdd, CanAdd);
            RemoveCommand = new DelegateCommand(OnRemove, CanRemove);
            SaveCommand = new DelegateCommand(OnSave, CanSave);
        }

        public void OnAdd()
        {
            SystemPath.Insert(0, NewPath);
            SetDirty();
            NewPath = "";
        }

        public bool CanAdd()
        {
            return !NewPathAlreadyInSystemPath() && DirectoryExists() && CanWritePath();
        }

        public void OnRemove()
        {
            SystemPath.RemoveAt(SelectedIndex);
            SetDirty();
        }

        public bool CanRemove()
        {
            return ItemIsSelected() && CanWritePath();
        }

        public void OnSave()
        {
            _pathEditorService.SetSystemPath(SystemPath);
            NewPath = "";
            SetNotDirty();
        }

        public bool CanSave()
        {
            return _dirty && CanWritePath();
        }

        public void SetDirty()
        {
            _dirty = true;
            (SaveCommand as DelegateCommand).RaiseCanExecuteChanged();
        }

        private void SetNotDirty()
        {
            _dirty = false;
            (SaveCommand as DelegateCommand).RaiseCanExecuteChanged();
        }

        private bool NewPathAlreadyInSystemPath()
        {
            return SystemPath.Any(p => p.ToLower() == NewPath.ToLower());
        }

        private bool DirectoryExists()
        {
            return _pathEditorService.DirectoryExists(NewPath);
        }

        private bool CanWritePath()
        {
            return !_pathEditorService.IsReadOnly;
        }

        private bool ItemIsSelected()
        {
            return SelectedIndex >= 0 && SelectedIndex < SystemPath.Count;
        }
    }
}
