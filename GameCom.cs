namespace BrainStorm
{
    public class GameCom
    {
        public GameServer? GameServer { get; set; } = null;
        public GameClient? GameClient { get; set; } = null;

        public void InitServer()
        {
            GameServer = new GameServer();
        }

        public void InitClient(string serverIP)
        {
            GameClient = new GameClient();
            GameClient.Connect(serverIP);
        }
    }
}
