using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class ClientThread
{
    public struct Struct_Internet
    {
        public string ip;
        public int port;
    }

    private Socket clientSocket;//連線使用的Socket
    private Struct_Internet internet;
    public string receiveMessage;
    private Thread threadReceive;
    private Thread threadConnect;

    public ClientThread(AddressFamily family, SocketType socketType, ProtocolType protocolType, string ip, int port)
    {
        clientSocket = new Socket(family, socketType, protocolType);
        internet.ip = ip;
        internet.port = port;

        threadConnect = new Thread(Accept)
        {
            IsBackground = true
        };
        threadConnect.Start();

        receiveMessage = null;

        threadReceive = new Thread(Receive)
        {
            IsBackground = true
        };
        threadReceive.Start();
    }

    private void Accept()
    {
        try
        {
            clientSocket.Connect(IPAddress.Parse(internet.ip), internet.port);//等待連線，若未連線則會停在這行
        }
        catch (Exception)
        {
        }
    }

    private void Receive()
    {
        long dataLength;
        byte[] bytes = new byte[1024 * 4];

        while (true)
        {
            if (clientSocket != null && clientSocket.Connected == true)
            {
                dataLength = clientSocket.Receive(bytes);
                receiveMessage = Encoding.UTF8.GetString(bytes);
            }
        }
    }

    public void Send(string message)
    {
        if (message == null)
        {
            throw new NullReferenceException("message不可為Null");
        }
        else
        {
            try
            {
                if (clientSocket != null && clientSocket.Connected == true)
                {
                    clientSocket.Send(Encoding.UTF8.GetBytes(message));
                }
            }
            catch (Exception)
            {

            }
        }
    }

    public bool Connected()
    {
        if (clientSocket != null && clientSocket.Connected == true) return true;
        else return false;
    }

    public void StopConnect()
    {
        try
        {
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
        catch (Exception)
        {

        }
    }
}
