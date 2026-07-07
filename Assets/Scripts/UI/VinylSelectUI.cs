using UnityEngine;

public class VinylSelectUI : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private MusicType discType; // Il tipo di musica rappresentato da QUESTO vinile specifico
    
    [Header("Movement Settings")]
    [SerializeField] private float lerpSpeed = 10f; // Velocità di spostamento fluido
    [SerializeField] private float moveDistance = 100f; // Distanza massima di spostamento originale

    private RectTransform rectTransform;
    private Vector2 startPosition;
    private Vector2 targetPosition;
    private bool isDown = false; // Traccia se l'UI è attualmente spostata verso il basso

    void Start()
    {
        if (player == null) 
            player = GameObject.Find("Player").GetComponent<Player>();
            
        rectTransform = GetComponent<RectTransform>();
        
        // Salviamo la posizione di partenza iniziale dell'UI
        startPosition = rectTransform.anchoredPosition;
        targetPosition = startPosition;

        // Controllo iniziale all'avvio (avviene solo se il player può effettivamente cambiare musica)
        if (player != null && player.canChangeMusicType && player.selectedMusicType == discType)
        {
            float currentDistance = CalcolaDistanzaSpostamento();
            targetPosition = new Vector2(startPosition.x, startPosition.y - currentDistance);
            rectTransform.anchoredPosition = targetPosition; // Posizionamento istantaneo solo al primo frame
            isDown = true;
        }
    }

    void Update()
    {
        if (player == null) return;

        // 1. Controlliamo se la destinazione deve cambiare o aggiornarsi
        CheckTargetPositionChange();

        // 2. Muoviamo gradualmente l'UI verso la targetPosition
        rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPosition, Time.deltaTime * lerpSpeed);
    }

    private void CheckTargetPositionChange()
    {
        // NUOVO CONTROLLO: Se il player NON può cambiare musica, blocchiamo l'evidenziamento 
        // e forziamo il disco a tornare/restare su (startPosition)
        if (!player.canChangeMusicType)
        {
            if (isDown)
            {
                targetPosition = startPosition;
                isDown = false;
            }
            return; // Interrompiamo il metodo qui, ignorando i controlli successivi
        }

        // Se questo vinile è quello attualmente selezionato dal Player (e può cambiare musica)
        if (player.selectedMusicType == discType)
        {
            // Calcoliamo la distanza corretta (piena o ridotta a 2/3)
            float attualeDistanza = CalcolaDistanzaSpostamento();
            Vector2 nuovoTarget = new Vector2(startPosition.x, startPosition.y - attualeDistanza);

            // Aggiorna il target se cambia la distanza (es. il RhythmManager ha appena cambiato traccia) o se non era ancora giù
            if (!isDown || targetPosition != nuovoTarget)
            {
                targetPosition = nuovoTarget;
                isDown = true;
            }
        }
        // Se NON è il vinile selezionato dal Player, deve tornare su
        else if (player.selectedMusicType != discType && isDown)
        {
            targetPosition = startPosition;
            isDown = false;
        }
    }

    /// <summary>
    /// Calcola la distanza di movimento: moveDistance originale, 
    /// oppure i 2/3 se coincide con il RhythmManager.
    /// </summary>
    private float CalcolaDistanzaSpostamento()
    {
        if (RhythmManager.Instance != null && discType == RhythmManager.Instance.musicType)
        {
            // Ritorna i 2/3 della distanza originale (corretto da 1f/2f a 2f/3f)
            return moveDistance * (1f / 2f);
        }
        
        return moveDistance;
    }
}