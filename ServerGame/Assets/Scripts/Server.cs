using UnityEngine;
using System;
using System.Net.Sockets;
using System.Collections;

public class Server : MonoBehaviour
{
    private ServerThread st;
    public Data data;
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
            Debug.Log("Client:" + st.receiveMessage);
            string[] sArray = st.receiveMessage.Split(new string[] { "{", "}" }, StringSplitOptions.RemoveEmptyEntries);
            string datastr = "{" + sArray[0] + "}";
            data = JsonUtility.FromJson<Data>(datastr);

            //移動的距離
            movementx = data.horizontal * movementSpeed * Time.deltaTime;
            movementy = data.vertical * movementSpeed * Time.deltaTime;
            isMove = true;

            float theta = player.transform.rotation.eulerAngles.z;
            if (data.presskey == "LeftArrow")
            {
                if (theta > 180) theta -= 360;
                if (theta + 90 >= 0) player.transform.Rotate(0, 0, (90 - theta) / 5);
                else player.transform.Rotate(0, 0, (-270 - theta) / 5);
            }
            else if (data.presskey == "RightArrow")
            {
                if (270 - player.transform.rotation.eulerAngles.z <= 180) player.transform.Rotate(0, 0, (270 - player.transform.rotation.eulerAngles.z) / 5);
                else player.transform.Rotate(0, 0, (-90 - player.transform.rotation.eulerAngles.z) / 5);
            }
            else if (data.presskey == "UpArrow")
            {
                if (360 - player.transform.rotation.eulerAngles.z <= 180) player.transform.Rotate(0, 0, (360 - player.transform.rotation.eulerAngles.z) / 5);
                else player.transform.Rotate(0, 0, (0 - player.transform.rotation.eulerAngles.z) / 5);
            }
            else if (data.presskey == "DownArrow")
            {
                if (180 - player.transform.rotation.eulerAngles.z <= 180) player.transform.Rotate(0, 0, (180 - player.transform.rotation.eulerAngles.z) / 5);
                else player.transform.Rotate(0, 0, (-180 - player.transform.rotation.eulerAngles.z) / 5);
            }
            else player.transform.Rotate(0, 0, 0);

            if (data.spawning)
            {
                Instantiate(bulletPrefab, new Vector2(player.transform.position.x - 0.55f * Mathf.Sin(player.transform.rotation.eulerAngles.z / 180 * Mathf.PI), player.transform.position.y + 0.55f * Mathf.Cos(player.transform.rotation.eulerAngles.z / 180 * Mathf.PI)), player.transform.rotation);
            }

            st.receiveMessage = null;
        }

        string[] str = new string[20];
        str[0] = Convert.ToString(player.transform.position.x);
        str[1] = Convert.ToString(player.transform.position.y);
        str[2] = Convert.ToString(player.transform.rotation.eulerAngles.z);
        /*int counter = 3;
        GameObject[] bulletArray = GameObject.FindGameObjectsWithTag("stuff");
        for (int i = 0; i < bulletArray.Length; i += 1)
        {
            str[counter++] = Convert.ToString(bulletArray[i].transform.position.x);
            str[counter++] = Convert.ToString(bulletArray[i].transform.position.y);
            str[counter++] = Convert.ToString(bulletArray[i].transform.rotation.eulerAngles.z);
        }*/
        string allstr = "";
        for (int i = 0; i < 20; i += 1)
        {
            allstr += str[i];
            allstr += " ";
        }

        //Debug.Log(allstr);
        st.Send(allstr);

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