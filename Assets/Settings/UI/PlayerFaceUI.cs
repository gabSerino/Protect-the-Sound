using UnityEngine;
using UnityEngine.UI;

public class PlayerFaceUI : MonoBehaviour
{
    [Header("Riferimenti")]
    [Tooltip("Trascina qui il GameObject del Player")]
    [SerializeField] private Player player;

    [Tooltip("Trascina qui il componente Image della faccina")]
    [SerializeField] private Image faceImage;

    [Header("Sprites Faccina")]
    [SerializeField] private Sprite defaultFace;
    [SerializeField] private Sprite druggedFace;
    [SerializeField] private Sprite badTripFace;

    void Update()
    {
        // Evitiamo errori se i riferimenti non sono stati assegnati
        if (player == null || faceImage == null) return;

        // Gestione della priorità degli stati

        // 1. Bad Trip (Priorità massima)
        if (player.mentalStatus == PlayerMentalStatus.BADTRIP)
        {
            faceImage.sprite = badTripFace;
        }
        // 2. Sotto l'effetto di una droga (ma non in Bad Trip)
        // Guardando il tuo codice, quando una droga fa effetto senza bad trip, 
        // lo status diventa STUNNED oppure consumedDrug non è NONE.
        else if (player.consumedDrug != DrugType.NONE || player.mentalStatus == PlayerMentalStatus.STUNNED)
        {
            faceImage.sprite = druggedFace;
        }
        // 3. Stato Normale (Nessuna droga, nessun malus)
        else
        {
            faceImage.sprite = defaultFace;
        }
    }
}