using UnityEngine;
using UnityEngine.UI;

public class Crush : MonoBehaviour
{
    public Text text;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("stuff"))
        {
            text.text = this.gameObject.name;
            Debug.Log(text.text);
            Destroy(col.gameObject);
        }
    }
}
