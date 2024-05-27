using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy")) 
        {
            if (collision.transform.parent != null)
                collision.transform.parent.gameObject.SetActive(false);
            else
                collision.gameObject.SetActive(false);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (collision.transform.parent != null)
                collision.transform.parent.gameObject.SetActive(false);
            else
                collision.gameObject.SetActive(false);
        }
    }
}
