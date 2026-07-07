using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ComboMeterUI : MonoBehaviour
{
    // Singleton: ci permette di chiamare questo script facilmente da qualsiasi altra parte del gioco
    public static ComboMeterUI Instance;

    [Header("Grafica")]
    public Image anelloImage; // Trascina qui l'AnelloCombo
    public RectTransform oggettoDaTremare; // Trascina qui l'oggetto della Faccina

    [Header("Valori Combo")]
    private float maxCombo;
    private float currentCombo;
    private float differenzaPuntiCombo = 0f;

    [Header("Effetto Tremolio")]
    public float durataTremolio = 0.15f;
    public float forzaTremolio = 5f;
    [Header("Player")]
    public Player player;

    private Vector2 posizioneOriginale;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        maxCombo = player.maxMusicPoints;
        currentCombo = player.currentMusicPoints;
        if (anelloImage != null) anelloImage.fillAmount = 0f;
        if (oggettoDaTremare != null) posizioneOriginale = oggettoDaTremare.anchoredPosition;
    }

    void Update()
    {
        VerificaCambioPuntiCombo();
        AggiornaPuntiCombo();
    }

    // Questa funzione verr� chiamata dalla Hitbox
    /*public void AggiungiCombo(float quantita)
    {
        // 1. Aumenta il valore della combo
        currentCombo += quantita;

        // 2. Controllo: se raggiunge o supera il massimo, azzera tutto
        if (currentCombo >= maxCombo)
        {
            currentCombo = 0f;

            // Qui puoi chiamare un'altra funzione quando la barra si riempie!
            // es AttivaPowerUp(); oppure RiproduciSuonoMaxCombo();
        }

        // 3. Aggiorna l'interfaccia grafica
        if (anelloImage != null)
            anelloImage.fillAmount = currentCombo / maxCombo;

        // 4. Fa partire l'animazione di tremolio
        if (oggettoDaTremare != null)
        {
            StopAllCoroutines(); // Blocca tremolii precedenti se colpisci molto veloce
            StartCoroutine(TremolioRoutine());
        }
    }*/

    private void AggiornaPuntiCombo()
    {
        if(differenzaPuntiCombo == 0) return;
        if (anelloImage != null)
            anelloImage.fillAmount = currentCombo / maxCombo;

        if (oggettoDaTremare != null && differenzaPuntiCombo > 0)
        {
            StopAllCoroutines(); // Blocca tremolii precedenti se colpisci molto veloce
            StartCoroutine(TremolioRoutine());
        }
    }

    private void VerificaCambioPuntiCombo()
    {
        if(player.currentMusicPoints != currentCombo)
        {
            differenzaPuntiCombo = player.currentMusicPoints - currentCombo;
            currentCombo = player.currentMusicPoints;
        }
        else
        {
            differenzaPuntiCombo = 0f;
        }
    }

    private IEnumerator TremolioRoutine()
    {
        float tempoTrascorso = 0f;

        while (tempoTrascorso < durataTremolio)
        {
            // Crea una posizione tremolante casuale
            float xOffset = Random.Range(-1f, 1f) * forzaTremolio;
            float yOffset = Random.Range(-1f, 1f) * forzaTremolio;

            oggettoDaTremare.anchoredPosition = new Vector2(posizioneOriginale.x + xOffset, posizioneOriginale.y + yOffset);

            tempoTrascorso += Time.deltaTime;
            yield return null;
        }

        // Rimette la faccina esattamente al suo posto alla fine
        oggettoDaTremare.anchoredPosition = posizioneOriginale;
    }
}