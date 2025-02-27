using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OrdSpel2
{
    public partial class MainWindow : Window
    {
        public GameCom GameCom { get; set; } = new GameCom();

        public Helpers Helpers { get; set; } = new Helpers();
        public GameState GameState { get; set; }
        public BoardHandler BoardHandler { get; set; }

        public AudioHandler AudioHandler { get; set; } = new AudioHandler();

        int _lastClickedX = 0;
        int _lastClickedY = 0;

        bool _buttonsEnabled = false;

        bool _natoWav = false;

        public MainWindow()
        {
            InitializeComponent();

            SetAllButtonProperties();
            SetAllButtons(false);

            if (File.Exists($"./memory.txt") == false)
                File.Create($"./memory.txt");

            if (File.Exists($"./settings.txt") == false)
                InitSettingsTxt();

            GameState = new GameState(Helpers);
            BoardHandler = new BoardHandler(Helpers, GameState);
        }

        private void InitSettingsTxt()
        {
            File.Create($"./settings.txt");
            // TODO
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

        private void SetAllButtonProperties()
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
            if (_natoWav)
                AudioHandler.HandleQWERTYAudio(e.Key);

            if (e.Key == Key.Enter)
            {
                inputBox.Background = Brushes.White;
                string text = ((TextBox)sender).Text;

                if (text == $"/NATO")
                    _natoWav = true;

                if (text == $"/hitler")
                    AudioHandler.PlayRandomHitlerSample();

                if (text == "")
                {
                    if (GameState.Phase == "[PHASE2]")
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

                else if (text.Contains(" ") || (text.Length > 10))
                    return;

                else if (text.Contains("startserver"))
                {
                    GameCom.InitServer();

                    if (GameCom.GameServer != null)
                        GameCom.GameServer.StringReceivedEvent += StringReceived;
                    else
                        return;

                    inputBox.Text = "";
                    Thread.Sleep(1000);
                    GameCom.GameServer.SendString("-GAME START-");

                    return;
                }

                else if (Char.IsDigit(text[0]))
                {
                    GameCom.InitClient(text);

                    if (GameCom.GameClient != null)
                        GameCom.GameClient.StringReceivedEvent += StringReceived;
                    else
                        return;

                    inputBox.Text = "";
                    Thread.Sleep(1000);
                    GameCom.GameClient.SendString("-GAME START-");

                    return;
                }

                else if (GameState.Phase == "[PHASE1]")
                {
                    bool canAddToBoard = BoardHandler.CanAddToBoard(text);

                    using (StreamWriter sw = File.AppendText($"./memory.txt"))
                    {
                        sw.WriteLine(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " --- " + text + "\n");
                    }

                    if (canAddToBoard == false || WillBoardCoverageBeAbove(50, text))
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

                    BoardHandler = new BoardHandler(Helpers, GameState);
                    BoardHandler.AddToBoard(text);

                    AddToChatBox(text);

                    if (GameCom.GameServer != null)
                    {
                        GameCom.GameServer.SendString(text);
                        GameCom.GameServer.SendString(GameState.ToString());
                    }

                    else if (GameCom.GameClient != null)
                    {
                        GameCom.GameClient.SendString(text);
                        GameCom.GameClient.SendString(GameState.ToString());
                    }

                    inputBox.Background = Brushes.Red;
                    inputBox.Text = "";
                    inputBox.IsEnabled = false;
                }

                else if (GameState.Phase == "[PHASE2]")
                {
                    if (GameState.GetWord(_lastClickedX, _lastClickedY).TheWord == text)
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
                        GameCom.GameServer.SendString(GameState.ToString());

                    else if (GameCom.GameClient != null)
                        GameCom.GameClient.SendString(GameState.ToString());

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

        private void StringReceived(object? sender, string str)
        {
            if (str.StartsWith("[BEGINGAMESTATE]"))
                System.Windows.Application.Current.Dispatcher.Invoke(() => HandleReceiveGameState(str));
            else
                System.Windows.Application.Current.Dispatcher.Invoke(() => AddToChatBox(str));
        }

        private void HandleReceiveGameState(string str)
        {
            GameState = new GameState(Helpers);
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
                AddToChatBox("Me: " + GameState.PointsPlayerA.ToString());
                AddToChatBox("Opp: " + GameState.PointsPlayerB.ToString());
            }

            else if (GameCom.GameClient != null)
            {
                AddToChatBox("Me: " + GameState.PointsPlayerB.ToString());
                AddToChatBox("Opp: " + GameState.PointsPlayerA.ToString());
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
    }
}