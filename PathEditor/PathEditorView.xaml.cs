using System.Windows;

namespace PathEditor
{
    public partial class PathEditorView : Window
    {
        public PathEditorView()
        {
            DataContext = new PathEditorViewModel(new PathEditorService());
            InitializeComponent();
        }
    }
}
