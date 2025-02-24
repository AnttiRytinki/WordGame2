using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace OrdSpel2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public GameState GameState { get; set; } = new GameState();
        public GameCom GameCom { get; set; } = new GameCom();

        int _lastClickedX = 0;
        int _lastClickedY = 0;

        bool _buttonsEnabled = false;

        public MainWindow()
        {
            InitializeComponent();

            SetButtonProperties();
            SetAllButtons(false);
        }

        private void SetAllButtons(bool enabled)
        {
            _buttonsEnabled = enabled;
        }

        private void Button_Click(object sender, MouseButtonEventArgs e)
        {
            //if (e.LeftButton == MouseButtonState.Pressed)
            //    ;

            //else if (e.RightButton == MouseButtonState.Pressed)
            //    ;

            //else
            //    return;

            if (_buttonsEnabled == false)
                return;

            inputBox.Background = Brushes.Red;
            inputBox.Text = "";
            inputBox.IsEnabled = false;

            string buttonName = ((Button)sender).Name;
            int x = int.Parse(buttonName[1].ToString());
            int y = int.Parse(buttonName[2].ToString());

            _lastClickedX = x;
            _lastClickedY = y;

            SetAllButtons(false);
            bool wordWasRevealed = false;
            bool letterWasRevealed = GameState.Reveal(x, y, out wordWasRevealed);

            if (letterWasRevealed)
            {
                inputBox.Background = Brushes.White;
                inputBox.Text = "";
                inputBox.IsEnabled = true;

                if (wordWasRevealed)
                {
                    inputBox.Background = Brushes.Red;
                    inputBox.Text = "";
                    inputBox.IsEnabled = false;

                    if (GameCom.GameServer != null)
                        GameState.PointsPlayerA = GameState.PointsPlayerA + 1;

                    else if (GameCom.GameClient != null)
                        GameState.PointsPlayerB = GameState.PointsPlayerB + 1;

                    ShowGamePoints();
                }
            }

            RenderRevealed();

            if (GameCom.GameServer != null)
                GameCom.GameServer.SendString(GameState.ToString());

            else if (GameCom.GameClient != null)
                GameCom.GameClient.SendString(GameState.ToString());
        }

        private void RenderRevealed()
        {
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    if (GameState.RevealedBoard[y][x] != ' ')
                    {
                        var button = (Button)this.FindName("b" + x.ToString() + y.ToString());

                        if (GameState.RevealedBoard[y][x] == '#')
                        {
                            button.Content = (char)9632;
                            continue;
                        }

                        button.Content = GameState.RevealedBoard[y][x];
                        var word = GameState.GetWord(x, y);

                        if (word.Horizontal == false)
                            button.BorderBrush = Brushes.Red;

                        else
                            button.BorderBrush = new SolidColorBrush(Color.FromArgb(255, (byte)0, (byte)200, (byte)0));
                    }
                }
            }
        }

        private void SetButtonProperties()
        {
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    var button = (Button)this.FindName("b" + x.ToString() + y.ToString());
                    button.BorderThickness = new Thickness(3);
                    button.BorderBrush = Brushes.Black;
                    button.FontSize = 16;
                }
            }
        }

        private void inputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                inputBox.Background = Brushes.White;

                if (((TextBox)sender).Text == "")
                {
                    if(GameState.Phase == "[PHASE2]")
                    {
                        GameState.LetterWasRevealed = false;
                        SetAllButtons(false);
                        ShowGamePoints();

                        inputBox.Background = Brushes.Red;
                        inputBox.Text = "";
                        inputBox.IsEnabled = false;

                        if (GameCom.GameServer != null)
                            GameCom.GameServer.SendString(GameState.ToString());

                        else if (GameCom.GameClient != null)
                            GameCom.GameClient.SendString(GameState.ToString());
                    }

                    else
                        return;
                }

                else if (((TextBox)sender).Text.Contains(" "))
                    return;

                else if (((TextBox)sender).Text.Contains("startserver"))
                {
                    GameCom.InitServer();
                    GameCom.GameServer.StringReceivedEvent += StringReceivedEvent;

                    inputBox.Text = "";

                    Thread.Sleep(1000);

                    GameCom.GameServer.SendString("-GAME START-");

                    return;
                }

                else if (Char.IsDigit(((TextBox)sender).Text[0]))
                {
                    GameCom.InitClient(((TextBox)sender).Text);
                    GameCom.GameClient.StringReceivedEvent += StringReceivedEvent;

                    inputBox.Text = "";

                    Thread.Sleep(1000);

                    GameCom.GameClient.SendString("-GAME START-");

                    return;
                }

                else if (GameState.Phase == "[PHASE1]")
                {
                    bool canAddToBoard = CanAddToBoard(((TextBox)sender).Text);

                    if (canAddToBoard == false || WillBoardCoverageBeAbove(50, ((TextBox)sender).Text))
                    {
                        GameState.Phase = "[PHASE2]";
                        SetAllButtons(false);

                        ShowGamePoints();

                        inputBox.Background = Brushes.Red;
                        inputBox.Text = "";
                        inputBox.IsEnabled = false;

                        if (GameCom.GameServer != null)
                            GameCom.GameServer.SendString(GameState.ToString());

                        else if (GameCom.GameClient != null)
                            GameCom.GameClient.SendString(GameState.ToString());

                        return;
                    }

                    AddToBoard(((TextBox)sender).Text);

                    AddToChatBox(((TextBox)sender).Text);

                    if (GameCom.GameServer != null)
                    {
                        GameCom.GameServer.SendString(((TextBox)sender).Text);
                        GameCom.GameServer.SendString(GameState.ToString());
                    }

                    else if (GameCom.GameClient != null)
                    {
                        GameCom.GameClient.SendString(((TextBox)sender).Text);
                        GameCom.GameClient.SendString(GameState.ToString());
                    }

                    inputBox.Background = Brushes.Red;
                    inputBox.Text = "";
                    inputBox.IsEnabled = false;
                }

                else if (GameState.Phase == "[PHASE2]")
                {
                    if (GameState.GetWord(_lastClickedX, _lastClickedY).TheWord == ((TextBox)sender).Text)
                    {
                        GameState.RevealWord(_lastClickedX, _lastClickedY);
                        RenderRevealed();

                        if (GameCom.GameServer != null)
                            GameState.PointsPlayerA = GameState.PointsPlayerA + 1;

                        else if (GameCom.GameClient != null)
                            GameState.PointsPlayerB = GameState.PointsPlayerB + 1;
                    }

                    GameState.LetterWasRevealed = false;

                    if (GameCom.GameServer != null)
                    {
                        GameCom.GameServer.SendString(GameState.ToString());
                    }

                    else if (GameCom.GameClient != null)
                    {
                        GameCom.GameClient.SendString(GameState.ToString());
                    }

                    ShowGamePoints();

                    inputBox.Background = Brushes.Red;
                    inputBox.Text = "";
                    inputBox.IsEnabled = false;
                }
            }
        }

        private bool WillBoardCoverageBeAbove(int n, string text)
        {
            int z = 0;

            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    if (GameState.Board[y][x] != ' ')
                        ++z;
                }
            }

            if ((z + text.Length) > n)
                return true;

            else
                return false;
        }

        private void StringReceivedEvent(object? sender, string str)
        {
            if (str.StartsWith("[BEGINGAMESTATE]"))
                System.Windows.Application.Current.Dispatcher.Invoke(() => HandleReceiveGameState(str));
            else
                System.Windows.Application.Current.Dispatcher.Invoke(() => AddToChatBox(str));
        }

        private void HandleReceiveGameState(string str)
        {
            GameState = new GameState();
            GameState.FromString(str);
            //GameState.RevealAll();
            RenderRevealed();

            if (GameState.Phase == "[PHASE2]")
            {
                ShowGamePoints();

                if (GameState.LetterWasRevealed == false)
                    SetAllButtons(true);
            }

            if (GameState.LetterWasRevealed == false || GameState.WordWasRevealed == true)
            {
                inputBox.Background = Brushes.White;
                inputBox.Text = "";
                inputBox.IsEnabled = true;
            }

            GameState.LetterWasRevealed = false;
            GameState.WordWasRevealed = false;
        }

        private void ShowGamePoints()
        {
            chatBox.Text = "";

            if (GameCom.GameServer != null)
            {
                AddToChatBox("Jag: " + GameState.PointsPlayerA.ToString());
                AddToChatBox("Motst: " + GameState.PointsPlayerB.ToString());
            }

            else if (GameCom.GameClient != null)
            {
                AddToChatBox("Jag: " + GameState.PointsPlayerB.ToString());
                AddToChatBox("Motst: " + GameState.PointsPlayerA.ToString());
            }
        }

        private void AddToChatBox(string text)
        {
            int newlineCount = chatBox.Text.Count(f => f == '\n');

            if (newlineCount > 2)
            {
                int firstNewlineIndex = chatBox.Text.IndexOf("\n");
                chatBox.Text = chatBox.Text.Substring(firstNewlineIndex + 1);
            }

            chatBox.Text += text + "\n";
        }

        private bool CanAddToBoard(string text)
        {
            bool canAdd;

            for (int i = 0; i < 100; i++)
            {
                Random rnd = new Random();
                int x = rnd.Next(0, 9);
                int y = rnd.Next(0, 9);
                bool right = RandBool();

                try
                {
                    canAdd = TryAddToBoard(x, y, right, text, true);
                }
                catch
                {
                    continue;
                }

                if (canAdd)
                {
                    return true;
                }
            }

            return false;
        }

        private bool AddToBoard(string text)
        {
            bool success;

            for (int i = 0; i < 100; i++)
            {
                Random rnd = new Random();
                int x = rnd.Next(0, 9);
                int y = rnd.Next(0, 9);
                bool right = RandBool();

                try
                {
                    success = TryAddToBoard(x, y, right, text, false);
                }
                catch
                {
                    continue;
                }

                if (success)
                {
                    GameState.WordList.Add(new Word(x, y, text, right));

                    return true;
                }
            }

            return false;
        }

        private bool TryAddToBoard(int x, int y, bool right, string text, bool test)
        {
            if (right)
            {
                if (BoardSpaceRightIsEmpty(x, y, text.Length))
                {
                    if (!test)
                        PutStringInBoardRight(x, y, text);

                    return true;
                }

                else
                    return false;
            }

            else
            {
                if (BoardSpaceDownIsEmpty(x, y, text.Length))
                {
                    if (!test)
                        PutStringInBoardDown(x, y, text);

                    return true;
                }

                else
                    return false;
            }
        }

        private void PutStringInBoardRight(int x, int y, string text)
        {
            for (int X = x; X < (text.Length + x); X++)
            {
                GameState.Board[y] = ReplaceAt(GameState.Board[y], X, text[X - x]);
            }
        }

        private bool BoardSpaceRightIsEmpty(int x, int y, int length)
        {
            for (int X = x; X < (length + x); X++)
            {
                if (GameState.Board[y][X] != (char)32)
                    return false;
            }

            return true;
        }

        private void PutStringInBoardDown(int x, int y, string text)
        {
            for (int Y = y; Y < (text.Length + y); Y++)
            {
                GameState.Board[Y] = ReplaceAt(GameState.Board[Y], x, text[Y - y]);
            }
        }

        private bool BoardSpaceDownIsEmpty(int x, int y, int length)
        {
            for (int Y = y; Y < (length + y); Y++)
            {
                if (GameState.Board[Y][x] != (char)32)
                    return false;
            }

            return true;
        }

        private bool RandBool()
        {
            var random = new Random();
            return random.Next(2) == 1;
        }

        public string ReplaceAt(string input, int index, char newChar)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            char[] chars = input.ToCharArray();
            chars[index] = newChar;
            return new string(chars);
        }
    }
}