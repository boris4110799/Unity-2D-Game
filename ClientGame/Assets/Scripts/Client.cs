using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using System;

public class Client : MonoBehaviour
{
    private ClientThread ct;
    
    public GameObject player;
    public GameObject bulletPrefab;  //子彈Prefab
    public float interval;  //兩發子彈之間的時間
    private bool isMove = false;
    private bool Spawning = false;
    private bool isSpawn = true;
    string[] sArray;
    string msg = null;

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
            float lerpx = Mathf.Lerp(player.transform.position.x, float.Parse(sArray[0]), 0.05f);
            float lerpy = Mathf.Lerp(player.transform.position.y, float.Parse(sArray[1]), 0.05f);
            float lerpangle = Mathf.LerpAngle(player.transform.rotation.eulerAngles.z, float.Parse(sArray[2]), 0.1f);

            player.transform.position = new Vector2(lerpx, lerpy);
            player.transform.rotation = Quaternion.Euler(0, 0, lerpangle);
            isMove = false;
        }
    }

    private void Update()
    {
        ct.Receive();

        if (ct.receiveMessage != null)
        {
            Debug.Log("Server:" + ct.receiveMessage);
            sArray = ct.receiveMessage.Split(new string[] { ". ", " " }, StringSplitOptions.RemoveEmptyEntries);

            isMove = true;
            
            ct.receiveMessage = null;
        }

        if (isSpawn)
            StartCoroutine(SpawnCoroutine());
        WriteMessage();
        
        ct.Send(msg);
    }

    private void WriteMessage()
    {
        msg = Convert.ToString(Input.GetAxis("Horizontal"));
        msg += ' ';
        msg += Convert.ToString(Input.GetAxis("Vertical"));
        msg += ' ';

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            msg += "LeftArrow";
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            msg += "RightArrow";
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            msg += "UpArrow";
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            msg += "DownArrow";
        }
        else msg += "None";
        msg += ' ';
        msg += Convert.ToString(Spawning);
        Spawning = false;
        msg += ' ';
    }

    IEnumerator SpawnCoroutine()
    {
        isSpawn = false;
        yield return new WaitForSeconds(interval);  //等候下一發子彈的時間
        Instantiate(bulletPrefab, new Vector2(player.transform.position.x - 0.55f * Mathf.Sin(player.transform.rotation.eulerAngles.z / 180 * Mathf.PI), player.transform.position.y + 0.55f * Mathf.Cos(player.transform.rotation.eulerAngles.z / 180 * Mathf.PI)), player.transform.rotation);
        Spawning = true;
        isSpawn = true;
    }

    private void OnApplicationQuit()
    {
        ct.StopConnect();
    }
}
