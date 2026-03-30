using UnityEngine;

public class DamageReceiver : MonoBehaviour
{
    // Trascina qui l'oggetto che ha lo script SharedHealth
    public SharedHealth sharedHealthManager;

    public void TakeDamage(float amount)
    {
        if (sharedHealthManager != null)
        {
            // Invece di scalare la vita locale, chiama quella globale
            sharedHealthManager.TakeDamageGlobal(amount);
        }
    }
}