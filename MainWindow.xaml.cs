using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static BrainStorm.Enums;

namespace BrainStorm
{
    public partial class MainWindow : Window
    {
        public GameCom GameCom { get; set; } = new GameCom();
        public GameEngine GameEngine { get; set; } = new GameEngine();
        public GameInputBox GameInputBox { get; set; } = new GameInputBox();
        public GameChatBox GameChatBox { get; set; } = new GameChatBox();
        public AudioHandler AudioHandler { get; set; } = new AudioHandler();

        int _lastClickedX = 0;
        int _lastClickedY = 0;

        bool _iAmServer = false;
        bool _gameHasStarted = false;

        bool _buttonsEnabled = false;
        bool _natoWavEnabled = false;

        public MainWindow()
        {
            InitializeComponent();

            InitAllButtons();

            if (File.Exists($"./memory.txt") == false)
                File.Create($"./memory.txt");

            if (File.Exists($"./settings.cfg") == false)
                InitSettingsCfg();

            GameEngine.State = new State();
            GameEngine.BoardHandler = new BoardHandler(GameEngine.State);
        }

        private void InitSettingsCfg()
        {
            File.Create($"./settings.cfg");
            // TODO
        }

        private void Button_Click(object sender, MouseButtonEventArgs e)
        {
            if (_buttonsEnabled == false)
                return;

            GameInputBox.OppTurn();
            inputBox = GameInputBox.UpdateTextBox(inputBox);

            string buttonName = ((Button)sender).Name;
            int x = int.Parse(buttonName[1].ToString());
            int y = int.Parse(buttonName[2].ToString());

            _lastClickedX = x;
            _lastClickedY = y;

            _buttonsEnabled = false;
            bool wordWasRevealed = false;
            bool letterWasRevealed = GameEngine.State.Reveal(x, y, out wordWasRevealed);

            if (letterWasRevealed)
            {
                GameInputBox.MyTurn();
                inputBox = GameInputBox.UpdateTextBox(inputBox);

                if (wordWasRevealed)
                {
                    GameInputBox.OppTurn();
                    inputBox = GameInputBox.UpdateTextBox(inputBox);

                    if (_iAmServer)
                    {
                        GameEngine.State.PointsPlayerA = GameEngine.State.PointsPlayerA + 1;
                        chatBox = GameChatBox.ShowGamePoints(chatBox, GameEngine.State.PointsPlayerA, GameEngine.State.PointsPlayerB);
                    }

                    else
                    {
                        GameEngine.State.PointsPlayerB = GameEngine.State.PointsPlayerB + 1;
                        chatBox = GameChatBox.ShowGamePoints(chatBox, GameEngine.State.PointsPlayerB, GameEngine.State.PointsPlayerA);
                    }
                }
            }

            RenderRevealed();

            if (_iAmServer)
                GameCom.GameServer.SendString(GameEngine.State.ToString());

            else
                GameCom.GameClient.SendString(GameEngine.State.ToString());
        }

        private void RenderRevealed()
        {
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    if (GameEngine.State.RevealedBoard[y][x] != ' ')
                    {
                        var button = (Button)this.FindName("b" + x.ToString() + y.ToString());

                        if (GameEngine.State.RevealedBoard[y][x] == '#')
                        {
                            button.Content = (char)9632;
                            continue;
                        }

                        button.Content = GameEngine.State.RevealedBoard[y][x];
                        var word = GameEngine.State.GetWord(x, y);

                        if (word.Direction == Direction.Horizontal)
                            button.BorderBrush = new SolidColorBrush(Color.FromArgb(255, (byte)0, (byte)200, (byte)0));

                        if (word.Direction == Direction.Vertical)
                            button.BorderBrush = Brushes.Red;
                    }
                }
            }
        }

        private void InitAllButtons()
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
            if (_natoWavEnabled && (AudioHandler.Initialized))
                AudioHandler.PlayNATOAudio(e.Key);

            if (e.Key == Key.Enter)
            {
                GameInputBox.Text = ((TextBox)sender).Text;
                GameInputBox.Brush = Brushes.White;
                inputBox = GameInputBox.UpdateTextBox(inputBox);

                if ((GameInputBox.Text == $"%NATO") && (AudioHandler.Initialized))
                {
                        if (_natoWavEnabled == false)
                            _natoWavEnabled = true;
                        else if (_natoWavEnabled == true)
                            _natoWavEnabled = false;
                    
                    chatBox = GameChatBox.Add(chatBox, GameInputBox.Text);
                    GameInputBox.Text = "";
                    inputBox = GameInputBox.UpdateTextBox(inputBox);

                    return;
                }

                if (GameInputBox.Text == "")
                {
                    if (GameEngine.State.Phase == "[PHASE2]")
                    {
                        GameEngine.State.LetterWasRevealed = false;
                        _buttonsEnabled = false;

                        if (_iAmServer)
                            chatBox = GameChatBox.ShowGamePoints(chatBox, GameEngine.State.PointsPlayerA, GameEngine.State.PointsPlayerB);
                        else
                            chatBox = GameChatBox.ShowGamePoints(chatBox, GameEngine.State.PointsPlayerB, GameEngine.State.PointsPlayerA);

                        GameInputBox.OppTurn();
                        inputBox = GameInputBox.UpdateTextBox(inputBox);

                        if (_iAmServer)
                            GameCom.GameServer.SendString(GameEngine.State.ToString());

                        else
                            GameCom.GameClient.SendString(GameEngine.State.ToString());
                    }

                    else
                        return;
                }

                else if (GameInputBox.Text.Contains(" ") || ((GameInputBox.Text.Length > 10) && !GameInputBox.Text.Contains("startserver")))
                    return;

                else if (GameInputBox.Text.Contains("startserver"))
                {
                    GameCom.InitServer();
                    _iAmServer = true;

                    if (_iAmServer)
                        GameCom.GameServer.StringReceivedEvent += StringReceived;
                    else
                        return;

                    GameInputBox.Text = "";
                    inputBox = GameInputBox.UpdateTextBox(inputBox);

                    //Thread.Sleep(1000);
                    _gameHasStarted = true;
                    GameCom.GameServer.SendString("-GAME START-");

                    return;
                }

                else if (Helpers.IsValidIP(GameInputBox.Text) && (_gameHasStarted == false))
                {
                    GameCom.InitClient(GameInputBox.Text);
                    _iAmServer = false;

                    if (_iAmServer == false)
                        GameCom.GameClient.StringReceivedEvent += StringReceived;
                    else
                        return;

                    inputBox.Text = "";
                    //Thread.Sleep(1000);
                    _gameHasStarted = true;
                    GameCom.GameClient.SendString("-GAME START-");

                    return;
                }

                else if (GameEngine.State.Phase == "[PHASE1]")
                {
                    bool canAddToBoard = GameEngine.BoardHandler.CanAddToBoard(GameInputBox.Text);

                    using (StreamWriter sw = File.AppendText($"./memory.txt"))
                    {
                        sw.WriteLine(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " --- " + GameInputBox.Text + "\n");
                    }

                    if (canAddToBoard == false || WillBoardCoverageBeAbove(50, GameInputBox.Text))
                    {
                        GameEngine.State.Phase = "[PHASE2]";
                        _buttonsEnabled = false;

                        if (_iAmServer)
                            chatBox = GameChatBox.ShowGamePoints(chatBox, GameEngine.State.PointsPlayerA, GameEngine.State.PointsPlayerB);
                        else
                            chatBox = GameChatBox.ShowGamePoints(chatBox, GameEngine.State.PointsPlayerB, GameEngine.State.PointsPlayerA);

                        GameInputBox.OppTurn();
                        inputBox = GameInputBox.UpdateTextBox(inputBox);

                        if (_iAmServer)
                            GameCom.GameServer.SendString(GameEngine.State.ToString());

                        else
                            GameCom.GameClient.SendString(GameEngine.State.ToString());

                        return;
                    }

                    GameEngine.BoardHandler = new BoardHandler(GameEngine.State);
                    GameEngine.BoardHandler.AddToBoard(GameInputBox.Text);

                    chatBox = GameChatBox.Add(chatBox, GameInputBox.Text);

                    if (_iAmServer)
                    {
                        GameCom.GameServer.SendString(GameInputBox.Text);
                        GameCom.GameServer.SendString(GameEngine.State.ToString());
                    }

                    else
                    {
                        GameCom.GameClient.SendString(GameInputBox.Text);
                        GameCom.GameClient.SendString(GameEngine.State.ToString());
                    }

                    GameInputBox.OppTurn();
                    inputBox = GameInputBox.UpdateTextBox(inputBox);
                }

                else if (GameEngine.State.Phase == "[PHASE2]")
                {
                    if (GameEngine.State.GetWord(_lastClickedX, _lastClickedY).TheWord == GameInputBox.Text)
                    {
                        GameEngine.State.RevealWord(_lastClickedX, _lastClickedY);
                        RenderRevealed();

                        if (_iAmServer)
                            GameEngine.State.PointsPlayerA = GameEngine.State.PointsPlayerA + 1;

                        else
                            GameEngine.State.PointsPlayerB = GameEngine.State.PointsPlayerB + 1;
                    }

                    GameEngine.State.LetterWasRevealed = false;

                    if (_iAmServer)
                        GameCom.GameServer.SendString(GameEngine.State.ToString());

                    else
                        GameCom.GameClient.SendString(GameEngine.State.ToString());

                    if (_iAmServer)
                        chatBox = GameChatBox.ShowGamePoints(chatBox, GameEngine.State.PointsPlayerA, GameEngine.State.PointsPlayerB);
                    else
                        chatBox = GameChatBox.ShowGamePoints(chatBox, GameEngine.State.PointsPlayerB, GameEngine.State.PointsPlayerA);

                    GameInputBox.OppTurn();
                    inputBox = GameInputBox.UpdateTextBox(inputBox);
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
                    if (GameEngine.State.Board[y][x] != ' ')
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
                System.Windows.Application.Current.Dispatcher.Invoke(() => HandleReceiveString(str));
        }

        private void HandleReceiveString(string str)
        {
            chatBox = GameChatBox.Add(chatBox, str);
        }

        private void HandleReceiveGameState(string str)
        {
            GameEngine.State = new State();
            GameEngine.State.FromString(str);
            //GameState.RevealAll();
            RenderRevealed();

            if (GameEngine.State.Phase == "[PHASE2]")
            {
                if (_iAmServer)
                    chatBox = GameChatBox.ShowGamePoints(chatBox, GameEngine.State.PointsPlayerA, GameEngine.State.PointsPlayerB);
                else
                    chatBox = GameChatBox.ShowGamePoints(chatBox, GameEngine.State.PointsPlayerB, GameEngine.State.PointsPlayerA);

                if (GameEngine.State.LetterWasRevealed == false)
                    _buttonsEnabled = true;
            }

            if (GameEngine.State.LetterWasRevealed == false || GameEngine.State.WordWasRevealed == true)
            {
                GameInputBox.MyTurn();
                inputBox = GameInputBox.UpdateTextBox(inputBox);
            }

            GameEngine.State.LetterWasRevealed = false;
            GameEngine.State.WordWasRevealed = false;
        }
    }
}