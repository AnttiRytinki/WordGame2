using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OrdSpel2
{
    public class GameServer
    {
        public TcpListener Server { get; set; } = new TcpListener(IPAddress.Any, 41748);
        public TcpClient Client { get; set; } = new TcpClient();

        Thread receiveThread;

        public event EventHandler<string>? StringReceivedEvent;

        public GameServer()
        {
            Server.Start();
            Client = Server.AcceptTcpClient();

            receiveThread = new Thread(ReceiveLoop);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }

        public bool SendString(string str)
        {
            try
            {
                byte[] sendBytes = UTF8Encoding.UTF8.GetBytes(str);
                Client.GetStream().Write(sendBytes, 0, sendBytes.Length);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ReceiveLoop()
        {
            while (Client.Connected == true)
            {
                byte[] receivedBytes = new byte[Client.ReceiveBufferSize];
                string receivedString = "";

                try
                {
                    int i = Client.GetStream().Read(receivedBytes, 0, receivedBytes.Length);

                    if (i == 0)
                    {
                        return;
                    }

                    receivedString = Encoding.UTF8.GetString(receivedBytes, 0, i);
                    StringReceivedEvent?.Invoke(this, receivedString);
                }
                catch (Exception)
                {
                    return;
                }
            }
        }
    }
}
