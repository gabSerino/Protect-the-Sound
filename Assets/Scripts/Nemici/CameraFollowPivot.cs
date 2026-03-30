using UnityEngine;

public class CameraFollowPivot : MonoBehaviour
{
    public Transform giocatore;
    public Transform cassa;
    public float raggioFisso = 7f; // La distanza che non deve MAI cambiare
    public float altezzaFissa = 3f; // L'altezza costante della camera
    public float fluidita = 5f;

    void LateUpdate()
    {
        if (giocatore == null || cassa == null) return;

        // 1. Calcoliamo il vettore dalla cassa al giocatore
        Vector3 versoGiocatore = giocatore.position - cassa.position;

        // 2. IMPORTANTE: Ignoriamo l'altezza (Y) per il calcolo della direzione
        // Questo evita che la camera faccia "salti" se il player salta
        versoGiocatore.y = 0;

        // 3. Se il giocatore è esattamente sopra la cassa, evitiamo errori
        if (versoGiocatore.magnitude < 0.1f) return;

        // 4. Creiamo la posizione target usando solo la DIREZIONE normalizzata
        // Moltiplichiamo la direzione per il raggio fisso, indipendentemente da dove sia il player
        Vector3 posizioneTarget = cassa.position + (versoGiocatore.normalized * raggioFisso);
        posizioneTarget.y = cassa.position.y + altezzaFissa;

        // 5. Movimento fluido verso il punto calcolato
        transform.position = Vector3.Lerp(transform.position, posizioneTarget, fluidita * Time.deltaTime);

        // 6. Guarda sempre la cassa
        transform.LookAt(cassa);
    }
}