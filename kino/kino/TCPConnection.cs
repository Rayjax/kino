
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

    public TCPConnection()
    {
        this.connected = false;
        this.TCPClient = new TcpClient();
    }

    public bool connect(String ipAddress, int port)
    {
        this.ipAddress = ipAddress;
        this.port = port;
        try
        {
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

    public void reconnect()
    {
        try
        {
            Console.WriteLine("TCP Client reconnecting.....");
            TCPClient.Connect(ipAddress, port);
            connected = true;
            Console.WriteLine("TCP Client reconnected");
        }
        catch (Exception e)
        {
            Console.WriteLine("TCP Client reconnection error..... " + e.StackTrace);
        }
    }

    public bool sendMessage(String message)
    {
        bool success = false;
        Console.WriteLine("sending message via TCP...");
        int tryCounter = 1;
        while (!success && tryCounter < this.connectionTries)
        {
            if (connected)
            {
                try
                {
                    Stream stm = TCPClient.GetStream();

                    ASCIIEncoding asen = new ASCIIEncoding();
                    byte[] ba = asen.GetBytes(message);
                    Console.WriteLine("Transmitting.....");

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
                Console.WriteLine("TCP Client cannot send message : TCP client not connected. Trying reconnect number " + tryCounter);
                this.reconnect();
            }
            tryCounter++;
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
            Console.WriteLine("TCP Client cannot disconnect : not connected yet");
        }
        return success;
    }

}
