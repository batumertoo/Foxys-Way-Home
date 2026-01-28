using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fall : MonoBehaviour
{
    private float lastTriggerTime = -999f;
    private float triggerCooldown = 2f; // Prevent rapid re-trigger
    [SerializeField] private float fallDamage = 10f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        // Cooldown check to prevent rapid re-trigger
        if (Time.time - lastTriggerTime < triggerCooldown) return;

        lastTriggerTime = Time.time;

        // Apply a small health penalty but keep coins; do not allow death/reload here
        if (HealthSystem.Instance != null)
        {
            HealthSystem.Instance.ApplyNonLethalDamage(fallDamage, 1f);
        }

        var player = collision.GetComponent<PlayerController>();
        if (player != null)
        {
            player.RespawnToLastPlatform();
        }
    }
}
