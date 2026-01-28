using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    private TMP_Text textMesh;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private Color healColor = Color.green;
    [SerializeField] private Color damageColor = Color.red;
    
    private float timeElapsed = 0f;
    private Color startColor;

    private void Awake()
    {
        textMesh = GetComponent<TMP_Text>();
    }

    public void Setup(int amount, bool isHeal)
    {
        Setup(amount, isHeal ? healColor : damageColor);
    }

    public void Setup(int amount, Color customColor)
    {
        if (textMesh == null) textMesh = GetComponent<TMP_Text>();

        textMesh.text = (amount > 0 ? "+" : "") + amount.ToString();
        textMesh.color = customColor;
        
        startColor = textMesh.color;
    }

    private void Update()
    {
        // Move upwards
        transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);

        // Fade out
        timeElapsed += Time.deltaTime;
        if (timeElapsed >= fadeDuration)
        {
            Destroy(gameObject);
        }
        else
        {
            if (textMesh != null)
            {
                float alpha = Mathf.Lerp(1f, 0f, timeElapsed / fadeDuration);
                // Use .alpha property to fade both text and outline together
                textMesh.alpha = alpha;
            }
        }
    }
}

