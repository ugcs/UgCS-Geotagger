using Avalonia.Controls;
using Avalonia.Markup.Xaml;

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
        }
    }
}