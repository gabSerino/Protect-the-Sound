using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemData itemData;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.sprite = itemData.icon;
    }

    void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            player.AddItem(itemData);
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
