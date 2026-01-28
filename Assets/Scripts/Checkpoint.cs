using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private bool activateOnce = false; // Always update checkpoint on pass-through
    [SerializeField] private AudioClip checkpointSound;
    [SerializeField] private GameObject visualFeedback; // Optional particle/animation
    
    private bool hasActivated = false;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (activateOnce && hasActivated) return;

            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                // Set checkpoint at this position (slightly above trigger)
                Vector2 checkpointPos = new Vector2(transform.position.x, transform.position.y + 0.5f);
                player.SetCheckpoint(checkpointPos);
                
                hasActivated = true;

                // Visual feedback
                if (visualFeedback != null)
                {
                    visualFeedback.SetActive(true);
                }

                // Audio feedback
                if (audioSource != null && checkpointSound != null)
                {
                    audioSource.PlayOneShot(checkpointSound);
                }

                Debug.Log("Checkpoint activated!");
            }
        }
    }

    // Call this on player death/level restart if you want to reset checkpoints
    public void ResetCheckpoint()
    {
        hasActivated = false;
        if (visualFeedback != null)
        {
            visualFeedback.SetActive(false);
        }
    }
}
