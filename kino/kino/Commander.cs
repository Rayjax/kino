using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace kino
{
    class Commander
    {
        private UDPmessager UDPmessager;
        private char[] UDPmessages;
        public Commander(String targetIP, int targetPort) 
        {
            this.UDPmessager = new UDPmessager(targetIP, targetPort);
            this.UDPmessages = new char[5];
            this.UDPmessages[0] = 's';
            this.UDPmessages[1] = 'a';
            this.UDPmessages[2] = 'r';
            this.UDPmessages[3] = 'g';
            this.UDPmessages[4] = 'd';
        }

        public bool sendCommand(Command command)
        {
            char test = this.UDPmessages[(int)command];
            return this.UDPmessager.sendMessage(test);
        }
    }
}
