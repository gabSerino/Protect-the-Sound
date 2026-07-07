using UnityEngine;
using UnityEngine.UI;

public class VinylUI : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private Sprite[] vinyls;
    [SerializeField] private Player player;

    [Header("Visual Settings")]
    [SerializeField] private float fadeSpeed = 5f; // Velocità della transizione di luminosità
    [SerializeField] private float dimmedBrightness = 0.3f; // 0.3 significa al 30% di luminosità (molto scuro)

    private RectTransform rectTransform;
    private Image image;
    private MusicType currentMusicType = MusicType.DEFAULT;
    
    private Color targetColor = Color.white;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        
        if (player == null) 
            player = GameObject.Find("Player").GetComponent<Player>();

        AssignVinylToMusic();
    }

    void Update()
    {
        RotateVinyl();
        CheckMusicChange();
        UpdateBrightness(); // Gestisce la transizione fluida del colore
    }
    
    private void RotateVinyl()
    {
        rectTransform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
    }

    private void AssignVinylToMusic()
    {
        if (RhythmManager.Instance == null) return;

        // Nota bene: se l'ordine dell'enum MusicType corrisponde all'indice dell'array,
        // potresti fare direttamente: image.sprite = vinyls[(int)RhythmManager.Instance.musicType];
        if (RhythmManager.Instance.musicType == MusicType.DEFAULT) image.sprite = vinyls[0];
        if (RhythmManager.Instance.musicType == MusicType.DnB) image.sprite = vinyls[1];
        if (RhythmManager.Instance.musicType == MusicType.SYNTHWAVE) image.sprite = vinyls[2];
        if (RhythmManager.Instance.musicType == MusicType.RAGGAE) image.sprite = vinyls[3];
        if (RhythmManager.Instance.musicType == MusicType.BREAKCORE) image.sprite = vinyls[4];
    }

    private void CheckMusicChange()
    {
        if (RhythmManager.Instance == null) return;

        if (RhythmManager.Instance.musicType != currentMusicType)
        {
            currentMusicType = RhythmManager.Instance.musicType;
            AssignVinylToMusic();
        }
    }

    private void UpdateBrightness()
    {
        if (player == null) return;

        // Impostiamo il colore di destinazione in base al bool del player
        if (player.canChangeMusicType)
        {
            targetColor = Color.white; // Bianco = Colore originale al 100% di luminosità
        }
        else
        {
            // Grigio scuro mantenendo l'alpha (trasparenza) originale dell'immagine
            targetColor = new Color(dimmedBrightness, dimmedBrightness, dimmedBrightness, image.color.a);
        }

        // Applichiamo il cambio di colore in modo fluido
        image.color = Color.Lerp(image.color, targetColor, fadeSpeed * Time.deltaTime); // Lerp = Interpolation
    }
}