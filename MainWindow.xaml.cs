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
        public GameNetTCPIP GameNetTCPIP { get; set; } = new GameNetTCPIP();
        public Engine Engine { get; set; } = new Engine();
        public InputBox InputBox { get; set; } = new InputBox();
        public ChatBox ChatBox { get; set; } = new ChatBox();
        public AudioHandler AudioHandler { get; set; } = new AudioHandler();

        bool _natoWavEnabled = false;

        public MainWindow()
        {
            InitializeComponent();

            InitAllButtons();

            if (File.Exists($"./memory.txt") == false)
                File.Create($"./memory.txt");

            if (File.Exists($"./settings.cfg") == false)
                InitSettingsCfg();
        }

        private void InitSettingsCfg()
        {
            File.Create($"./settings.cfg");
            // TODO
        }

        private void Button_Click(object sender, MouseButtonEventArgs e)
        {
            if (Engine.ButtonsEnabled == false)
                return;

            InputBox.OppTurn();
            inputBox = InputBox.UpdateTextBox(inputBox);

            string buttonName = ((Button)sender).Name;
            int x = int.Parse(buttonName[1].ToString());
            int y = int.Parse(buttonName[2].ToString());

            Engine.LastClickedX = x;
            Engine.LastClickedY = y;

            Engine.ButtonsEnabled = false;
            bool wordWasRevealed = false;

            if (Engine.State.Reveal(x, y, out wordWasRevealed))
            {
                InputBox.MyTurn();
                inputBox = InputBox.UpdateTextBox(inputBox);

                if (wordWasRevealed)
                {
                    InputBox.OppTurn();
                    inputBox = InputBox.UpdateTextBox(inputBox);

                    if (GameNetTCPIP.IsServer)
                    {
                        Engine.State.PointsPlayerA = Engine.State.PointsPlayerA + 1;
                        chatBox = ChatBox.ShowGamePoints(chatBox, Engine.State.PointsPlayerA, Engine.State.PointsPlayerB);
                    }

                    else
                    {
                        Engine.State.PointsPlayerB = Engine.State.PointsPlayerB + 1;
                        chatBox = ChatBox.ShowGamePoints(chatBox, Engine.State.PointsPlayerB, Engine.State.PointsPlayerA);
                    }
                }
            }

            RenderRevealed();
            GameNetTCPIP.SendString(Engine.State.ToString());
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
                InputBox.Text = ((TextBox)sender).Text;
                InputBox.Brush = Brushes.White;
                inputBox = InputBox.UpdateTextBox(inputBox);

                if ((InputBox.Text == $"%NATO") && (AudioHandler.Initialized))
                {
                    if (_natoWavEnabled == false)
                        _natoWavEnabled = true;
                    else if (_natoWavEnabled == true)
                        _natoWavEnabled = false;

                    chatBox = ChatBox.Add(chatBox, InputBox.Text);
                    InputBox.Text = "";
                    inputBox = InputBox.UpdateTextBox(inputBox);

                    return;
                }

                if (InputBox.Text == "")
                {
                    if (Engine.State.Phase == "[PHASE2]")
                    {
                        Engine.State.LetterWasRevealed = false;
                        Engine.ButtonsEnabled = false;

                        if (GameNetTCPIP.IsServer)
                            chatBox = ChatBox.ShowGamePoints(chatBox, Engine.State.PointsPlayerA, Engine.State.PointsPlayerB);
                        else
                            chatBox = ChatBox.ShowGamePoints(chatBox, Engine.State.PointsPlayerB, Engine.State.PointsPlayerA);

                        InputBox.OppTurn();
                        inputBox = InputBox.UpdateTextBox(inputBox);

                        GameNetTCPIP.SendString(Engine.State.ToString());
                    }

                    else
                        return;
                }

                else if (InputBox.Text.Contains(" ") || ((InputBox.Text.Length > 10) && (InputBox.Text != $"%startserver")))
                    return;

                else if (InputBox.Text == $"%startserver")
                {
                    GameNetTCPIP.InitServer();
                    GameNetTCPIP.GameServer.StringReceivedEvent += StringReceived;

                    InputBox.Text = "";
                    inputBox = InputBox.UpdateTextBox(inputBox);

                    //Thread.Sleep(1000);
                    Engine.GameHasStarted = true;
                    GameNetTCPIP.SendString("-GAME START-");

                    return;
                }

                else if (Helpers.IsValidIP(InputBox.Text) && (Engine.GameHasStarted == false))
                {
                    GameNetTCPIP.InitAndConnectClient(InputBox.Text);
                    GameNetTCPIP.GameClient.StringReceivedEvent += StringReceived;

                    inputBox.Text = "";
                    //Thread.Sleep(1000);
                    Engine.GameHasStarted = true;
                    GameNetTCPIP.SendString("-GAME START-");

                    return;
                }

                else if (Engine.State.Phase == "[PHASE1]")
                {
                    using (StreamWriter sw = File.AppendText($"./memory.txt"))
                    {
                        sw.WriteLine(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " --- " + InputBox.Text + "\n");
                    }

                    if ((Engine.BoardHandler.CanAddToBoard(InputBox.Text) == false) || Helpers.WillBoardCoverageBeAbove(Engine.State, 50, InputBox.Text))
                    {
                        Engine.State.Phase = "[PHASE2]";
                        Engine.ButtonsEnabled = false;

                        if (GameNetTCPIP.IsServer)
                            chatBox = ChatBox.ShowGamePoints(chatBox, Engine.State.PointsPlayerA, Engine.State.PointsPlayerB);
                        else
                            chatBox = ChatBox.ShowGamePoints(chatBox, Engine.State.PointsPlayerB, Engine.State.PointsPlayerA);

                        InputBox.OppTurn();
                        inputBox = InputBox.UpdateTextBox(inputBox);

                        GameNetTCPIP.SendString(Engine.State.ToString());

                        return;
                    }

                    Engine.BoardHandler = new BoardHandler(Engine.State);
                    Engine.BoardHandler.AddToBoard(InputBox.Text);

                    chatBox = ChatBox.Add(chatBox, InputBox.Text);

                    GameNetTCPIP.SendString(InputBox.Text);
                    GameNetTCPIP.SendString(Engine.State.ToString());

                    InputBox.OppTurn();
                    inputBox = InputBox.UpdateTextBox(inputBox);
                }

                else if (Engine.State.Phase == "[PHASE2]")
                {
                    if (Engine.State.GetWord(Engine.LastClickedX, Engine.LastClickedY).TheWord == InputBox.Text)
                    {
                        Engine.State.RevealWord(Engine.LastClickedX, Engine.LastClickedY);
                        RenderRevealed();

                        if (GameNetTCPIP.IsServer)
                            Engine.State.PointsPlayerA = Engine.State.PointsPlayerA + 1;
                        else
                            Engine.State.PointsPlayerB = Engine.State.PointsPlayerB + 1;
                    }

                    Engine.State.LetterWasRevealed = false;

                    GameNetTCPIP.SendString(Engine.State.ToString());

                    if (GameNetTCPIP.IsServer)
                        chatBox = ChatBox.ShowGamePoints(chatBox, Engine.State.PointsPlayerA, Engine.State.PointsPlayerB);
                    else
                        chatBox = ChatBox.ShowGamePoints(chatBox, Engine.State.PointsPlayerB, Engine.State.PointsPlayerA);

                    InputBox.OppTurn();
                    inputBox = InputBox.UpdateTextBox(inputBox);
                }
            }
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
            chatBox = ChatBox.Add(chatBox, str);
        }

        private void HandleReceiveGameState(string str)
        {
            Engine.State = new State();
            Engine.State.FromString(str);
            //GameState.RevealAll();
            RenderRevealed();

            if (Engine.State.Phase == "[PHASE2]")
            {
                if (GameNetTCPIP.IsServer)
                    chatBox = ChatBox.ShowGamePoints(chatBox, Engine.State.PointsPlayerA, Engine.State.PointsPlayerB);
                else
                    chatBox = ChatBox.ShowGamePoints(chatBox, Engine.State.PointsPlayerB, Engine.State.PointsPlayerA);

                if (Engine.State.LetterWasRevealed == false)
                    Engine.ButtonsEnabled = true;
            }

            if (Engine.State.LetterWasRevealed == false || Engine.State.WordWasRevealed == true)
            {
                InputBox.MyTurn();
                inputBox = InputBox.UpdateTextBox(inputBox);
            }

            Engine.State.LetterWasRevealed = false;
            Engine.State.WordWasRevealed = false;
        }
    }
}