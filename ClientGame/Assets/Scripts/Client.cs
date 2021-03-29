using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System;
using System.Text.Json;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Cinemachine;

public class Client : MonoBehaviour
{
    private ClientThread ct;
    public ServerData[] Sdata = new ServerData[10];
    public ClientData Cdata = new ClientData();

    private GameObject myplayer;
    public GameObject basicplayer;
    public GameObject bulletPrefab;  //子彈Prefab
    public Text text;

    private GameObject player;
    private bool[] isplayercreate = new bool[10];
    private bool connected = false;

    private int myid = -1;
    public float interval;  //兩發子彈之間的時間
    private bool[] isMove = new bool[10];
    private bool Spawning = false;
    private bool isSpawn = true;

    private void Start()
    {
        basicplayer.SetActive(true);
        myplayer = Instantiate(basicplayer, new Vector2(2, 0), Quaternion.identity) as GameObject;
        myplayer.name = "player" + Convert.ToString(0);
        GameObject.Find("CM vcam1").GetComponent<CinemachineVirtualCamera>().Follow = myplayer.transform;
        //playerbullet = Instantiate(basicbullet, new Vector2(0,0), Quaternion.identity) as GameObject;
        //systemplayer.GetComponent<BoxCollider2D>().isTrigger = true;
        basicplayer.SetActive(false);

    }

    private void FixedUpdate()
    {
        if (connected)
        {
            for (int i = 0; i < 10; i += 1)
            {
                if (Sdata[i] != null && isplayercreate[i] && isMove[i])
                {
                    string playername = "player" + Convert.ToString(i);
                    player = GameObject.Find(playername);

                    float lerpx = Mathf.Lerp(player.transform.position.x, Sdata[i].position_x, 0.2f);
                    float lerpy = Mathf.Lerp(player.transform.position.y, Sdata[i].position_y, 0.2f);
                    float lerpangle = Mathf.LerpAngle(player.transform.rotation.eulerAngles.z, Sdata[i].z_angle, 0.5f);

                    player.transform.position = new Vector2(lerpx, lerpy);
                    player.transform.rotation = Quaternion.Euler(0, 0, lerpangle);

                    isMove[i] = false;
                }
            }
        }
    }

    private void Update()
    {
        if (connected)
        {
            if (ct.receiveMessage != null)
            {
                //Debug.Log("Server:" + ct.receiveMessage);
                string[] sArray = ct.receiveMessage.Split(new string[] { "{", "}" }, StringSplitOptions.RemoveEmptyEntries);
                myid = Convert.ToInt32(sArray[0]);

                if (myid != -1)
                {
                    myplayer.name = "player" + Convert.ToString(myid);
                }

                for (int i = 1; i < sArray.Length; i += 1)
                {
                    if (sArray[i].Contains("\"id\":") == false) continue;

                    string datastr = "{" + sArray[i] + "}";
                    Debug.Log(datastr);
                    ServerData tempdata = JsonSerializer.Deserialize<ServerData>(datastr);

                    if (Sdata[tempdata.id] == null && isplayercreate[tempdata.id] == false)
                    {
                        Sdata[tempdata.id] = tempdata;
                        isplayercreate[tempdata.id] = true;

                        basicplayer.SetActive(true);
                        player = Instantiate(basicplayer, new Vector2(Sdata[tempdata.id].position_x, Sdata[tempdata.id].position_y), Quaternion.Euler(0, 0, Sdata[tempdata.id].z_angle)) as GameObject;
                        player.name = "player" + Convert.ToString(tempdata.id);
                        //playerbullet = Instantiate(basicbullet, new Vector2(0,0), Quaternion.identity) as GameObject;
                        //systemplayer.GetComponent<BoxCollider2D>().isTrigger = true;
                        basicplayer.SetActive(false);
                    }

                    isMove[tempdata.id] = true;

                    if (tempdata.spawning && tempdata.id != myid)
                    {
                        string playername = "player" + Convert.ToString(tempdata.id);
                        player = GameObject.Find(playername);
                        Instantiate(bulletPrefab, new Vector2(player.transform.position.x - Mathf.Sin(player.transform.rotation.eulerAngles.z / 180 * Mathf.PI), player.transform.position.y + Mathf.Cos(player.transform.rotation.eulerAngles.z / 180 * Mathf.PI)), player.transform.rotation);
                    }
                }

                ct.receiveMessage = null;
            }

            if (isSpawn)
                StartCoroutine(SpawnCoroutine());

            WriteMessage();
            var json = JsonSerializer.Serialize(Cdata);
            //Debug.Log(json);
            ct.Send(json);

            //CreatePlayer();
            CheckPlayer();
        }
    }

    private void WriteMessage()
    {
        Keyboard keyboard = Keyboard.current;
        Cdata.id = myid;
        Cdata.horizontal = keyboard.rightArrowKey.ReadValue() - keyboard.leftArrowKey.ReadValue();
        Cdata.vertical = keyboard.upArrowKey.ReadValue() - keyboard.downArrowKey.ReadValue();
        
        if (keyboard.leftArrowKey.isPressed)
        {
            Cdata.presskey = "LeftArrow";
        }
        else if (keyboard.rightArrowKey.isPressed)
        {
            Cdata.presskey = "RightArrow";
        }
        else if (keyboard.upArrowKey.isPressed)
        {
            Cdata.presskey = "UpArrow";
        }
        else if (keyboard.downArrowKey.isPressed)
        {
            Cdata.presskey = "DownArrow";
        }
        else Cdata.presskey = "None";

        Cdata.spawning = Spawning;
        Spawning = false;
    }

    IEnumerator SpawnCoroutine()
    {
        isSpawn = false;
        yield return new WaitForSeconds(interval);  //等候下一發子彈的時間
        Instantiate(bulletPrefab, new Vector2(myplayer.transform.position.x - Mathf.Sin(myplayer.transform.rotation.eulerAngles.z / 180 * Mathf.PI), myplayer.transform.position.y + Mathf.Cos(myplayer.transform.rotation.eulerAngles.z / 180 * Mathf.PI)), myplayer.transform.rotation);
        Spawning = true;
        isSpawn = true;
    }

    private void CreatePlayer()
    {
        for (int i = 1; i < 10; i += 1)
        {
            if (Sdata[i] == null && isplayercreate[i] == true)
            {
                basicplayer.SetActive(true);
                player = Instantiate(basicplayer, new Vector2(Sdata[i].position_x, Sdata[i].position_y), Quaternion.Euler(0,0,Sdata[i].z_angle)) as GameObject;
                player.name = "player" + Convert.ToString(i);
                //playerbullet = Instantiate(basicbullet, new Vector2(0,0), Quaternion.identity) as GameObject;
                //systemplayer.GetComponent<BoxCollider2D>().isTrigger = true;
                basicplayer.SetActive(false);
            }
        }
    }

    private void CheckPlayer()
    {
        for (int i = 1; i < 10; i += 1)
        {
            if (Sdata[i] != null && isplayercreate[i] == false)
            {
                string tempstr = "player" + Convert.ToString(i);
                Sdata[i] = null;
                Destroy(GameObject.Find(tempstr));
            }
        }
    }

    public void CheckInput(string str)
    {
        ct = new ClientThread(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, str, 8000);
        
        for (int i=0;i<100000;i+=1)
        {
            if (ct.Connected())
            {
                GameObject.Find("Canvas").transform.GetChild(0).gameObject.SetActive(false);
                connected = true;
                break;
            }
        }
    }

    private void OnApplicationQuit()
    {
        ct.StopConnect();
    }
}
