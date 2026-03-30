using UnityEngine;

public class CameraFollowPivot : MonoBehaviour
{
    public Transform giocatore;
    public Transform cassa;

    [Header("Impostazioni Distanza")]
    public float raggioFisso = 7f;
    public float altezzaFissa = 5f;
    public float fluidita = 5f;

    [Header("Limiti di Rotazione")]
    public Vector3 direzioneCentrale = Vector3.back; // La linea tratteggiata (es. Vector3.back o forward)
    public float limiteAngolare = 150f; // Ampiezza totale in gradi

    void LateUpdate()
    {
        if (giocatore == null || cassa == null) return;

        // 1. Calcoliamo il vettore direzione dal pivot (cassa) al giocatore
        Vector3 versoGiocatore = giocatore.position - cassa.position;
        versoGiocatore.y = 0; // Ignoriamo l'altezza per il calcolo dell'angolo

        // 2. Calcoliamo l'angolo attuale del giocatore rispetto alla direzione centrale
        float angoloGiocatore = Vector3.SignedAngle(direzioneCentrale, versoGiocatore, Vector3.up);

        // 3. Applichiamo il limite (Clamp)
        // Se il limite è 150, la camera oscillerà tra -75 e +75
        float semiAmpiezza = limiteAngolare / 2f;
        float angoloLimitato = Mathf.Clamp(angoloGiocatore, -semiAmpiezza, semiAmpiezza);

        // 4. Trasformiamo l'angolo limitato di nuovo in una direzione (Vector3)
        Quaternion rotazioneLimitata = Quaternion.AngleAxis(angoloLimitato, Vector3.up);
        Vector3 direzioneFinale = rotazioneLimitata * direzioneCentrale;

        // 5. Posizioniamo la camera
        Vector3 posizioneTarget = cassa.position + (direzioneFinale.normalized * raggioFisso);
        posizioneTarget.y = cassa.position.y + altezzaFissa;

        transform.position = Vector3.Lerp(transform.position, posizioneTarget, fluidita * Time.deltaTime);

        // 6. Guarda la cassa
        transform.LookAt(cassa);
    }

    // Disegna il raggio d'azione nell'editor per aiutarti a settarlo
    void OnDrawGizmosSelected()
    {
        if (cassa == null) return;
        Gizmos.color = Color.yellow;
        Vector3 destra = Quaternion.AngleAxis(limiteAngolare / 2f, Vector3.up) * direzioneCentrale;
        Vector3 sinistra = Quaternion.AngleAxis(-limiteAngolare / 2f, Vector3.up) * direzioneCentrale;
        Gizmos.DrawLine(cassa.position, cassa.position + destra * raggioFisso);
        Gizmos.DrawLine(cassa.position, cassa.position + sinistra * raggioFisso);
    }
}