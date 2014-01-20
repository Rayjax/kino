using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace kino
{
    class UDPmessager
    {
        private Socket socket;
        private IPEndPoint endPoint;

        public UDPmessager(String targetIP, int targetPort)
        {
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPAddress serverAddr = IPAddress.Parse(targetIP);
            this.endPoint = new IPEndPoint(serverAddr, targetPort);
        }

        public bool sendMessage(String message){
            bool success = true;
            try
            {
                byte[] send_buffer = Encoding.ASCII.GetBytes(message);
                this.socket.SendTo(send_buffer, this.endPoint);
            }
            catch (Exception ex)
            {
                success = false;
            }
            return success;
        }
    }
}
