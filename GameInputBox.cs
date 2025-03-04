using System.Windows.Media;

namespace BrainStorm
{
    public class GameInputBox
    {
        public SolidColorBrush Brush { get; set; } = Brushes.White;
        public string Text { get; set; } = "";
        public bool IsEnabled { get; set; } = true;

        public GameInputBox()
        {
        }

        public GameInputBox(SolidColorBrush brush, string text, bool isEnabled)
        {
            Brush = brush;
            Text = text;
            IsEnabled = isEnabled;
        }

        public void MyTurn()
        {
            Brush = Brushes.White;
            Text = "";
            IsEnabled = true;
        }

        public void OppTurn()
        {
            Brush = Brushes.Red;
            Text = "";
            IsEnabled = false;
        }

        public System.Windows.Controls.TextBox UpdateTextBox(System.Windows.Controls.TextBox previousTextBox)
        {
            System.Windows.Controls.TextBox newTextBox = previousTextBox;
            newTextBox.Background = Brush;
            newTextBox.Text = Text;
            newTextBox.IsEnabled = IsEnabled;

            return newTextBox;
        }
    }
}
