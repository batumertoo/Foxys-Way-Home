using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eagle : Enemy
{
    [SerializeField] private Transform firePoint;
    private Transform player;
    [SerializeField] private GameObject bulletPrefab;
    private float shootRange = 12f; //should match radius of trigger on shot script
    [SerializeField] private float shootCD = 3f;

    protected override void Start() {
        base.Start();
        var playerObj = GameObject.FindWithTag("Player");
        player = playerObj != null ? playerObj.transform : null;
        if (player == null)
        {
            Debug.LogWarning("Eagle could not find Player by tag; disabling shooting until player exists.");
            return;
        }
        InvokeRepeating(nameof(Shoot), 0f, shootCD);
    }

    private void Shoot() {
        if (player == null || firePoint == null) return;
        if(Vector2.Distance(player.position, firePoint.position) < shootRange) {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
    }

    
}
