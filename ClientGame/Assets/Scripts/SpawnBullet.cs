using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBullet : MonoBehaviour
{
    public GameObject bulletPrefab;  //子彈Prefab
    public float interval;  //兩發子彈之間的時間

    void Start()
    {
        StartCoroutine(SpawnCoroutine());
    }

    IEnumerator SpawnCoroutine()
    {
        while (true)
        {
            Instantiate(bulletPrefab, new Vector2(transform.position.x - 0.55f * Mathf.Sin(transform.rotation.eulerAngles.z / 180 * Mathf.PI), transform.position.y + 0.55f * Mathf.Cos(transform.rotation.eulerAngles.z / 180 * Mathf.PI)), transform.rotation);
            
            //等候下一發子彈的時間
            yield return new WaitForSeconds(interval);
        }
    }
}
