using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace kino
{
    class Commander
    {
        private UDPmessager UDPmessager;
        private TCPConnection TCPClient;
        private String[] messages;
        private Char commandDelimiter = '#';
        private bool transportIsUDP;
        public bool clientisConnected;

        public Commander(String targetIP, int targetPort, bool transportIsUDP) 
        {
            this.transportIsUDP = transportIsUDP;

            //Init messages
            this.messages = new String[5];
            this.messages[0] = "s";
            this.messages[1] = "a";
            this.messages[2] = "r";
            this.messages[3] = "g";
            this.messages[4] = "d";

            //Create communication client
            if (transportIsUDP)
            {
                this.UDPmessager = new UDPmessager(targetIP, targetPort);
                this.clientisConnected = true;
            }
            else
            {
                this.TCPClient = new TCPConnection();
                this.clientisConnected = this.TCPClient.connect(targetIP, targetPort);
            }
        }

        public bool sendCommand(Command command)
        {
            bool success = false;

            //Set the message to the corresponding command and apply the standard delimiter
            String message = this.messages[(int)command] + this.commandDelimiter;

            if (this.transportIsUDP)
            {
                //Use UDP
                success = this.UDPmessager.sendMessage(message);
            }
            else
            {
                //Use TCP
                success = this.TCPClient.sendMessage(message);
            }
            return success;
        }

        public void closeConnection()
        {
            if (!transportIsUDP && clientisConnected)
            {
                this.TCPClient.disconnect();
            }
        }
    }
}
