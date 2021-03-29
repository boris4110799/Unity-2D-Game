using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class Server : MonoBehaviour
{
    private ServerThread st;
    public ServerData Sdata = new ServerData();
    public ClientData[] Cdata = new ClientData[10];
    //private bool isSend;//儲存是否發送訊息完畢

    public GameObject basicplayer;
    public GameObject bulletPrefab;
    private GameObject newplayer;
    private int playernumber;
    private bool[] isplayercreate = new bool[10];
    private float[] playerhp = new float[10];
    
    private bool connected = true;
    private GameObject player;
    public Text text;
    public Text IP;
    //public Text IPinput;
    private string ipaddress;
    private string msg;

    public float movementSpeed;
    float movementx = 0;
    float movementy = 0;
    private bool[] isMove = new bool[10];
    public float minPosX;
    public float maxPosX;
    public float minPosY;
    public float maxPosY;

    private void Start()
    {
        IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
        ipaddress = hostEntry.AddressList.ToList().Where(p => p.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault().ToString();

        //開始連線，設定使用網路、串流、TCP
        st = new ServerThread(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, ipaddress, 8000);
        st.Listen();//讓Server socket開始監聽連線

        IP.text = "IP : " + ipaddress;

        
    }
    
    private void FixedUpdate()
    {
        if (connected)
        {
            for (int i = 0; i < 10; i += 1)
            {
                if (isplayercreate[i] && isMove[i])
                {
                    string playername = "player" + Convert.ToString(Cdata[i].id);
                    player = GameObject.Find(playername);

                    //移動的距離
                    movementx = Cdata[i].horizontal * movementSpeed * Time.deltaTime;
                    movementy = Cdata[i].vertical * movementSpeed * Time.deltaTime;

                    //更新座標
                    player.transform.position = new Vector2(Mathf.Clamp(player.transform.position.x + movementx, minPosX, maxPosX), Mathf.Clamp(player.transform.position.y + movementy, minPosY, maxPosY));
                    isMove[i] = false;
                }
            }
        }
    }

    private void Update()
    {
        if (connected)
        {
            //CreatePlayer();

            if (st.receiveMessage != null)
            {
                //Debug.Log("Client:" + st.receiveMessage);
                string[] sArray = st.receiveMessage.Split(new string[] { "{", "}" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string tempstr in sArray)
                {
                    if (tempstr.Contains("\"id\":") == false) continue;

                    string datastr = "{" + tempstr + "}";
                    Debug.Log(datastr);
                    ClientData tempdata = JsonSerializer.Deserialize<ClientData>(datastr);

                    if (tempdata.id == -1)
                    {
                        tempdata.id = CreatePlayer();
                    }

                    int id = tempdata.id;
                    Cdata[id] = tempdata;

                    string playername = "player" + Convert.ToString(Cdata[id].id);
                    player = GameObject.Find(playername);
                    isMove[id] = true;

                    float z_angle = player.transform.rotation.eulerAngles.z;
                    if (Cdata[id].presskey == "LeftArrow")
                    {
                        if (z_angle > 180) z_angle -= 360;
                        if (z_angle + 90 >= 0) player.transform.Rotate(0, 0, (90 - z_angle) / 5);
                        else player.transform.Rotate(0, 0, (-270 - z_angle) / 5);
                    }
                    else if (Cdata[id].presskey == "RightArrow")
                    {
                        if (270 - z_angle <= 180) player.transform.Rotate(0, 0, (270 - z_angle) / 5);
                        else player.transform.Rotate(0, 0, (-90 - z_angle) / 5);
                    }
                    else if (Cdata[id].presskey == "UpArrow")
                    {
                        if (360 - z_angle <= 180) player.transform.Rotate(0, 0, (360 - z_angle) / 5);
                        else player.transform.Rotate(0, 0, (0 - z_angle) / 5);
                    }
                    else if (Cdata[id].presskey == "DownArrow")
                    {
                        if (180 - z_angle <= 180) player.transform.Rotate(0, 0, (180 - z_angle) / 5);
                        else player.transform.Rotate(0, 0, (-180 - z_angle) / 5);
                    }
                    else player.transform.Rotate(0, 0, 0);

                    if (Cdata[id].spawning)
                    {
                        Instantiate(bulletPrefab, new Vector2(player.transform.position.x - Mathf.Sin(z_angle / 180 * Mathf.PI), player.transform.position.y + Mathf.Cos(z_angle / 180 * Mathf.PI)), player.transform.rotation);
                    }

                    /*Sdata.id = Cdata[id].id;
                    Sdata.position_x = player.transform.position.x;
                    Sdata.position_y = player.transform.position.y;
                    Sdata.z_angle = player.transform.rotation.eulerAngles.z;
                    Sdata.hp = playerhp[id];
                    var json = JsonSerializer.Serialize(Sdata);
                    Debug.Log(json);
                    st.Send(Sdata.id, json);*/
                }

                st.receiveMessage = null;
            }

            WriteMessage();
            for (int i = 1; i < st.status.Length; i += 1)
            {
                if (st.status[i] == true && isplayercreate[i] == true)
                    st.Send(i, Convert.ToString(i) + msg);
            }

            CheckPlayer();
            //CheckText();
        }
    }

    private void WriteMessage()
    {
        msg = null;
        for (int i=1;i<st.status.Length; i+=1)
        {
            if (st.status[i] == true && isplayercreate[i] == true)
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

    private int CreatePlayer()
    {
        for (int i=1;i<st.status.Length;i+=1)
        {
            if (st.status[i] == true && isplayercreate[i] == false)
            {
                isplayercreate[i] = true;
                playerhp[i] = 100;
                basicplayer.SetActive(true);
                newplayer = Instantiate(basicplayer, new Vector2(0, 0), Quaternion.identity) as GameObject;
                newplayer.name = "player" + Convert.ToString(i);
                //playerbullet = Instantiate(basicbullet, new Vector2(0,0), Quaternion.identity) as GameObject;
                //systemplayer.GetComponent<BoxCollider2D>().isTrigger = true;
                basicplayer.SetActive(false);
                return i;
            }
        }
        return -1;
    }

    private void CheckPlayer()
    {
        for (int i = 1; i < st.status.Length; i += 1)
        {
            if (st.status[i] == false && isplayercreate[i] == true)
            {
                isplayercreate[i] = false;
                
                string tempstr = "player" + Convert.ToString(i);
                Destroy(GameObject.Find(tempstr));
            }
        }
    }

    private void CheckText()
    {
        string str = text.text;
        if (str.Contains("player"))
        {
            int id = Convert.ToInt32(str.Substring(6));
            playerhp[id] -= 10;
            GameObject bar = GameObject.Find("HpBar");
            bar.transform.localScale = new Vector3(playerhp[id] / 100f, bar.transform.localScale.y, bar.transform.localScale.z);
            Debug.Log(id);
        }
        text.text = null;
    }

    public void CheckInput(string str)
    {
        if (str == ipaddress)
        {
            connected = true;
        }
    }

    private void OnApplicationQuit()//應用程式結束時自動關閉連線
    {
        st.StopConnect();
    }
}
