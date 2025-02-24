using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OrdSpel2
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
