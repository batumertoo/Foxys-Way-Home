using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bat : Enemy
{
    [SerializeField] private float flySpeed = 2f;
    [SerializeField] private float flightDistance = 3f; 
    [SerializeField] private int coinReward = 5; // Oldurunce verilecek coin
    [SerializeField] private GameObject floatingTextPrefab; // Prefab reference
    
    private Vector3 startPosition;
    private bool movingRight = true;
    private Vector3 startScale;

    protected override void Start()
    {
        base.Start();
        startPosition = transform.position;
        startScale = transform.localScale;
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        if (movingRight)
        {
            if (transform.position.x < startPosition.x + flightDistance)
            {
                rb.velocity = new Vector2(flySpeed, 0);
                if (transform.localScale.x > 0) 
                    transform.localScale = new Vector3(-Mathf.Abs(startScale.x), startScale.y, startScale.z);
            }
            else
            {
                movingRight = false;
            }
        }
        else
        {
            if (transform.position.x > startPosition.x - flightDistance)
            {
                rb.velocity = new Vector2(-flySpeed, 0);
                if (transform.localScale.x < 0)
                    transform.localScale = new Vector3(Mathf.Abs(startScale.x), startScale.y, startScale.z);
            }
            else
            {
                movingRight = true;
            }
        }
    }

    // PlayerController'dan cagrilacak ozel olum fonksiyonu
    public void Die()
    {
        anim.SetTrigger("Death");
        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic; // Oldukten sonra dusmesin, asili kalsin
        GetComponent<Collider2D>().enabled = false; // Artik carpisilamasin
        
        // Coin ver
        if (PermanentUI.perm != null)
        {
            PermanentUI.perm.coins += coinReward;
            if (PermanentUI.perm.coinText != null)
            {
                PermanentUI.perm.coinText.text = PermanentUI.perm.coins.ToString();
            }
        }
        
        // Sesi cal
        if (explosion != null) explosion.Play();
        
        // Floating Text goster
        if (floatingTextPrefab != null)
        {
            GameObject go = Instantiate(floatingTextPrefab, transform.position, Quaternion.identity);
            FloatingText ft = go.GetComponent<FloatingText>();
            if (ft != null)
            {
                ft.Setup(coinReward, Color.yellow); // Ozel sari renk
            }
        }
        
        // Yok et (Animasyon suresi kadar bekle yoksa hemen yok olur)
        Destroy(gameObject, 1f); 
    }
}
