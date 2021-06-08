using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;

namespace UgCSGeotagger.Views
{
    public class MessageBoxView : Window
    {
        public enum MessageBoxButtons
        {
            Ok,
            OkCancel,
            YesNo,
            YesNoCancel
        }

        public enum MessageBoxResult
        {
            Ok,
            Cancel,
            Yes,
            No
        }

        public MessageBoxView()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static Task<MessageBoxResult> Show(Window parent, string text, string title, MessageBoxButtons buttons)
        {
            var msgbox = new MessageBoxView()
            {
                Title = title
            };
            msgbox.FindControl<TextBlock>("DialogText").Text = text;
            var res = MessageBoxResult.Ok;
            if (buttons == MessageBoxButtons.Ok || buttons == MessageBoxButtons.OkCancel)
            {
                msgbox.FindControl<Button>("Ok").Click += (_, __) =>
                {
                    msgbox.Close();
                };
            }

            var tcs = new TaskCompletionSource<MessageBoxResult>();
            msgbox.Closed += delegate { tcs.TrySetResult(res); };
            if (parent != null)
                msgbox.ShowDialog(parent);
            else msgbox.Show();
            return tcs.Task;
        }
    }
}