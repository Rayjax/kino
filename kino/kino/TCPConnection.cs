
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;


public class TCPConnection {

    TcpClient TCPClient;
    bool connected;
    int connectionTries = 10;
    String ipAddress;
    int port;

    public TCPConnection(String ipAddress, int port)
    {
        this.ipAddress = ipAddress;
        this.port = port;
        this.connected = false;
    }

    public bool connect()
    {
        try
        {
            Console.WriteLine("TCP Client initialising.....");
            this.TCPClient = new TcpClient();
            Console.WriteLine("TCP Client connecting.....");
            TCPClient.Connect(ipAddress, port);
            connected = true;
            Console.WriteLine("TCP Client connected");
        }

        catch (Exception e)
        {
            Console.WriteLine("TCP Client connection error..... " + e.StackTrace);
        }
        return connected;
    }

    public bool sendMessage(String message)
    {
        bool success = false;
        Console.WriteLine("TCP client starting to send message...");
        if (connected)
        {
            try
            {
                Stream stm = TCPClient.GetStream();

                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] ba = asen.GetBytes(message);
                Console.WriteLine("TCP client sending message.....");

                stm.Write(ba, 0, ba.Length);

                byte[] bb = new byte[100];
                int k = stm.Read(bb, 0, 100);

                for (int i = 0; i < k; i++)
                    Console.Write(Convert.ToChar(bb[i]));
                success = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("TCP Client cannot send message : " + ex.StackTrace);
                connected = false;
                success = false;
            }
        }
        else
        {
            Console.WriteLine("TCP Client cannot send message : TCP client not connected.");
        }
        return success;
    }

    public bool disconnect()
    {
        bool success = false;
        if (connected)
        {
            try
            {
                Console.WriteLine("TCP Client trying to disconnect");
                TCPClient.Close();
                success = true;
                Console.WriteLine("TCP Client disconnected");
            }
            catch (Exception ex)
            {
                Console.WriteLine("TCP Client cannot disconnect : " + ex.StackTrace);
            }
        }
        else
        {
            Console.WriteLine("TCP Client cannot disconnect : is not connected");
        }
        return success;
    }

    public bool connectSendMessageAndDisconnect(String message)
    {
        bool messageSent = false;

        //Connect
        this.connect();

        //Send message
        messageSent = this.sendMessage(message);

        //Disconnect
        this.disconnect();

        return messageSent;
    }
}
