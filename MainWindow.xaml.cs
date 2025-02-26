using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OrdSpel2
{
    public partial class MainWindow : Window
    {
        public Helpers Helpers { get; set; } = new Helpers();

        public GameState GameState { get; set; } = new GameState();
        public GameCom GameCom { get; set; } = new GameCom();

        public BoardHandler BoardHandler { get; set; }

        int _lastClickedX = 0;
        int _lastClickedY = 0;

        bool _buttonsEnabled = false;

        bool _natoWav = false;
        List<ISampleProvider> _selectedSamples = new List<ISampleProvider>();
        ConcatenatingSampleProvider? _fullAudio;
        WaveOutEvent _wavPlayer = new WaveOutEvent();

        public MainWindow()
        {
            InitializeComponent();

            SetButtonProperties();
            SetAllButtons(false);

            BoardHandler = new BoardHandler(Helpers, GameState);
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
            if (_natoWav)
                handleQWERTYAudio(e.Key);

            if (e.Key == Key.Enter)
            {
                inputBox.Background = Brushes.White;
                string text = ((TextBox)sender).Text;

                if (text == $"/NATO")
                    _natoWav = true;

                if (text == $"/hitler")
                {
                    _selectedSamples.Add(new AudioFileReader(new AudioSample("Hitler01", ".//wav//hitler//Hitler_01.wav").Path));
                    _selectedSamples.Add(new AudioFileReader(new AudioSample("Hitler02", ".//wav//hitler//Hitler_02.wav").Path));
                    _selectedSamples.Add(new AudioFileReader(new AudioSample("Hitler03", ".//wav//hitler//Hitler_03.wav").Path));
                    _selectedSamples.Add(new AudioFileReader(new AudioSample("Hitler04", ".//wav//hitler//Hitler_04.wav").Path));
                    _selectedSamples.Add(new AudioFileReader(new AudioSample("Hitler_LongLive", ".//wav//hitler//Hitler_LongLive.wav").Path));

                    Random rnd = new Random();
                    int idx = rnd.Next(0, 5);

                    var _oneSelectedSamples = new List<ISampleProvider>();
                    var _selectedSample = _selectedSamples[idx];
                    _oneSelectedSamples.Add(_selectedSample);

                    _fullAudio = new ConcatenatingSampleProvider(_oneSelectedSamples);

                    try
                    {
                        _wavPlayer.Init(_fullAudio);
                        _wavPlayer.Play();
                    }
                    catch
                    {
                        ;
                    }
                }

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

                else if (text.Contains(" "))
                    return;

                else if (text.Contains("startserver"))
                {
                    GameCom.InitServer();
                    GameCom.GameServer.StringReceivedEvent += StringReceived;

                    inputBox.Text = "";

                    Thread.Sleep(1000);

                    GameCom.GameServer.SendString("-GAME START-");

                    return;
                }

                else if (Char.IsDigit(text[0]))
                {
                    GameCom.InitClient(text);
                    GameCom.GameClient.StringReceivedEvent += StringReceived;

                    inputBox.Text = "";

                    Thread.Sleep(1000);

                    GameCom.GameClient.SendString("-GAME START-");

                    return;
                }

                else if (GameState.Phase == "[PHASE1]")
                {
                    bool canAddToBoard = BoardHandler.CanAddToBoard(text);

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

        private void handleQWERTYAudio(Key key)
        {
            if (key == Key.A)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("AlphaSample", ".//wav//NATO//ALPHA.wav").Path));
            else if (key == Key.B)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("BravoSample", ".//wav//NATO//BRAVO.wav").Path));
            else if (key == Key.C)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("CharlieSample", ".//wav//NATO//CHARLIE.wav").Path));
            else if (key == Key.D)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("DeltaSample", ".//wav//NATO//DELTA.wav").Path));
            else if (key == Key.E)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("EchoSample", ".//wav//NATO//ECHO.wav").Path));
            else if (key == Key.F)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("FoxtrotSample", ".//wav//NATO//FOXTROT.wav").Path));
            else if (key == Key.G)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("GolfSample", ".//wav//NATO//GOLF.wav").Path));
            else if (key == Key.H)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("HotelSample", ".//wav//NATO//HOTEL.wav").Path));
            else if (key == Key.I)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("IndiaSample", ".//wav//NATO//INDIA.wav").Path));
            else if (key == Key.J)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("JulietSample", ".//wav//NATO//JULIET.wav").Path));
            else if (key == Key.K)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("KiloSample", ".//wav//NATO//KILO.wav").Path));
            else if (key == Key.L)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("LimaSample", ".//wav//NATO//LIMA.wav").Path));
            else if (key == Key.M)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("MikeSample", ".//wav//NATO//MIKE.wav").Path));
            else if (key == Key.N)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("NovemberSample", ".//wav//NATO//NOVEMBER.wav").Path));
            else if (key == Key.O)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("OscarSample", ".//wav//NATO//OSCAR.wav").Path));
            else if (key == Key.P)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("PapaSample", ".//wav//NATO//PAPA.wav").Path));
            else if (key == Key.Q)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("QuebecSample", ".//wav//NATO//QUEBEC.wav").Path));
            else if (key == Key.R)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("RomeoSample", ".//wav//NATO//ROMEO.wav").Path));
            else if (key == Key.S)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("SierraSample", ".//wav//NATO//SIERRA.wav").Path));
            else if (key == Key.T)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("TangoSample", ".//wav//NATO//TANGO.wav").Path));
            else if (key == Key.U)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("UniformSample", ".//wav//NATO//UNIFORM.wav").Path));
            else if (key == Key.V)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("VictorSample", ".//wav//NATO//VICTOR.wav").Path));
            else if (key == Key.W)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("WhiskeySample", ".//wav//NATO//WHISKEY.wav").Path));
            else if (key == Key.X)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("XraySample", ".//wav//NATO//XRAY.wav").Path));
            else if (key == Key.Y)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("YankeeSample", ".//wav//NATO//YANKEE.wav").Path));
            else if (key == Key.Z)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("ZuluSample", ".//wav//NATO//ZULU.wav").Path));

            _fullAudio = new ConcatenatingSampleProvider(_selectedSamples);

            try
            {
                _wavPlayer.Init(_fullAudio);
                _wavPlayer.Play();
            }
            catch 
            {
                ;
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