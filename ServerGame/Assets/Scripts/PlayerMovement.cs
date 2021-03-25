using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerMovement : MonoBehaviour
{
    // Start is called before the first frame update
    //宣告一個浮點數(floating point)來設定角色的移動速度
    public float movementSpeed;
    public float minPosX;
    public float maxPosX;
    public float minPosY;
    public float maxPosY;

    void Start()
    {
        //設定初始位置和角度
        transform.position = new Vector2(2, 0);
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        //移動的距離
        float movementx = Input.GetAxis("Horizontal") * movementSpeed * Time.deltaTime;
        float movementy = Input.GetAxis("Vertical") * movementSpeed * Time.deltaTime;
        
        //更新座標
        transform.position = new Vector2(Mathf.Clamp(transform.position.x + movementx, minPosX, maxPosX), Mathf.Clamp(transform.position.y + movementy, minPosY, maxPosY));

        //旋轉圖形
        /*if (Input.GetKeyDown(KeyCode.LeftArrow)) transform.rotation = Quaternion.Euler(0, 0, 90);
        else if (Input.GetKeyDown(KeyCode.RightArrow)) transform.rotation = Quaternion.Euler(0, 0, -90);
        else if (Input.GetKeyDown(KeyCode.UpArrow)) transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (Input.GetKeyDown(KeyCode.DownArrow)) transform.rotation = Quaternion.Euler(0, 0, 180);
        else transform.Rotate(0, 0, 0);*/

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (90 - transform.rotation.eulerAngles.z <= -180) transform.Rotate(0, 0, (540 - transform.rotation.eulerAngles.z) / 5);
            else transform.Rotate(0, 0, (90 - transform.rotation.eulerAngles.z) / 5);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            if (270 - transform.rotation.eulerAngles.z <= 180) transform.Rotate(0, 0, (270 - transform.rotation.eulerAngles.z) / 5);
            else transform.Rotate(0, 0, (-90 - transform.rotation.eulerAngles.z) / 5);
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            if (360 - transform.rotation.eulerAngles.z <= 180) transform.Rotate(0, 0, (360 - transform.rotation.eulerAngles.z) / 5);
            else transform.Rotate(0, 0, (0 - transform.rotation.eulerAngles.z) / 5);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            if (180 - transform.rotation.eulerAngles.z <= 180) transform.Rotate(0, 0, (180 - transform.rotation.eulerAngles.z) / 5);
            else transform.Rotate(0, 0, (-180 - transform.rotation.eulerAngles.z) / 5);
        }
        else transform.Rotate(0, 0, 0);
    }
}
