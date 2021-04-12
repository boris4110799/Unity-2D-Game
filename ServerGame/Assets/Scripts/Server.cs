using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Linq;

public class Server : MonoBehaviour
{
    private ServerThread st;
    public ServerData Sdata = new ServerData();     //紀錄伺服器發送的資料
    public ClientData[] Cdata = new ClientData[10]; //紀錄玩家發送的資料

    public GameObject basicplayer;      //玩家樣本
    public GameObject bulletPrefab;     //子彈樣本
    public GameObject hpbarPrefab;      //血條樣本
    public Text text;                   //傳遞子彈擊中的資訊
    public Text IP;                     //傳遞使用者輸入的IP
    public float movementSpeed;         //設定單位移動速度
    public float minPosX;               //設定移動的左界
    public float maxPosX;               //設定移動的右界
    public float minPosY;               //設定移動的下界
    public float maxPosY;               //設定移動的上界

    private GameObject playerf;         //玩家變數
    private GameObject player;          //玩家變數
    private GameObject hpbar;           //血條變數
    private string ipaddress;           //儲存本機IP
    private string msg;                 //訊息變數
    private bool[] isplayercreate = new bool[10];   //紀錄玩家在伺服器端是否被建立
    private bool[] isMove = new bool[10];           //紀錄玩家是否可進行移動
    private float[] playerhp = new float[10];       //紀錄玩家的血量

    private void Start()
    {
        //取得伺服器的IP位置
        IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
        ipaddress = hostEntry.AddressList.ToList().Where(p => p.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault().ToString();

        //開始連線，設定使用網路、串流、TCP
        st = new ServerThread(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, ipaddress, 8888);
        st.Listen(); //讓Server socket開始監聽連線

        IP.text = "IP : " + ipaddress; //顯示IP位置

        basicplayer.SetActive(false); //將玩家樣本設定為隱藏
    }
    
    private void FixedUpdate()
    {
        for (int i = 0; i < 10; i += 1)
        {
            if (isplayercreate[i] && isMove[i])
            {
                //尋找特定的玩家和血條
                string playername = "player" + Convert.ToString(Cdata[i].id);
                playerf = GameObject.Find(playername);
                string hpname = "hpbar" + Convert.ToString(Cdata[i].id);
                hpbar = GameObject.Find(hpname);

                //移動的距離
                float movementx = 0;
                float movementy = 0;
                if (Cdata[i].presskey == "LeftArrow")
                {
                    movementx = -1 * movementSpeed * Time.deltaTime;
                    movementy = 0 * movementSpeed * Time.deltaTime;
                }
                else if (Cdata[i].presskey == "RightArrow")
                {
                    movementx = 1 * movementSpeed * Time.deltaTime;
                    movementy = 0 * movementSpeed * Time.deltaTime;
                }
                else if (Cdata[i].presskey == "UpArrow")
                {
                    movementx = 0 * movementSpeed * Time.deltaTime;
                    movementy = 1 * movementSpeed * Time.deltaTime;
                }
                else if (Cdata[i].presskey == "DownArrow")
                {
                    movementx = 0 * movementSpeed * Time.deltaTime;
                    movementy = -1 * movementSpeed * Time.deltaTime;
                }

                //更新玩家座標與血條位置
                playerf.transform.position = new Vector2(Mathf.Clamp(playerf.transform.position.x + movementx, minPosX, maxPosX), Mathf.Clamp(playerf.transform.position.y + movementy, minPosY, maxPosY));
                hpbar.transform.position = new Vector2(playerf.transform.position.x - 2, playerf.transform.position.y - 1.5f);

                //更新玩家旋轉方向
                float z_angle = playerf.transform.rotation.eulerAngles.z;
                if (Cdata[i].presskey == "LeftArrow")
                {
                    if (z_angle > 180) z_angle -= 360;
                    if (z_angle + 90 >= 0) player.transform.Rotate(0, 0, (90 - z_angle) / 5);
                    else player.transform.Rotate(0, 0, (-270 - z_angle) / 5);
                }
                else if (Cdata[i].presskey == "RightArrow")
                {
                    if (270 - z_angle <= 180) player.transform.Rotate(0, 0, (270 - z_angle) / 5);
                    else player.transform.Rotate(0, 0, (-90 - z_angle) / 5);
                }
                else if (Cdata[i].presskey == "UpArrow")
                {
                    if (360 - z_angle <= 180) player.transform.Rotate(0, 0, (360 - z_angle) / 5);
                    else player.transform.Rotate(0, 0, (0 - z_angle) / 5);
                }
                else if (Cdata[i].presskey == "DownArrow")
                {
                    if (180 - z_angle <= 180) player.transform.Rotate(0, 0, (180 - z_angle) / 5);
                    else player.transform.Rotate(0, 0, (-180 - z_angle) / 5);
                }
                else player.transform.Rotate(0, 0, 0);

                isMove[i] = false; //結束移動
            }
        }
    }

    private void Update()
    {
        if (st.receiveMessage != null)
        {
            string[] sArray = st.receiveMessage.Split(new string[] { "{", "}" }, StringSplitOptions.RemoveEmptyEntries); //分割訊息

            //遍歷所有字串
            foreach (string tempstr in sArray)
            {
                if (tempstr.Contains("\"id\":") == false) continue; //如果字串不包含"id":則跳過

                //將結構體反序列化
                string datastr = "{" + tempstr + "}";
                Debug.Log(datastr);
                ClientData tempdata = JsonSerializer.Deserialize<ClientData>(datastr);

                //設定新玩家的編號
                if (tempdata.id == -1)
                {
                    tempdata.id = CreatePlayer();
                }

                int id = tempdata.id;
                Cdata[id] = tempdata;

                //尋找特定的玩家
                string playername = "player" + Convert.ToString(id);
                player = GameObject.Find(playername);

                isMove[id] = true; //設定該玩家為可移動狀態

                //發射子彈
                if (Cdata[id].spawning)
                {
                    Instantiate(bulletPrefab, new Vector2(player.transform.position.x - Mathf.Sin(player.transform.rotation.eulerAngles.z / 180 * Mathf.PI), player.transform.position.y + Mathf.Cos(player.transform.rotation.eulerAngles.z / 180 * Mathf.PI)), player.transform.rotation);
                }
            }

            st.receiveMessage = null; //清空訊息
        }

        CheckPlayer();

        //發送訊息給所有玩家
        WriteMessage();
        //Debug.Log(msg);
        for (int i = 1; i < 10; i += 1)
        {
            if (isplayercreate[i] == true && st.Connected(i))
            {
                st.Send(i, Convert.ToString(i) + msg);
            }
        }

        CheckText();
    }

    private void WriteMessage() //編寫訊息
    {
        msg = null;
        for (int i=1;i<10;i+=1)
        {
            if (isplayercreate[i] == true && st.Connected(i))
            {
                string playername = "player" + Convert.ToString(Cdata[i].id);
                player = GameObject.Find(playername);
                Sdata.id = Cdata[i].id;
                Sdata.position_x = player.transform.position.x;
                Sdata.position_y = player.transform.position.y;
                Sdata.z_angle = player.transform.rotation.eulerAngles.z;
                Sdata.hp = playerhp[i];
                Sdata.spawning = Cdata[i].spawning;
                var json = JsonSerializer.Serialize(Sdata);
                msg += json;
            }
        }
    }

    private int CreatePlayer() //建構新玩家並給予編號
    {
        for (int i=1;i<10;i+=1)
        {
            if (st.Connected(i) == true && isplayercreate[i] == false)
            {
                isplayercreate[i] = true;   //設定玩家為已建立
                playerhp[i] = 100;          //設定玩家血量為100

                //建構新玩家
                basicplayer.SetActive(true);
                player = Instantiate(basicplayer, new Vector2(0, 0), Quaternion.identity) as GameObject;
                player.name = "player" + Convert.ToString(i);
                basicplayer.SetActive(false);

                //建構新血條
                hpbar = Instantiate(hpbarPrefab, new Vector2(player.transform.position.x - 2, player.transform.position.y - 1.5f), Quaternion.identity) as GameObject;
                hpbar.name = "hpbar" + Convert.ToString(i);
                
                return i; //回傳玩家編號
            }
        }
        return -1;
    }

    private void CheckPlayer() //檢查玩家是否已退出
    {
        for (int i = 1; i < 10; i += 1)
        {
            if (isplayercreate[i] == true && st.Connected(i) == false)
            {
                isplayercreate[i] = false; //設定玩家為已消失

                //移除特定的玩家和血條
                string tempstr = "player" + Convert.ToString(i);
                Destroy(GameObject.Find(tempstr));
                tempstr = "hpbar" + Convert.ToString(i);
                Destroy(GameObject.Find(tempstr));
            }
        }
    }

    private void CheckText() //根據子彈擊中資訊扣除玩家血量
    {
        string str = text.text;
        if (str.Contains("player"))
        {
            int id = Convert.ToInt32(str.Substring(6));
            playerhp[id] -= 10; //更新玩家血量

            //更新特定玩家的血條值
            string hpname = "hpbar" + Convert.ToString(id);
            GameObject bar = GameObject.Find(hpname).transform.GetChild(0).gameObject;
            bar.transform.localScale = new Vector3(playerhp[id] / 100f, bar.transform.localScale.y, bar.transform.localScale.z);
        }
        text.text = null; //清空訊息
    }

    private void OnApplicationQuit() //應用程式結束時自動關閉連線
    {
        st.StopConnect();
    }
}
