using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System;
using System.Text.Json;

public class Client : MonoBehaviour
{
    private ClientThread ct;
    public ServerData Sdata = new ServerData();
    public ClientData Cdata = new ClientData();

    public GameObject player;
    public GameObject bulletPrefab;  //子彈Prefab
    public float interval;  //兩發子彈之間的時間
    private bool isMove = false;
    private bool Spawning = false;
    private bool isSpawn = true;

    private void Start()
    {
        ct = new ClientThread(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, "127.0.0.1", 8000);
        ct.StartConnect();

        player.transform.position = new Vector2(2, 0);
        player.transform.rotation = Quaternion.Euler(0, 0, 0);
        
    }

    private void FixedUpdate()
    {
        if (isMove)
        {
            float lerpx = Mathf.Lerp(player.transform.position.x, Sdata.position_x, 0.2f);
            float lerpy = Mathf.Lerp(player.transform.position.y, Sdata.position_y, 0.2f);
            float lerpangle = Mathf.LerpAngle(player.transform.rotation.eulerAngles.z, Sdata.z_angle, 0.5f);

            player.transform.position = new Vector2(lerpx, lerpy);
            player.transform.rotation = Quaternion.Euler(0, 0, lerpangle);
            //player.transform.position = new Vector2(Sdata.position_x, Sdata.position_y);
            //player.transform.rotation = Quaternion.Euler(0, 0, Sdata.z_angle);
            isMove = false;
        }
    }

    private void Update()
    {
        ct.Receive();

        if (ct.receiveMessage != null)
        {
            //Debug.Log("Server:" + ct.receiveMessage);
            string[] sArray = ct.receiveMessage.Split(new string[] { "{", "}" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string tempstr in sArray)
            {
                if (tempstr[0] != '\"') continue;
                
                string datastr = "{" + tempstr + "}";
                Debug.Log(datastr);
                Sdata = JsonSerializer.Deserialize<ServerData>(datastr);
                isMove = true;
            }

            ct.receiveMessage = null;
        }

        if (isSpawn)
            StartCoroutine(SpawnCoroutine());
        
        WriteMessage();
        var json = JsonSerializer.Serialize<ClientData>(Cdata);
        //Debug.Log(json);
        ct.Send(json);
    }

    private void WriteMessage()
    {
        Cdata.id = 0;
        Cdata.horizontal = Input.GetAxis("Horizontal");
        Cdata.vertical = Input.GetAxis("Vertical");
        
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            Cdata.presskey = "LeftArrow";
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            Cdata.presskey = "RightArrow";
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            Cdata.presskey = "UpArrow";
        }
        else if (Input.GetKey(KeyCode.DownArrow))
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
        Instantiate(bulletPrefab, new Vector2(player.transform.position.x - 0.55f * Mathf.Sin(player.transform.rotation.eulerAngles.z / 180 * Mathf.PI), player.transform.position.y + 0.55f * Mathf.Cos(player.transform.rotation.eulerAngles.z / 180 * Mathf.PI)), player.transform.rotation);
        Spawning = true;
        isSpawn = true;
    }

    /*private void Serialize()
    {
        var json = JsonUtility.ToJson(data);
    }*/

    private void OnApplicationQuit()
    {
        ct.StopConnect();
    }
}
