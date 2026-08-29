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

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (anelloImage != null) anelloImage.fillAmount = 0f;
        if (testoComboCarica != null) testoComboCarica.SetActive(false);
    }

    // Usiamo LateUpdate per assicurarci che il Player abbia già azzerato i punti in questo frame
    void LateUpdate()
    {
        if (player == null || player.maxMusicPoints <= 0) return;

        // FASE DI SCELTA VINILE:
        if (player.canChangeMusicType)
        {
            if (anelloImage != null)
            {
                anelloImage.enabled = false;
                anelloImage.fillAmount = 0f; // Assicura che sia già a zero per quando riapparirà
            }

            if (testoComboCarica != null)
                testoComboCarica.SetActive(true);

            return; // Usciamo subito: la scelta è in corso
        }

        // FASE DI GIOCO NORMALE:
        float percentuale = player.currentMusicPoints / player.maxMusicPoints;

        if (anelloImage != null)
        {
            anelloImage.enabled = true;
            anelloImage.fillAmount = percentuale;
        }

        bool isCarica = player.currentMusicPoints >= player.musicPtsThreshold;
        if (testoComboCarica != null)
        {
            testoComboCarica.SetActive(isCarica);
        }
    }

    public void AggiungiCombo(float quantita)
    {
    }
}