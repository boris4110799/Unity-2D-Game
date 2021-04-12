using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Net.Sockets;
using System.Text.Json;
using Cinemachine;

public class Client : MonoBehaviour
{
    private ClientThread ct;
    public ServerData[] Sdata = new ServerData[10];
    public ClientData Cdata = new ClientData();

    private GameObject myplayer;
    public GameObject basicplayer;
    public GameObject bulletPrefab;  //子彈Prefab
    public GameObject hpbarPrefab;
    private GameObject mybar;
    public Text pname;

    private GameObject player;
    private GameObject playerf;
    private GameObject hpbar;
    private bool[] isplayercreate = new bool[10];
    private bool[] isplayeralive = new bool[10];
    private bool connected = false;

    private int myid = -1;
    public float interval;  //兩發子彈之間的時間
    private bool[] isMove = new bool[10];
    private bool Spawning = false;
    private bool isSpawn = true;
    private bool idrequest = false;

    private void Start()
    {
        basicplayer.SetActive(true);
        myplayer = Instantiate(basicplayer, new Vector2(2, 0), Quaternion.identity) as GameObject;
        myplayer.name = "player" + Convert.ToString(0);
        GameObject.Find("CM vcam1").GetComponent<CinemachineVirtualCamera>().Follow = myplayer.transform;
        basicplayer.SetActive(false);

        mybar = Instantiate(hpbarPrefab, new Vector2(myplayer.transform.position.x - 2, myplayer.transform.position.y - 1.5f), Quaternion.identity) as GameObject;
        mybar.name = "hpbar" + Convert.ToString(0);
    }

    private void FixedUpdate()
    {
        if (connected)
        {
            for (int i = 1; i < 10; i += 1)
            {
                if (isMove[i])
                {
                    string playername = "player" + Convert.ToString(i);
                    playerf = GameObject.Find(playername);
                    string hpname = "hpbar" + Convert.ToString(Sdata[i].id);
                    hpbar = GameObject.Find(hpname);

                    float lerpx = Mathf.Lerp(playerf.transform.position.x, Sdata[i].position_x, 0.2f);
                    float lerpy = Mathf.Lerp(playerf.transform.position.y, Sdata[i].position_y, 0.2f);
                    float lerpangle = Mathf.LerpAngle(playerf.transform.rotation.eulerAngles.z, Sdata[i].z_angle, 0.5f);

                    playerf.transform.position = new Vector2(lerpx, lerpy);
                    playerf.transform.rotation = Quaternion.Euler(0, 0, lerpangle);
                    hpbar.transform.position = new Vector2(playerf.transform.position.x - 2, playerf.transform.position.y - 1.5f);
                    
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
                isplayercreate[myid] = true;

                if (myid != -1)
                {
                    myplayer.name = "player" + Convert.ToString(myid);
                    mybar.name = "hpbar" + Convert.ToString(myid);
                    pname.text = myplayer.name;
                }

                for (int i = 1; i < sArray.Length; i += 1)
                {
                    if (sArray[i].Contains("\"id\":") == false) continue;

                    string datastr = "{" + sArray[i] + "}";
                    Debug.Log(datastr);
                    ServerData tempdata = JsonSerializer.Deserialize<ServerData>(datastr);
                    Debug.Log("temp_id:" + Convert.ToString(tempdata.id));
                    Sdata[tempdata.id] = tempdata;

                    if (isplayercreate[tempdata.id] == false)
                    {
                        isplayercreate[tempdata.id] = true;

                        basicplayer.SetActive(true);
                        player = Instantiate(basicplayer, new Vector2(Sdata[tempdata.id].position_x, Sdata[tempdata.id].position_y), Quaternion.Euler(0, 0, Sdata[tempdata.id].z_angle)) as GameObject;
                        player.name = "player" + Convert.ToString(tempdata.id);
                        basicplayer.SetActive(false);

                        hpbar = Instantiate(hpbarPrefab, new Vector2(player.transform.position.x - 2, player.transform.position.y - 1.5f), Quaternion.identity) as GameObject;
                        hpbar.name = "hpbar" + Convert.ToString(i);
                    }

                    isplayeralive[tempdata.id] = true;
                    isMove[tempdata.id] = true;

                    if (tempdata.spawning && tempdata.id != myid)
                    {
                        string playername = "player" + Convert.ToString(tempdata.id);
                        player = GameObject.Find(playername);
                        Instantiate(bulletPrefab, new Vector2(player.transform.position.x - Mathf.Sin(player.transform.rotation.eulerAngles.z / 180 * Mathf.PI), player.transform.position.y + Mathf.Cos(player.transform.rotation.eulerAngles.z / 180 * Mathf.PI)), player.transform.rotation);
                    }

                    string hpname = "hpbar" + Convert.ToString(tempdata.id);
                    GameObject bar = GameObject.Find(hpname).transform.GetChild(0).gameObject;
                    bar.transform.localScale = new Vector3(Sdata[tempdata.id].hp / 100f, bar.transform.localScale.y, bar.transform.localScale.z);
                }

                CheckPlayer();
                ct.receiveMessage = null;
            }
            else
            {
                Debug.Log("null message");
            }

            if (isSpawn)
                StartCoroutine(SpawnCoroutine());

            if (idrequest == false || myid != -1)
            {
                idrequest = true;
                WriteMessage();
                var json = JsonSerializer.Serialize(Cdata);
                //Debug.Log(json);
                ct.Send(json);
            }

            //CreatePlayer();
            //CheckPlayer();
        }
    }

    private void WriteMessage()
    {
        Keyboard keyboard = Keyboard.current;
        Cdata.id = myid;
        
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
                basicplayer.SetActive(false);
            }
        }
    }

    private void CheckPlayer()
    {
        for (int i = 1; i < 10; i += 1)
        {
            if (Sdata[i] != null && isplayeralive[i] == false)
            {
                Sdata[i] = null;
                isplayercreate[i] = false;

                string tempstr = "player" + Convert.ToString(i);
                Destroy(GameObject.Find(tempstr));
                tempstr = "hpbar" + Convert.ToString(i);
                Destroy(GameObject.Find(tempstr));
            }

            isplayeralive[i] = false;
        }
    }

    public void CheckInput(string str)
    {
        ct = new ClientThread(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, str, 8888);
        
        for (int i=0;i<10000000; i+=1)
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
