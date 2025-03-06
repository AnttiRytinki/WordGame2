namespace BrainStorm
{
    public class ChatBox
    {
        public ChatBox()
        {
        }

        public System.Windows.Controls.TextBox ShowGamePoints(System.Windows.Controls.TextBox previousTextBox, int myPoints, int oppPoints)
        {
            System.Windows.Controls.TextBox newTextBox = previousTextBox;
            newTextBox.Text = "";

            newTextBox = Add(newTextBox, "Me: " + myPoints.ToString());
            newTextBox = Add(newTextBox, "Opp: " + oppPoints.ToString());

            return newTextBox;
        }

        /// <summary>
        /// Add text to previousTextBox and only display the three last entries
        /// </summary>
        public System.Windows.Controls.TextBox Add(System.Windows.Controls.TextBox previousTextBox, string text)
        {
            System.Windows.Controls.TextBox newTextBox = previousTextBox;

            int newlineCount = newTextBox.Text.Count(f => f == '\n');

            if (newlineCount > 2)
            {
                int firstNewlineIndex = newTextBox.Text.IndexOf("\n");
                newTextBox.Text = newTextBox.Text.Substring(firstNewlineIndex + 1);
            }

            newTextBox.Text += text + "\n";

            return newTextBox;
        }
    }
}
