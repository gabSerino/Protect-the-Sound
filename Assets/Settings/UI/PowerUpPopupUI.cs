using System.Collections.Generic;
using UnityEngine;
using TMPro;

public struct StatChangeEntry
{
    public string label;   // es. "ATK", "SPD", "HP"
    public float value;    // il delta, positivo o negativo
    public bool isPercent; // true se va mostrato con il simbolo %

    public StatChangeEntry(string label, float value, bool isPercent = false)
    {
        this.label = label;
        this.value = value;
        this.isPercent = isPercent;
    }
}

public class PowerUpPopupUI : MonoBehaviour
{
    public static PowerUpPopupUI Instance { get; private set; }

    [Header("Riferimenti")]
    [Tooltip("Trascina qui il GameObject del Player (come in PlayerFaceUI)")]
    [SerializeField] private Player player;

    [Header("Setup Grafica Text & Cornice")]
    [Tooltip("RectTransform (dentro un Canvas) sotto cui viene istanziato il popup")]
    [SerializeField] private RectTransform popupParent;
    [Tooltip("Prefab con solo un componente TextMeshProUGUI")]
    [SerializeField] private TextMeshProUGUI popupPrefab;
    [Tooltip("GameObject della cornice che incornicia il testo. Si attiva solo quando c'è un testo valido.")]
    [SerializeField] private GameObject popupFrame;

    [Header("Effetti Visivi Extra")]
    [Tooltip("Elemento grafico visibile SOLO quando il giocatore è in stato BADTRIP")]
    [SerializeField] private GameObject badTripGraphic;

    [Header("Colori")]
    [SerializeField] private Color positiveColor = new Color(0.35f, 1f, 0.35f);
    [SerializeField] private Color negativeColor = new Color(1f, 0.35f, 0.35f);
    [SerializeField] private string separator = "    ";

    [Header("Soglie Simboli (+ / ++)")]
    [Tooltip("Valore sopra il quale la variazione fissa diventa 'tanto' (es. >= 5 è ++, sotto è +)")]
    [SerializeField] private float highFlatThreshold = 5f;
    [Tooltip("Valore sopra il quale la variazione percentuale diventa 'tanto' (es. 0.2 o 20 a seconda di come salvi le percentuali)")]
    [SerializeField] private float highPercentThreshold = 20f;

    private TextMeshProUGUI currentPopup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Disattiviamo gli elementi visivi all'avvio del gioco
        if (popupFrame != null) popupFrame.SetActive(false);
        if (badTripGraphic != null) badTripGraphic.SetActive(false);
    }

    private void Update()
    {
        if (player == null) return;

        // 1. Gestione grafica del Bad Trip (attiva/disattiva ogni frame in base allo stato)
        bool isBadTrip = player.mentalStatus == PlayerMentalStatus.BADTRIP;
        if (badTripGraphic != null)
        {
            badTripGraphic.SetActive(isBadTrip);
        }

        // 2. Controllo degli effetti generali per nascondere il testo
        bool isUnderEffect =
            isBadTrip ||
            player.consumedDrug != DrugType.NONE ||
            player.mentalStatus == PlayerMentalStatus.STUNNED;

        if (!isUnderEffect)
            HidePowerUp();
    }

    public void ShowStatChanges(List<StatChangeEntry> changes)
    {
        HidePowerUp();

        if (changes == null || changes.Count == 0) return;

        if (popupPrefab == null || popupParent == null)
        {
            Debug.LogWarning("PowerUpPopupUI: assegna popupPrefab e popupParent nell'Inspector.");
            return;
        }

        string text = BuildRichText(changes);

        // Se nessuna statistica tra quelle passate era ATK o SPD, non creiamo il popup
        if (string.IsNullOrEmpty(text)) return;

        currentPopup = Instantiate(popupPrefab, popupParent);
        currentPopup.text = text;
        currentPopup.alpha = 1f;

        // Attiviamo la cornice solo quando abbiamo generato un testo valido
        if (popupFrame != null)
        {
            popupFrame.SetActive(true);
        }
    }

    public void HidePowerUp()
    {
        if (currentPopup != null)
        {
            Destroy(currentPopup.gameObject);
            currentPopup = null;
        }

        // Nascondiamo anche la cornice
        if (popupFrame != null)
        {
            popupFrame.SetActive(false);
        }
    }

    private string BuildRichText(List<StatChangeEntry> changes)
    {
        var parts = new List<string>();

        foreach (var change in changes)
        {
            if (!IsAttackOrSpeed(change.label)) continue;
            if (Mathf.Approximately(change.value, 0f)) continue;

            string symbol = GetSymbol(change.value, change.isPercent);
            Color color = change.value >= 0f ? positiveColor : negativeColor;
            string colorHex = ColorUtility.ToHtmlStringRGB(color);

            parts.Add($"<color=#{colorHex}>{symbol} {change.label.ToUpper()}</color>");
        }

        return string.Join(separator, parts);
    }

    private bool IsAttackOrSpeed(string label)
    {
        string l = label.ToUpper().Trim();
        return l == "ATK" || l == "ATTACK" || l == "ATTACCO" ||
               l == "SPD" || l == "SPEED" || l == "VELOCITÀ" || l == "VELOCITA";
    }

    private string GetSymbol(float value, bool isPercent)
    {
        float threshold = isPercent ? highPercentThreshold : highFlatThreshold;
        float absValue = Mathf.Abs(value);

        if (value > 0f)
        {
            return absValue >= threshold ? "++" : "+";
        }
        else
        {
            return absValue >= threshold ? "--" : "-";
        }
    }
}