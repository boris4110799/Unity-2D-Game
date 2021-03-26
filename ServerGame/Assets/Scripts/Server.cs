using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text.Json;

public class Server : MonoBehaviour
{
    private ServerThread st;
    public ServerData Sdata = new ServerData();
    public ClientData Cdata = new ClientData();
    //private bool isSend;//儲存是否發送訊息完畢

    public GameObject basicplayer;
    public GameObject bulletPrefab;
    private GameObject player;
    private GameObject playerbullet;

    public float movementSpeed;
    float movementx = 0;
    float movementy = 0;
    bool isMove = false;
    public float minPosX;
    public float maxPosX;
    public float minPosY;
    public float maxPosY;

    private void Start()
    {
        //開始連線，設定使用網路、串流、TCP
        st = new ServerThread(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, "127.0.0.1", 8000);
        st.Listen();//讓Server socket開始監聽連線
        st.StartConnect();//開啟Server socket
        //isSend = true;
        
        player = Instantiate(basicplayer, new Vector2(2, 0), Quaternion.identity) as GameObject;
        //playerbullet = Instantiate(basicbullet, new Vector2(0,0), Quaternion.identity) as GameObject;
        //systemplayer.GetComponent<BoxCollider2D>().isTrigger = true;
        basicplayer.SetActive(false);
        //playerbullet.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (isMove)
        {
            //移動的距離
            movementx = Cdata.horizontal * movementSpeed * Time.deltaTime;
            movementy = Cdata.vertical * movementSpeed * Time.deltaTime;

            //更新座標
            player.transform.position = new Vector2(Mathf.Clamp(player.transform.position.x + movementx, minPosX, maxPosX), Mathf.Clamp(player.transform.position.y + movementy, minPosY, maxPosY));
            isMove = false;
        }
    }

    private void Update()
    {
        st.Receive();

        if (st.receiveMessage != null)
        {
            //Debug.Log("Client:" + st.receiveMessage);
            string[] sArray = st.receiveMessage.Split(new string[] { "{", "}" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string tempstr in sArray)
            {
                if (tempstr[0] != '\"') continue;
                
                string datastr = "{" + tempstr + "}";
                Debug.Log(datastr);
                Cdata = JsonSerializer.Deserialize<ClientData>(datastr);

                isMove = true;

                float z_angle = player.transform.rotation.eulerAngles.z;
                if (Cdata.presskey == "LeftArrow")
                {
                    if (z_angle > 180) z_angle -= 360;
                    if (z_angle + 90 >= 0) player.transform.Rotate(0, 0, (90 - z_angle) / 5);
                    else player.transform.Rotate(0, 0, (-270 - z_angle) / 5);
                }
                else if (Cdata.presskey == "RightArrow")
                {
                    if (270 - z_angle <= 180) player.transform.Rotate(0, 0, (270 - z_angle) / 5);
                    else player.transform.Rotate(0, 0, (-90 - z_angle) / 5);
                }
                else if (Cdata.presskey == "UpArrow")
                {
                    if (360 - z_angle <= 180) player.transform.Rotate(0, 0, (360 - z_angle) / 5);
                    else player.transform.Rotate(0, 0, (0 - z_angle) / 5);
                }
                else if (Cdata.presskey == "DownArrow")
                {
                    if (180 - z_angle <= 180) player.transform.Rotate(0, 0, (180 - z_angle) / 5);
                    else player.transform.Rotate(0, 0, (-180 - z_angle) / 5);
                }
                else player.transform.Rotate(0, 0, 0);

                if (Cdata.spawning)
                {
                    Instantiate(bulletPrefab, new Vector2(player.transform.position.x - 0.55f * Mathf.Sin(z_angle / 180 * Mathf.PI), player.transform.position.y + 0.55f * Mathf.Cos(z_angle / 180 * Mathf.PI)), player.transform.rotation);
                }
            }

            st.receiveMessage = null;
        }

        WriteMessage();
        var json = JsonSerializer.Serialize<ServerData>(Sdata);
        //Debug.Log(json);
        st.Send(json);
    }

    private void WriteMessage()
    {
        Sdata.id = 0;
        Sdata.position_x = player.transform.position.x;
        Sdata.position_y = player.transform.position.y;
        Sdata.z_angle = player.transform.rotation.eulerAngles.z;
    }

    private void OnApplicationQuit()//應用程式結束時自動關閉連線
    {
        st.StopConnect();
    }
}
/*string[] sArray = st.receiveMessage.Split(new string[] { ". ", " " }, StringSplitOptions.RemoveEmptyEntries);

            //移動的距離
            movementx = float.Parse(sArray[0]) * movementSpeed * Time.deltaTime * 1.5f;
            movementy = float.Parse(sArray[1]) * movementSpeed * Time.deltaTime * 1.5f;
            isMove = true;

            if (sArray[2] == "LeftArrow")
            {
                if (90 - transform.rotation.eulerAngles.z <= -180) player.transform.Rotate(0, 0, (540 - player.transform.rotation.eulerAngles.z) / 5);
                else player.transform.Rotate(0, 0, (90 - player.transform.rotation.eulerAngles.z) / 5);
            }
            else if (sArray[2] == "RightArrow")
            {
                if (270 - player.transform.rotation.eulerAngles.z <= 180) player.transform.Rotate(0, 0, (270 - player.transform.rotation.eulerAngles.z) / 5);
                else player.transform.Rotate(0, 0, (-90 - player.transform.rotation.eulerAngles.z) / 5);
            }
            else if (sArray[2] == "UpArrow")
            {
                if (360 - player.transform.rotation.eulerAngles.z <= 180) player.transform.Rotate(0, 0, (360 - player.transform.rotation.eulerAngles.z) / 5);
                else player.transform.Rotate(0, 0, (0 - player.transform.rotation.eulerAngles.z) / 5);
            }
            else if (sArray[2] == "DownArrow")
            {
                if (180 - player.transform.rotation.eulerAngles.z <= 180) player.transform.Rotate(0, 0, (180 - player.transform.rotation.eulerAngles.z) / 5);
                else player.transform.Rotate(0, 0, (-180 - player.transform.rotation.eulerAngles.z) / 5);
            }
            else player.transform.Rotate(0, 0, 0);

            isSpawn = bool.Parse(sArray[3]);*/