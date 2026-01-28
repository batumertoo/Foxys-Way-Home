//==============================================================
// HealthSystem
// HealthSystem.Instance.TakeDamage (float Damage);
// HealthSystem.Instance.HealDamage (float Heal);
// Attach to the Hero.
//==============================================================

using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
	public static HealthSystem Instance;

	public Image currentHealthBar;
	public Image currentHealthGlobe;
	public Text healthText;
	public float hitPoint = 100f;
	public float maxHitPoint = 100f;
    public GameObject floatingTextPrefab;

	//==============================================================
	// Regenerate Health
	//==============================================================
	public bool Regenerate = false; // Disabled by default - set to true if you want health regeneration
	public float regen = 0.1f;
	private float timeleft = 0.0f;	// Left time for current interval
	public float regenUpdateInterval = 1f;

	public bool GodMode;

	//==============================================================
	// Awake
	//==============================================================
	void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}
	
	//==============================================================
	// Awake
	//==============================================================
  	void Start()
	{
		UpdateGraphics();
		timeleft = regenUpdateInterval; 
	}

	//==============================================================
	// Update
	//==============================================================
	void Update ()
	{
		if (Regenerate)
			Regen();
	}

	//==============================================================
	// Regenerate Health
	//==============================================================
	private void Regen()
	{
		timeleft -= Time.deltaTime;

		if (timeleft <= 0.0) // Interval ended - update health and start new interval
		{
			// Debug mode
			if (GodMode)
			{
				HealDamage(maxHitPoint);
			}
			else
			{
				HealDamage(regen);
			}

			UpdateGraphics();

			timeleft = regenUpdateInterval;
		}
	}

	//==============================================================
	// Health Logic
	//==============================================================
	private void UpdateHealthBar()
	{
		if (currentHealthBar != null)
		{
			float ratio = hitPoint / maxHitPoint;
			
			// Try fillAmount first (if Image Type is Filled)
			if (currentHealthBar.type == Image.Type.Filled)
			{
				currentHealthBar.fillAmount = ratio;
			}
			else
			{
				// Fallback to localPosition method (for Simple Image Type)
				currentHealthBar.rectTransform.localPosition = new Vector3(
					currentHealthBar.rectTransform.rect.width * ratio - currentHealthBar.rectTransform.rect.width, 
					0, 
					0
				);
			}
		}
		if (healthText != null)
		{
			healthText.text = hitPoint.ToString ("0");
		}
	}

	private void UpdateHealthGlobe()
	{
		if (currentHealthGlobe != null)
		{
			float ratio = hitPoint / maxHitPoint;
			currentHealthGlobe.rectTransform.localPosition = new Vector3(0, currentHealthGlobe.rectTransform.rect.height * ratio - currentHealthGlobe.rectTransform.rect.height, 0);
		}
		if (healthText != null)
		{
			healthText.text = hitPoint.ToString("0");
		}
	}

	public void TakeDamage(float Damage)
	{
		hitPoint -= Damage;
		if (hitPoint < 1)
			hitPoint = 0;

		UpdateGraphics();

        ShowFloatingText((int)Damage, false);

		StartCoroutine(PlayerHurts());
	}

	public void HealDamage(float Heal)
	{
		hitPoint += Heal;
		if (hitPoint > maxHitPoint) 
			hitPoint = maxHitPoint;

		UpdateGraphics();

        if ((int)Heal > 0)
            ShowFloatingText((int)Heal, true);
	}

	// Apply damage that cannot kill the player (used for falls that should not reload the scene)
	public void ApplyNonLethalDamage(float damage, float minHp = 1f)
	{
		if (damage <= 0f) return;

		hitPoint -= damage;
		if (hitPoint < minHp) hitPoint = minHp;

		UpdateGraphics();
		ShowFloatingText((int)damage, false);
	}
	public void SetMaxHealth(float max)
	{
		maxHitPoint += (int)(maxHitPoint * max / 100);

		UpdateGraphics();
	}

	//==============================================================
	// Update all Bars & Globes UI graphics
	//==============================================================
	private void UpdateGraphics()
	{
		UpdateHealthBar();
		UpdateHealthGlobe();
	}

	//==============================================================
	// Coroutine Player Hurts
	//==============================================================
	IEnumerator PlayerHurts()
	{
		// Player gets hurt. Do stuff.. play anim, sound..

		if (hitPoint < 1) // Health is Zero!!
		{
			yield return StartCoroutine(PlayerDied()); // Hero is Dead
		}

		else
			yield return null;
	}

	//==============================================================
	// Hero is dead
	//==============================================================
	IEnumerator PlayerDied()
	{
		// Player is dead. Do stuff.. play anim, sound..

		yield return new WaitForSeconds(1f); // Wait a bit before reloading

		// Reset health and reload scene; restore coins to level-start baseline
		ResetHealth();
		if (PermanentUI.perm != null)
		{
			PermanentUI.perm.RestoreCoinsToLevelStart();
		}
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

    //==============================================================
    // Show Floating Text
    //==============================================================
    public void ShowFloatingText(int amount, bool isHeal)
    {
        if (floatingTextPrefab)
        {
            // Default to current position, but try to find the Player
            Vector3 spawnPosition = transform.position;
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                spawnPosition = player.transform.position;
            }

            Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0.5f, 1f), 0);
            GameObject go = Instantiate(floatingTextPrefab, spawnPosition + offset, Quaternion.identity);
            go.GetComponent<FloatingText>().Setup(amount, isHeal);
        }
    }

	//==============================================================
	// Reset Health
	//==============================================================
	public void ResetHealth()
	{
		hitPoint = maxHitPoint;
		UpdateGraphics();
	}
}

