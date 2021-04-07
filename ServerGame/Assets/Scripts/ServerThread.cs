using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class ServerThread
{
    public struct Struct_Internet
    {
        public string ip;
        public int port;
    }

    private Socket serverSocket;            //伺服器本身的Socket
    private Socket[] clientSocket;          //連線使用的Socket
    private Struct_Internet internet;
    private int SocketIndex = 0;            //紀錄Socket的編號
    public bool[] status;                   //紀錄Socket的狀態
    public string receiveMessage = null;    //初始化接受的資料
    private Thread threadConnect = null;    //連線的Thread

    public ServerThread(AddressFamily family, SocketType socketType, ProtocolType protocolType, string ip, int port)
    {
        serverSocket = new Socket(family, socketType, protocolType);
        internet.ip = ip;
        internet.port = port;
    }

    public void Listen() //開始傾聽連線需求
    {
        Array.Resize(ref clientSocket, 1);  //動態增加Socket的數目
        Array.Resize(ref status, 1);        //動態增加status的數目

        serverSocket.Bind(new IPEndPoint(IPAddress.Parse(internet.ip), internet.port)); //伺服器本身的IP和Port
        serverSocket.Listen(10);    //最多一次接受多少人連線
        SocketWaitAccept();         //另外寫一個函式用來分配Client端的Socket
    }

    private void SocketWaitAccept() //等待Client連線
    {
        // 判斷目前是否有空的 Socket 可以提供給Client端連線
        bool FlagFinded = false;
        for (int i=1;i<clientSocket.Length;i+=1)
        {
            if (clientSocket[i] != null) //serverSocket[i]若不為null表示已被實作過，判斷是否有Client端連線
            {
                if (clientSocket[i].Connected == false) //如果目前第i個Socket若沒有人連線，便可提供給下一個Client進行連線
                {
                    status[i] = false;
                    if (FlagFinded == false)
                    {
                        SocketIndex = i;
                        FlagFinded = true;
                    }
                }
                else
                {
                    status[i] = true;
                }
            }
        }

        //分配更多的Socket以供下一個Client連線
        if (FlagFinded == false)
        {
            SocketIndex = clientSocket.Length;
            Array.Resize(ref clientSocket, SocketIndex + 1);
            Array.Resize(ref status, SocketIndex + 1);
        }

        //建立Thread進行連線
        threadConnect = new Thread(Accept)
        {
            IsBackground = true //設定為背景執行續，當程式關閉時會自動結束
        };
        threadConnect.Start();
    }

    private void Accept()
    {
        try
        {
            clientSocket[SocketIndex] = serverSocket.Accept(); //等到Client端連線成功後才會往下執行
            
            status[SocketIndex] = true;

            int tempIndex = SocketIndex;
            SocketWaitAccept();
            long dataLength;                    //儲存傳遞過來的資料長度
            byte[] bytes = new byte[1024 * 4];  //用來儲存傳遞過來的資料
            
            while (true)
            {
                //接收來自Client端傳來的資料
                if (clientSocket[tempIndex] != null && clientSocket[tempIndex].Connected == true)
                {
                    dataLength = clientSocket[tempIndex].Receive(bytes);    //資料接收完畢之前都會停在這邊
                    receiveMessage = Encoding.UTF8.GetString(bytes);        //將傳過來的資料解碼並儲存
                }
                else
                {
                    status[tempIndex] = false;
                    break;
                }
            }
        }
        catch (Exception)
        {

        }
    }

    public void StopConnect() //停止連線
    {
        for (int i = 0; i < clientSocket.Length; i += 1)
        {
            try
            {
                clientSocket[i].Close();
                status[i] = false;
            }
            catch (Exception)
            {

            }
        }
    }

    public void Send(int id, string message) //傳送訊息
    {
        if (message == null)
        {
            throw new NullReferenceException("message不可為Null");
        }
        else
        {
            try
            {
                //若成功連線才傳遞資料
                if (clientSocket[id] != null && clientSocket[id].Connected == true) 
                {
                    clientSocket[id].Send(Encoding.UTF8.GetBytes(message)); //將資料進行編碼並轉為Byte後傳遞
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
