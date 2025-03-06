
namespace BrainStorm
{
    public class GameNetTCPIP
    {
        public GameServer? GameServer { get; set; } = null;
        public GameClient? GameClient { get; set; } = null;

        public bool IsServer { get; set; } = false;

        public void InitServer()
        {
            GameServer = new GameServer();
            IsServer = true;
        }

        public void InitAndConnectClient(string serverIP)
        {
            GameClient = new GameClient();
            GameClient.Connect(serverIP);
            IsServer = false;
        }

        public void SendString(string str)
        {
            if (IsServer)
            {
                if (GameServer == null)
                {
                    // SHOW ERROR
                    return;
                }

                GameServer.SendString(str);
            }

            else
            {
                if (GameClient == null)
                {
                    // SHOW ERROR
                    return;
                }

                GameClient.SendString(str);
            }
        }
    }
}
