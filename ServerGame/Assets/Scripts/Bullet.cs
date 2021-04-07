using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Vector2 Speed;   //設定子彈的速度
    public float second;    //過幾秒後移除Game Object

    void Start()
    {
        //speed = new Vector2(Random.Range(minSpeed.x, maxSpeed.x), Random.Range(minSpeed.y, maxSpeed.y));

        //second秒後呼叫DestroyGameObject函數
        Invoke(nameof(DestroyGameObject), second);
    }

    void Update()
    {
        transform.Translate(Speed * Time.deltaTime);
    }

    void DestroyGameObject()
    {
        Destroy(this.gameObject);
    }
}
