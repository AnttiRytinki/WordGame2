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

        bool _buttonsEnabled = false;
        bool _natoWavEnabled = false;

        public MainWindow()
        {
            InitializeComponent();

            SetAllButtonProperties();
            SetAllButtons(false);

            if (File.Exists($"./memory.txt") == false)
                File.Create($"./memory.txt");

            if (File.Exists($"./settings.txt") == false)
                InitSettingsTxt();

            GameEngine.GameState = new GameState();
            GameEngine.BoardHandler = new BoardHandler(GameEngine.GameState);
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

            GameInputBox.OppTurn();
            inputBox = GameInputBox.UpdateTextBox(inputBox);

            string buttonName = ((Button)sender).Name;
            int x = int.Parse(buttonName[1].ToString());
            int y = int.Parse(buttonName[2].ToString());

            _lastClickedX = x;
            _lastClickedY = y;

            SetAllButtons(false);
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

            if (GameCom.GameServer != null)
                GameCom.GameServer.SendString(GameEngine.GameState.ToString());

            else if (GameCom.GameClient != null)
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
            if (_natoWavEnabled)
            {
                try
                {
                    AudioHandler.HandleQWERTYAudio(e.Key);
                }
                catch
                {
                    ;
                }
            }

            if (e.Key == Key.Enter)
            {
                string text = ((TextBox)sender).Text;
                GameInputBox.Brush = Brushes.White;
                inputBox = GameInputBox.UpdateTextBox(inputBox);

                if (text == $"/NATO")
                {
                    if (_natoWavEnabled == false)
                        _natoWavEnabled = true;
                    else if (_natoWavEnabled == true)
                        _natoWavEnabled = false;
                }

                if (text == $"/hitler")
                {
                    try
                    {
                        AudioHandler.PlayRandomHitlerSample();
                    }
                    catch 
                    {
                        ;
                    }
                }

                if (text == "")
                {
                    if (GameEngine.GameState.Phase == "[PHASE2]")
                    {
                        GameEngine.GameState.LetterWasRevealed = false;
                        SetAllButtons(false);

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

                else if (text.Contains(" ") || ((text.Length > 10) && !text.Contains("startserver")))
                    return;

                else if (text.Contains("startserver"))
                {
                    GameCom.InitServer();
                    _iAmServer = true;

                    if (_iAmServer)
                        GameCom.GameServer.StringReceivedEvent += StringReceived;
                    else
                        return;

                    GameInputBox.Text = "";
                    inputBox = GameInputBox.UpdateTextBox(inputBox);

                    Thread.Sleep(1000);
                    GameCom.GameServer.SendString("-GAME START-");

                    return;
                }

                else if (Char.IsDigit(text[0]))
                {
                    GameCom.InitClient(text);
                    _iAmServer = false;

                    if (_iAmServer == false)
                        GameCom.GameClient.StringReceivedEvent += StringReceived;
                    else
                        return;

                    inputBox.Text = "";
                    Thread.Sleep(1000);
                    GameCom.GameClient.SendString("-GAME START-");

                    return;
                }

                else if (GameEngine.GameState.Phase == "[PHASE1]")
                {
                    bool canAddToBoard = GameEngine.BoardHandler.CanAddToBoard(text);

                    using (StreamWriter sw = File.AppendText($"./memory.txt"))
                    {
                        sw.WriteLine(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " --- " + text + "\n");
                    }

                    if (canAddToBoard == false || WillBoardCoverageBeAbove(50, text))
                    {
                        GameEngine.GameState.Phase = "[PHASE2]";
                        SetAllButtons(false);

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
                    GameEngine.BoardHandler.AddToBoard(text);

                    chatBox = GameChatBox.Add(chatBox, text);

                    if (_iAmServer)
                    {
                        GameCom.GameServer.SendString(text);
                        GameCom.GameServer.SendString(GameEngine.GameState.ToString());
                    }

                    else
                    {
                        GameCom.GameClient.SendString(text);
                        GameCom.GameClient.SendString(GameEngine.GameState.ToString());
                    }

                    GameInputBox.OppTurn();
                    inputBox = GameInputBox.UpdateTextBox(inputBox);
                }

                else if (GameEngine.GameState.Phase == "[PHASE2]")
                {
                    if (GameEngine.GameState.GetWord(_lastClickedX, _lastClickedY).TheWord == text)
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
                    SetAllButtons(true);
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