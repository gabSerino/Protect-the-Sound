using UnityEngine;
using FMODUnity;

[RequireComponent(typeof(SharedHealth))]
public class DamageReceiver : MonoBehaviour
{
    // Ora ogni cassa ha la propria vita. Se lasci il campo vuoto in Inspector,
    // verr� usato automaticamente il componente SharedHealth presente su questo stesso oggetto.
    public SharedHealth sharedHealthManager;

    [SerializeField] private FMODUnity.EventReference hitSoundEvent = new FMODUnity.EventReference();

    void Awake()
    {
        if (sharedHealthManager == null)
        {
            sharedHealthManager = GetComponent<SharedHealth>();
        }
    }

    public void TakeDamage(float amount)
    {
        if (sharedHealthManager != null)
        {
            RuntimeManager.PlayOneShot(hitSoundEvent);
            sharedHealthManager.TakeDamage(amount);
        }
        else
        {
            Debug.LogWarning($"{name}: nessun componente SharedHealth trovato o assegnato.", this);
        }
    }
}