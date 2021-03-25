using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector2 minSpeed; //最低速度
    public Vector2 maxSpeed; //最高速度
    private Vector2 speed;   //以隨機選定的速度
    public float second; //過幾秒後移除Game Object

    void Start()
    {
        speed = new Vector2(Random.Range(minSpeed.x, maxSpeed.x), Random.Range(minSpeed.y, maxSpeed.y));

        //second秒後呼叫DestroyGameObject函數
        Invoke(nameof(DestroyGameObject), second);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(speed * Time.deltaTime);
    }

    void DestroyGameObject()
    {
        Destroy(this.gameObject);
    }
}
