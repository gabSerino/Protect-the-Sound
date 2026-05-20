using UnityEngine;

public class Drug : MonoBehaviour
{
    public DrugData drugData;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.sprite = drugData.icon;
    }

    void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            player.ApplyDrug(drugData);
            Destroy(gameObject);
        }
    }

    void OnBecameVisible()
    {
        spriteRenderer.enabled = true;
    }

    void OnBecameInvisible()
    {
        spriteRenderer.enabled = false;
    }
}
