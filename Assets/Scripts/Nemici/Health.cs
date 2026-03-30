using UnityEngine;

public class Health : MonoBehaviour
{
    public float points = 100f; // Salute iniziale

    public void TakeDamage(float amount)
    {
        points -= amount;
        Debug.Log(gameObject.name + " ha ricevuto danni! Salute rimasta: " + points);

        if (points <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " è stato distrutto!");
        // Qui puoi aggiungere effetti, suoni o il Game Over
        Destroy(gameObject);
    }
}