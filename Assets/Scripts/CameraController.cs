using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    private Rigidbody2D playerRb;

    private void Awake()
    {
        CachePlayerRb();
    }

    private void LateUpdate()
    {
        if (playerRb == null)
        {
            CachePlayerRb();
            if (playerRb == null) return;
        }

        Vector2 targetPos = playerRb.position;
        transform.position = new Vector3(targetPos.x, targetPos.y, transform.position.z);
    }

    private void CachePlayerRb()
    {
        if (player == null)
        {
            var found = GameObject.FindWithTag("Player");
            if (found != null)
            {
                player = found;
            }
        }

        if (player != null)
        {
            playerRb = player.GetComponent<Rigidbody2D>();
        }
    }
}
