using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathBox : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
            Vector2 respawnPoint = new Vector2(-77+(pc.xInputIsRight ? pc.directionOffset : 0), 30);
            collision.gameObject.transform.position = respawnPoint;
        }
    }
}
