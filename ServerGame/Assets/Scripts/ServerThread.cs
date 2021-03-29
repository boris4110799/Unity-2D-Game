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

    private Socket serverSocket;//伺服器本身的Socket
    private Socket[] clientSocket;//連線使用的Socket
    private Struct_Internet internet;
    private int SocketIndex = 0;
    public bool[] status;
    public string receiveMessage = null;//初始化接受的資料
    private Thread threadConnect = null;//連線的Thread

    public ServerThread(AddressFamily family, SocketType socketType, ProtocolType protocolType, string ip, int port)
    {
        serverSocket = new Socket(family, socketType, protocolType);
        internet.ip = ip;
        internet.port = port;
    }

    //開始傾聽連線需求
    public void Listen()
    {
        Array.Resize(ref clientSocket, 1); // 用 Resize 的方式動態增加 Socket 的數目
        Array.Resize(ref status, 1);

        serverSocket.Bind(new IPEndPoint(IPAddress.Parse(internet.ip), internet.port));//伺服器本身的IP和Port
        serverSocket.Listen(10);//最多一次接受多少人連線
        SocketWaitAccept();   // 另外寫一個函數用來分配 Client 端的 Socket
    }

    // 等待Client連線
    private void SocketWaitAccept()
    {
        // 判斷目前是否有空的 Socket 可以提供給Client端連線
        bool FlagFinded = false;

        for (int i=1;i<clientSocket.Length;i+=1)
        {
            // serverSocket[i] 若不為 null 表示已被實作過, 判斷是否有 Client 端連線
            if (clientSocket[i] != null)
            {
                // 如果目前第 i 個 Socket 若沒有人連線, 便可提供給下一個 Client 進行連線
                if (clientSocket[i].Connected == false)
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

        // 如果 FlagFinded 為 false 表示目前並沒有多餘的 Socket 可供 Client 連線
        if (FlagFinded == false)
        {
            // 增加 Socket 的數目以供下一個 Client 端進行連線
            SocketIndex = clientSocket.Length;
            Array.Resize(ref clientSocket, SocketIndex + 1);
            Array.Resize(ref status, SocketIndex + 1);
        }

        //由於連線成功之前程式都會停下，所以必須使用Thread
        threadConnect = new Thread(Accept)
        {
            IsBackground = true//設定為背景執行續，當程式關閉時會自動結束
        };
        threadConnect.Start();
    }

    private void Accept()
    {
        try
        {
            clientSocket[SocketIndex] = serverSocket.Accept();//等到Client端連線成功後才會往下執行
            //連線成功後，若是不想再接受其他連線，可以關閉serverSocket
            //serverSocket.Close();
            status[SocketIndex] = true;

            int tempIndex = SocketIndex;
            SocketWaitAccept();
            long dataLength;//儲存傳遞過來的"資料長度"
            byte[] bytes = new byte[1024 * 4];//用來儲存傳遞過來的資料
            
            while (true)
            {
                // 程式會被 hand 在此, 等待接收來自 Client 端傳來的資料
                if (clientSocket[tempIndex] != null && clientSocket[tempIndex].Connected == true)
                {
                    dataLength = clientSocket[tempIndex].Receive(bytes);//資料接收完畢之前都會停在這邊
                    receiveMessage = Encoding.UTF8.GetString(bytes);//將傳過來的資料解碼並儲存
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

    //停止連線
    public void StopConnect()
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

    //寄送訊息
    public void Send(int id, string message)
    {
        if (message == null)
        {
            throw new NullReferenceException("message不可為Null");
        }
        else
        {
            //for (int i = 0; i < clientSocket.Length; i += 1)
            //{
                try
                {
                    if (clientSocket[id] != null && clientSocket[id].Connected == true)//若成功連線才傳遞資料
                    {
                        //將資料進行編碼並轉為Byte後傳遞
                        clientSocket[id].Send(Encoding.UTF8.GetBytes(message));
                    }
                }
                catch (Exception)
                {

                }
            //}
        }
    }
}
