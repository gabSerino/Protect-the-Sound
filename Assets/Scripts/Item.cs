using UnityEngine;
using System.Collections; // <--- Aggiunto per poter usare le Coroutine

public class Item : MonoBehaviour
{
    public ItemData itemData;
    private SpriteRenderer spriteRenderer;

    [Header("Despawn Settings")]
    [Tooltip("Se attivato, l'oggetto si distruggerà da solo dopo un po' di tempo")]
    public bool destroyOverTime = true;
    [Tooltip("Secondi totali prima che l'oggetto sparisca")]
    public float lifetime = 15f;
    [Tooltip("Quanti secondi prima di sparire inizia a lampeggiare?")]
    public float flickerDuration = 3f;

    void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Se l'itemData è stato assegnato a mano nell'Inspector, lo carichiamo subito
        if (itemData != null)
        {
            Initialize(itemData);
        }
    }

    void Start()
    {
        // Se l'opzione è attiva, facciamo partire il timer di autodistruzione
        if (destroyOverTime)
        {
            StartCoroutine(DespawnRoutine());
        }
    }

    // Inietta i dati quando il nemico droppa l'oggetto
    public void Initialize(ItemData newData)
    {
        itemData = newData;
        if (spriteRenderer != null && itemData != null)
        {
            spriteRenderer.sprite = itemData.icon;
        }
    }

    private IEnumerator DespawnRoutine()
    {
        // 1. Aspetta tranquillamente per gran parte della sua "vita"
        float waitTime = Mathf.Max(0, lifetime - flickerDuration);
        yield return new WaitForSeconds(waitTime);

        // 2. Inizia la fase di lampeggio per avvisare il giocatore!
        float timer = 0f;
        bool isVisible = true;

        while (timer < flickerDuration)
        {
            isVisible = !isVisible;

            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = isVisible ? 1f : 0f; // Cambia la trasparenza a 0 o 100%
                spriteRenderer.color = c;
            }

            // Diventa sempre più veloce a lampeggiare verso la fine
            float blinkSpeed = (timer > flickerDuration * 0.6f) ? 0.05f : 0.2f;

            yield return new WaitForSeconds(blinkSpeed);
            timer += blinkSpeed;
        }

        // 3. Il tempo è scaduto: l'oggetto viene rimosso definitivamente per liberare memoria
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        // Cerca il player anche se colpisce un figlio
        Player player = other.GetComponentInParent<Player>();

        if (player != null && itemData != null)
        {
            // Il player cerca di aggiungere l'oggetto.
            // Se AddItem restituisce 'true' (aveva spazio), allora l'oggetto si distrugge.
            if (player.AddItem(itemData))
            {
                Destroy(gameObject);
            }
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