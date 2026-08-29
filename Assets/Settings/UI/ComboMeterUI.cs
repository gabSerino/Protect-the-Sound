using UnityEngine;
using UnityEngine.UI;

public class ComboMeterUI : MonoBehaviour
{
    public static ComboMeterUI Instance;

    [Header("Grafica")]
    public Image anelloImage;

    [Header("Elementi a Barra Carica")]
    [Tooltip("Trascina qui il GameObject (o il contenitore) delle scritte che devono apparire solo a barra piena")]
    public GameObject testoComboCarica;

    [Header("Player")]
    public Player player;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (anelloImage != null) anelloImage.fillAmount = 0f;

        // Assicuriamoci che all'inizio del gioco la scritta sia nascosta
        if (testoComboCarica != null) testoComboCarica.SetActive(false);
    }

    void Update()
    {
        // Controllo di sicurezza: interrompe se il player non c'è o i punti massimi non sono impostati
        if (player == null || player.maxMusicPoints <= 0) return;

        // 1. Calcola la percentuale di carica (valore da 0 a 1)
        float percentuale = player.currentMusicPoints / player.maxMusicPoints;

        // 2. Aggiorna il riempimento dell'anello
        if (anelloImage != null)
        {
            anelloImage.fillAmount = percentuale;
        }

        // 3. Controlla se la barra è carica al 100%
        bool isCarica = percentuale >= 1f;

        // 4. Attiva o disattiva il testo di conseguenza
        if (testoComboCarica != null)
        {
            testoComboCarica.SetActive(isCarica);
        }
    }

    // Mantenuta vuota per sicurezza nel caso la Hitbox la stia ancora chiamando
    public void AggiungiCombo(float quantita)
    {
    }
}