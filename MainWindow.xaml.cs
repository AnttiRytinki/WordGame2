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
        public GameNetTCPIP GameCom { get; set; } = new GameNetTCPIP();
        public Engine Engine { get; set; } = new Engine();
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

            Engine.State = new State();
            Engine.BoardHandler = new BoardHandler(Engine.State);
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
            bool letterWasRevealed = Engine.State.Reveal(x, y, out wordWasRevealed);

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
                        Engine.State.PointsPlayerA = Engine.State.PointsPlayerA + 1;
                        chatBox = GameChatBox.ShowGamePoints(chatBox, Engine.State.PointsPlayerA, Engine.State.PointsPlayerB);
                    }

                    else
                    {
                        Engine.State.PointsPlayerB = Engine.State.PointsPlayerB + 1;
                        chatBox = GameChatBox.ShowGamePoints(chatBox, Engine.State.PointsPlayerB, Engine.State.PointsPlayerA);
                    }
                }
            }

            RenderRevealed();

            if (_iAmServer)
                GameCom.GameServer.SendString(Engine.State.ToString());

            else
                GameCom.GameClient.SendString(Engine.State.ToString());
        }

        private void RenderRevealed()
        {
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    if (Engine.State.RevealedBoard[y][x] != ' ')
                    {
                        var button = (Button)this.FindName("b" + x.ToString() + y.ToString());

                        if (Engine.State.RevealedBoard[y][x] == '#')
                        {
                            button.Content = (char)9632;
                            continue;
                        }

                        button.Content = Engine.State.RevealedBoard[y][x];
                        var word = Engine.State.GetWord(x, y);

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
                    if (Engine.State.Phase == "[PHASE2]")
                    {
                        Engine.State.LetterWasRevealed = false;
                        _buttonsEnabled = false;

                        if (_iAmServer)
                            chatBox = GameChatBox.ShowGamePoints(chatBox, Engine.State.PointsPlayerA, Engine.State.PointsPlayerB);
                        else
                            chatBox = GameChatBox.ShowGamePoints(chatBox, Engine.State.PointsPlayerB, Engine.State.PointsPlayerA);

                        GameInputBox.OppTurn();
                        inputBox = GameInputBox.UpdateTextBox(inputBox);

                        if (_iAmServer)
                            GameCom.GameServer.SendString(Engine.State.ToString());

                        else
                            GameCom.GameClient.SendString(Engine.State.ToString());
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
                    GameCom.InitAndConnectClient(GameInputBox.Text);
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

                else if (Engine.State.Phase == "[PHASE1]")
                {
                    bool canAddToBoard = Engine.BoardHandler.CanAddToBoard(GameInputBox.Text);

                    using (StreamWriter sw = File.AppendText($"./memory.txt"))
                    {
                        sw.WriteLine(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " --- " + GameInputBox.Text + "\n");
                    }

                    if (canAddToBoard == false || WillBoardCoverageBeAbove(50, GameInputBox.Text))
                    {
                        Engine.State.Phase = "[PHASE2]";
                        _buttonsEnabled = false;

                        if (_iAmServer)
                            chatBox = GameChatBox.ShowGamePoints(chatBox, Engine.State.PointsPlayerA, Engine.State.PointsPlayerB);
                        else
                            chatBox = GameChatBox.ShowGamePoints(chatBox, Engine.State.PointsPlayerB, Engine.State.PointsPlayerA);

                        GameInputBox.OppTurn();
                        inputBox = GameInputBox.UpdateTextBox(inputBox);

                        if (_iAmServer)
                            GameCom.GameServer.SendString(Engine.State.ToString());

                        else
                            GameCom.GameClient.SendString(Engine.State.ToString());

                        return;
                    }

                    Engine.BoardHandler = new BoardHandler(Engine.State);
                    Engine.BoardHandler.AddToBoard(GameInputBox.Text);

                    chatBox = GameChatBox.Add(chatBox, GameInputBox.Text);

                    if (_iAmServer)
                    {
                        GameCom.GameServer.SendString(GameInputBox.Text);
                        GameCom.GameServer.SendString(Engine.State.ToString());
                    }

                    else
                    {
                        GameCom.GameClient.SendString(GameInputBox.Text);
                        GameCom.GameClient.SendString(Engine.State.ToString());
                    }

                    GameInputBox.OppTurn();
                    inputBox = GameInputBox.UpdateTextBox(inputBox);
                }

                else if (Engine.State.Phase == "[PHASE2]")
                {
                    if (Engine.State.GetWord(_lastClickedX, _lastClickedY).TheWord == GameInputBox.Text)
                    {
                        Engine.State.RevealWord(_lastClickedX, _lastClickedY);
                        RenderRevealed();

                        if (_iAmServer)
                            Engine.State.PointsPlayerA = Engine.State.PointsPlayerA + 1;

                        else
                            Engine.State.PointsPlayerB = Engine.State.PointsPlayerB + 1;
                    }

                    Engine.State.LetterWasRevealed = false;

                    if (_iAmServer)
                        GameCom.GameServer.SendString(Engine.State.ToString());

                    else
                        GameCom.GameClient.SendString(Engine.State.ToString());

                    if (_iAmServer)
                        chatBox = GameChatBox.ShowGamePoints(chatBox, Engine.State.PointsPlayerA, Engine.State.PointsPlayerB);
                    else
                        chatBox = GameChatBox.ShowGamePoints(chatBox, Engine.State.PointsPlayerB, Engine.State.PointsPlayerA);

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
                    if (Engine.State.Board[y][x] != ' ')
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
            Engine.State = new State();
            Engine.State.FromString(str);
            //GameState.RevealAll();
            RenderRevealed();

            if (Engine.State.Phase == "[PHASE2]")
            {
                if (_iAmServer)
                    chatBox = GameChatBox.ShowGamePoints(chatBox, Engine.State.PointsPlayerA, Engine.State.PointsPlayerB);
                else
                    chatBox = GameChatBox.ShowGamePoints(chatBox, Engine.State.PointsPlayerB, Engine.State.PointsPlayerA);

                if (Engine.State.LetterWasRevealed == false)
                    _buttonsEnabled = true;
            }

            if (Engine.State.LetterWasRevealed == false || Engine.State.WordWasRevealed == true)
            {
                GameInputBox.MyTurn();
                inputBox = GameInputBox.UpdateTextBox(inputBox);
            }

            Engine.State.LetterWasRevealed = false;
            Engine.State.WordWasRevealed = false;
        }
    }
}