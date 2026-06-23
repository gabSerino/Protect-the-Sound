using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemData itemData;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Se l'itemData è stato assegnato a mano nell'Inspector, lo carichiamo subito
        if (itemData != null)
        {
            Initialize(itemData);
        }
    }

    // NUOVO METODO: Inietta i dati quando il nemico droppa l'oggetto
    public void Initialize(ItemData newData)
    {
        itemData = newData;
        if (spriteRenderer != null && itemData != null)
        {
            spriteRenderer.sprite = itemData.icon;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Cerca il player anche se colpisce un figlio (come abbiamo fatto per i danni)
        Player player = other.GetComponentInParent<Player>();

        if (player != null && itemData != null)
        {
            player.AddItem(itemData);
            Destroy(gameObject);
        }
    }

    void OnBecameVisible()
    {
        if (spriteRenderer != null) spriteRenderer.enabled = true;
    }

    void OnBecameInvisible()
    {
        if (spriteRenderer != null) spriteRenderer.enabled = false;
    }
}