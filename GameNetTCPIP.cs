namespace BrainStorm
{
    public class GameNetTCPIP
    {
        public GameServer? GameServer { get; set; } = null;
        public GameClient? GameClient { get; set; } = null;

        public void InitServer()
        {
            GameServer = new GameServer();
        }

        public void InitAndConnectClient(string serverIP)
        {
            GameClient = new GameClient();
            GameClient.Connect(serverIP);
        }
    }
}
