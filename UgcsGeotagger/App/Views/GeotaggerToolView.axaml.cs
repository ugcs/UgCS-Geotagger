using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using UgCSGeotagger.Models;
using UgCSGeotagger.ViewModels;

namespace UgCSGeotagger.Views
{
    public class GeotaggerToolView : UserControl
    {
        public GeotaggerToolView()
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
            var dragDropPanel = e.Source as Control;
            var nameOfPanel = dragDropPanel?.Tag;
            if (nameOfPanel != null)
            {
                var files = e.Data.GetFileNames();
                var dataContext = DataContext as GeotaggerToolViewModel;
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
        }
    }
}