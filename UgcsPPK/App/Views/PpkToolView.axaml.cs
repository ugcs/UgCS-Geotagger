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
            if (e.Source is StackPanel dragDropPanel)
            {
                var files = e.Data.GetFileNames();
                var dataContext = DataContext as PpkToolViewModel;
                switch (dragDropPanel.Name)
                {
                    case DataFile.PositionSolutionFileAbbr:
                        dataContext.AddFiles(files, DataFile.PositionSolutionFileAbbr);
                        break;

                    case DataFile.FileToUpdateAbbr:
                        dataContext.AddFiles(files, DataFile.FileToUpdateAbbr);
                        break;

                    default:
                        return;
                }
            }
        }
    }
}