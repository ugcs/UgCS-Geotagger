using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using UgCSPPK.Models;
using UgCSPPK.ViewModels;

namespace UgCSPPK.Views
{
    public class PpkToolView : UserControl
    {
        public PpkToolView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            AddHandler(DragDrop.DropEvent, OnDrop);
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            this.Cursor = new Cursor(StandardCursorType.Wait);
            var dragDropPanel = e.Source as Control;
            var nameOfPanel = dragDropPanel?.Tag;
            if (nameOfPanel != null)
            {
                var files = e.Data.GetFileNames();
                var dataContext = DataContext as PpkToolViewModel;
                switch (nameOfPanel)
                {
                    case DataFile.PositionSolutionFileAbbr:
                        dataContext.AddFiles(files, DataFile.PositionSolutionFileAbbr);
                        break;

                    case DataFile.FileToUpdateAbbr:
                        dataContext.AddFiles(files, DataFile.FileToUpdateAbbr);
                        break;

                    default:
                        break;
                }
            }
            this.Cursor = new Cursor(StandardCursorType.Arrow);
        }
    }
}