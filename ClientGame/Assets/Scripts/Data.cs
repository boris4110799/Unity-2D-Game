using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerData
{
    public int id { get; set; }
    public float position_x { get; set; }
    public float position_y { get; set; }
    public float z_angle { get; set; }
    public float hp { get; set; }
    public bool spawning { get; set; }

    public ServerData()
    {
        id = -1;
        position_x = 0;
        position_y = 0;
        z_angle = 0;
        hp = 100;
        spawning = false;
    }
}

public class ClientData
{
    public int id { get; set; }
    public float horizontal { get; set; }
    public float vertical { get; set; }
    public string presskey { get; set; }
    public bool spawning { get; set; }

    public ClientData()
    {
        id = -1;
        horizontal = 0;
        vertical = 0;
        presskey = "None";
        spawning = false;
    }
}
