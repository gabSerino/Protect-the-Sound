using UnityEngine;
using UnityEngine.UI;

public class ComboMeterUI : MonoBehaviour
{
    public static ComboMeterUI Instance;

    [Header("Grafica")]
    public Image anelloImage;

    [Header("Elementi a Barra Carica")]
    public GameObject testoComboCarica;

    [Header("Player")]
    public Player player;

    // NUOVA VARIABILE: Memorizza se siamo nella fase in cui la barra si sta esaurendo
    private bool isDrainingPhase = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (anelloImage != null) anelloImage.fillAmount = 0f;
        if (testoComboCarica != null) testoComboCarica.SetActive(false);
    }

    void LateUpdate()
    {
        if (player == null || player.maxMusicPoints <= 0) return;

        // FASE DI SCELTA VINILE:
        if (player.canChangeMusicType)
        {
            // Registriamo che abbiamo attivato l'abilità. 
            // Da questo momento in poi (finché non si azzera), siamo in fase di svuotamento.
            isDrainingPhase = true;

            if (anelloImage != null)
            {
                anelloImage.enabled = false;
                anelloImage.fillAmount = 0f;
            }

            if (testoComboCarica != null)
                testoComboCarica.SetActive(true);

            return;
        }

        // FASE DI GIOCO NORMALE:

        // Quando la barra si svuota del tutto (o quasi, per sicurezza coi float), usciamo dalla fase di svuotamento.
        // A questo punto il giocatore potrà ricominciare a caricare la combo.
        if (player.currentMusicPoints <= 0.1f)
        {
            isDrainingPhase = false;
        }

        float percentuale = player.currentMusicPoints / player.maxMusicPoints;

        if (anelloImage != null)
        {
            anelloImage.enabled = true;
            anelloImage.fillAmount = percentuale;
        }

        // --- LA MAGIA AVVIENE QUI ---
        // Il testo appare SOLO se hai superato la soglia E NON sei nella fase di svuotamento
        bool isCarica = (player.currentMusicPoints >= player.musicPtsThreshold) && !isDrainingPhase;

        if (testoComboCarica != null)
        {
            testoComboCarica.SetActive(isCarica);
        }
    }

    public void AggiungiCombo(float quantita)
    {
    }
}