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
        public GameInput GameInput { get; set; } = new GameInput();
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

            GameEngine.GameState = new GameState();
            GameEngine.BoardHandler = new BoardHandler(GameEngine.GameState);
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
            bool letterWasRevealed = GameEngine.GameState.Reveal(x, y, out wordWasRevealed);

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
                        GameEngine.GameState.PointsPlayerA = GameEngine.GameState.PointsPlayerA + 1;
                        chatBox = GameChatBox.ShowGamePoints(chatBox, GameEngine.GameState.PointsPlayerA, GameEngine.GameState.PointsPlayerB);
                    }

                    else
                    {
                        GameEngine.GameState.PointsPlayerB = GameEngine.GameState.PointsPlayerB + 1;
                        chatBox = GameChatBox.ShowGamePoints(chatBox, GameEngine.GameState.PointsPlayerB, GameEngine.GameState.PointsPlayerA);
                    }
                }
            }

            RenderRevealed();

            if (_iAmServer)
                GameCom.GameServer.SendString(GameEngine.GameState.ToString());

            else
                GameCom.GameClient.SendString(GameEngine.GameState.ToString());
        }

        private void RenderRevealed()
        {
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    if (GameEngine.GameState.RevealedBoard[y][x] != ' ')
                    {
                        var button = (Button)this.FindName("b" + x.ToString() + y.ToString());

                        if (GameEngine.GameState.RevealedBoard[y][x] == '#')
                        {
                            button.Content = (char)9632;
                            continue;
                        }

                        button.Content = GameEngine.GameState.RevealedBoard[y][x];
                        var word = GameEngine.GameState.GetWord(x, y);

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

            //GameInput.HandleKeypress(e.Key);

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
                    if (GameEngine.GameState.Phase == "[PHASE2]")
                    {
                        GameEngine.GameState.LetterWasRevealed = false;
                        _buttonsEnabled = false;

                        if (_iAmServer)
                            chatBox = GameChatBox.ShowGamePoints(chatBox, GameEngine.GameState.PointsPlayerA, GameEngine.GameState.PointsPlayerB);
                        else
                            chatBox = GameChatBox.ShowGamePoints(chatBox, GameEngine.GameState.PointsPlayerB, GameEngine.GameState.PointsPlayerA);

                        GameInputBox.OppTurn();
                        inputBox = GameInputBox.UpdateTextBox(inputBox);

                        if (_iAmServer)
                            GameCom.GameServer.SendString(GameEngine.GameState.ToString());

                        else
                            GameCom.GameClient.SendString(GameEngine.GameState.ToString());
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

                else if (GameEngine.GameState.Phase == "[PHASE1]")
                {
                    bool canAddToBoard = GameEngine.BoardHandler.CanAddToBoard(GameInputBox.Text);

                    using (StreamWriter sw = File.AppendText($"./memory.txt"))
                    {
                        sw.WriteLine(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " --- " + GameInputBox.Text + "\n");
                    }

                    if (canAddToBoard == false || WillBoardCoverageBeAbove(50, GameInputBox.Text))
                    {
                        GameEngine.GameState.Phase = "[PHASE2]";
                        _buttonsEnabled = false;

                        if (_iAmServer)
                            chatBox = GameChatBox.ShowGamePoints(chatBox, GameEngine.GameState.PointsPlayerA, GameEngine.GameState.PointsPlayerB);
                        else
                            chatBox = GameChatBox.ShowGamePoints(chatBox, GameEngine.GameState.PointsPlayerB, GameEngine.GameState.PointsPlayerA);

                        GameInputBox.OppTurn();
                        inputBox = GameInputBox.UpdateTextBox(inputBox);

                        if (_iAmServer)
                            GameCom.GameServer.SendString(GameEngine.GameState.ToString());

                        else
                            GameCom.GameClient.SendString(GameEngine.GameState.ToString());

                        return;
                    }

                    GameEngine.BoardHandler = new BoardHandler(GameEngine.GameState);
                    GameEngine.BoardHandler.AddToBoard(GameInputBox.Text);

                    chatBox = GameChatBox.Add(chatBox, GameInputBox.Text);

                    if (_iAmServer)
                    {
                        GameCom.GameServer.SendString(GameInputBox.Text);
                        GameCom.GameServer.SendString(GameEngine.GameState.ToString());
                    }

                    else
                    {
                        GameCom.GameClient.SendString(GameInputBox.Text);
                        GameCom.GameClient.SendString(GameEngine.GameState.ToString());
                    }

                    GameInputBox.OppTurn();
                    inputBox = GameInputBox.UpdateTextBox(inputBox);
                }

                else if (GameEngine.GameState.Phase == "[PHASE2]")
                {
                    if (GameEngine.GameState.GetWord(_lastClickedX, _lastClickedY).TheWord == GameInputBox.Text)
                    {
                        GameEngine.GameState.RevealWord(_lastClickedX, _lastClickedY);
                        RenderRevealed();

                        if (_iAmServer)
                            GameEngine.GameState.PointsPlayerA = GameEngine.GameState.PointsPlayerA + 1;

                        else
                            GameEngine.GameState.PointsPlayerB = GameEngine.GameState.PointsPlayerB + 1;
                    }

                    GameEngine.GameState.LetterWasRevealed = false;

                    if (_iAmServer)
                        GameCom.GameServer.SendString(GameEngine.GameState.ToString());

                    else
                        GameCom.GameClient.SendString(GameEngine.GameState.ToString());

                    if (_iAmServer)
                        chatBox = GameChatBox.ShowGamePoints(chatBox, GameEngine.GameState.PointsPlayerA, GameEngine.GameState.PointsPlayerB);
                    else
                        chatBox = GameChatBox.ShowGamePoints(chatBox, GameEngine.GameState.PointsPlayerB, GameEngine.GameState.PointsPlayerA);

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
                    if (GameEngine.GameState.Board[y][x] != ' ')
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
            GameEngine.GameState = new GameState();
            GameEngine.GameState.FromString(str);
            //GameState.RevealAll();
            RenderRevealed();

            if (GameEngine.GameState.Phase == "[PHASE2]")
            {
                if (_iAmServer)
                    chatBox = GameChatBox.ShowGamePoints(chatBox, GameEngine.GameState.PointsPlayerA, GameEngine.GameState.PointsPlayerB);
                else
                    chatBox = GameChatBox.ShowGamePoints(chatBox, GameEngine.GameState.PointsPlayerB, GameEngine.GameState.PointsPlayerA);

                if (GameEngine.GameState.LetterWasRevealed == false)
                    _buttonsEnabled = true;
            }

            if (GameEngine.GameState.LetterWasRevealed == false || GameEngine.GameState.WordWasRevealed == true)
            {
                GameInputBox.MyTurn();
                inputBox = GameInputBox.UpdateTextBox(inputBox);
            }

            GameEngine.GameState.LetterWasRevealed = false;
            GameEngine.GameState.WordWasRevealed = false;
        }
    }
}