using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Data
{
    public int id;
    public float horizontal;
    public float vertical;
    public string presskey;
    public bool spawning;

    Data()
    {
        id = -1;
        horizontal = 0;
        vertical = 0;
        presskey = "None";
        spawning = false;
    }
}
