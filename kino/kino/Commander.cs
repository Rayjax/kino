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
            this.UDPmessages[1] = 'f';
            this.UDPmessages[2] = 'b';
            this.UDPmessages[3] = 'l';
            this.UDPmessages[4] = 'r';
        }

        public bool sendCommand(Command command)
        {
            char test = this.UDPmessages[(int)command];
            return this.UDPmessager.sendMessage(test);
        }
    }
}
