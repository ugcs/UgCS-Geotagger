using ReactiveUI;
using UgCSGeotagger.ViewModels;

namespace App.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase _content;

        public ViewModelBase Content
        {
            get => _content;
            private set => this.RaiseAndSetIfChanged(ref _content, value);
        }

        public MainWindowViewModel()
        {
            Content = new GeotaggerToolViewModel();
        }
    }
}