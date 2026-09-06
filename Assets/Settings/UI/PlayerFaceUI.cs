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
    [SerializeField] private Sprite cocaFace;
    [SerializeField] private Sprite marjuanaFace;
    [SerializeField] private Sprite lsdFace;
    [SerializeField] private Sprite mdmaFace;
    [SerializeField] private Sprite badTripFace;

    void Update()
    {
        // Evitiamo errori se i riferimenti non sono stati assegnati
        if (player == null || faceImage == null) return;

        // Gestione della priorit� degli stati

        // 1. Bad Trip (Priorit� massima)
        if (player.mentalStatus == PlayerMentalStatus.BADTRIP)
        {
            faceImage.sprite = badTripFace;
        }
        // 2. Sotto l'effetto di una droga (ma non in Bad Trip)
        // Guardando il tuo codice, quando una droga fa effetto senza bad trip, 
        // lo status diventa STUNNED oppure consumedDrug non � NONE.
        else if (player.consumedDrug != DrugType.NONE || player.mentalStatus == PlayerMentalStatus.STUNNED)
        {
            if(player.consumedDrug == DrugType.COCAINE)
            {
                faceImage.sprite = cocaFace;
            }
            else if (player.consumedDrug == DrugType.MARIJUANA)
            {
                faceImage.sprite = marjuanaFace;
            }
            else if (player.consumedDrug == DrugType.LSD)
            {
                faceImage.sprite = lsdFace;
            }
            else if (player.consumedDrug == DrugType.MDMA)
            {
                faceImage.sprite = mdmaFace;
            }
        }
        // 3. Stato Normale (Nessuna droga, nessun malus)
        else
        {
            faceImage.sprite = defaultFace;
        }
    }
}