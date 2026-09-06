using System;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

// Tipo di condizione che una riga di tutorial può richiedere per essere superata.
// "None" = riga puramente narrativa, si avanza col click.
// Le altre richiedono che una statistica globale raggiunga una certa soglia:
// finché non è soddisfatta la riga resta nascosta (vedi flusso in TutorialManager).
public enum TutorialConditionType
{
    None,
    KillEnemies,
    DrinkWater,
    UseItems,
    BarIsCharged,
    ChargeBarEmpty,
    DashCount
}

// Categorie di comandi del player attivabili/disattivabili indipendentemente.
[Flags]
public enum PlayerControlFlags
{
    None = 0,
    Movement = 1 << 0,
    Dash = 1 << 1,
    Attack = 1 << 2,
    Inventory = 1 << 3,
    All = ~0
}

[Serializable]
public class TutorialLine
{
    [TextArea(2, 4)]
    public string text;

    public TutorialConditionType conditionType = TutorialConditionType.None;

    // Usato solo per KillEnemies / UseItems / DashCount (soglia da raggiungere).
    // Ignorato per None, DrinkWater, BarIsCharged e ChargeBarEmpty (semplici bool).
    public int requiredAmount = 1;

    [Tooltip("Comandi attivi mentre questa riga è IN ATTESA (box nascosto) che la sua condizione venga soddisfatta. Ignorato se conditionType è None.")]
    public PlayerControlFlags controlsWhileWaiting = PlayerControlFlags.None;

    [Header("Spawn nemici durante l'attesa (opzionale, coesiste con la condizione)")]
    [Tooltip("Se attivo, lo spawner viene acceso finché questa riga è in attesa (box nascosto), e spento non appena si esce dall'attesa.")]
    public bool spawnEnemiesWhileWaiting = false;

    [Tooltip("GameObject dello spawner da attivare/disattivare per questa riga. Di base è sempre disattivo.")]
    public GameObject enemySpawner;

    [Header("Attivazione acqua (opzionale, indipendente dalla condizione)")]
    [Tooltip("Se attivo, appena si entra in questa riga (sia che resti in attesa sia che venga mostrata subito) vengono attivati lo spawner e l'item dell'acqua qui sotto. Vengono disattivati non appena si passa alla riga successiva.")]
    public bool activateWater = false;

    [Tooltip("GameObject dello spawner dell'acqua da attivare/disattivare per questa riga.")]
    public GameObject waterSpawner;

    [Tooltip("GameObject dell'item acqua da attivare/disattivare per questa riga.")]
    public GameObject waterItem;
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Contenuto Tutorial")]
    public TutorialLine[] lines;

    [Header("Riferimenti UI")]
    [Tooltip("Root del box/canvas del tutorial, viene attivato/disattivato automaticamente.")]
    public GameObject tutorialCanvas;
    public TMP_Text lineTextUI;

    [Header("Variabili Universali (stato di gioco tracciato dal tutorial)")]
    public int totalKilledEnemies = 0;
    public bool drankWater = false;
    public int totalUsedItems = 0;
    public bool barIsCharged = false;
    public int totalDashCount = 0;

    // Evento utile per UI aggiuntiva (es. evidenziare l'obiettivo corrente,
    // aggiornare una progress bar "2/3 nemici uccisi", ecc.)
    public event Action<int, TutorialLine> OnLineChanged;
    public event Action OnTutorialCompleted;

    public Player player;

    private int currentIndex = -1;
    private bool tutorialActive = true;

    // true quando il box è effettivamente a schermo (riga narrativa o condizione
    // già soddisfatta). false quando siamo "in attesa" di un'azione di gameplay:
    // in quel caso il box è nascosto ma currentIndex punta comunque alla riga pendente.
    private bool lineVisible = false;

    // Riga (se presente) il cui spawner è attualmente acceso perché in attesa.
    // Serve per sapere quale spawner spegnere/quando distruggere i nemici.
    private TutorialLine activeSpawnerLine = null;

    // Riga (se presente) per cui sono attualmente accesi spawner/item dell'acqua.
    private TutorialLine activeWaterLine = null;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    void Start()
    {
        // Rete di sicurezza: a prescindere da cosa succede sotto, il player
        // parte sempre bloccato finché non viene mostrata/valutata una riga.
        ApplyControls(PlayerControlFlags.None);

        // Di base tutti gli spawner sono disattivi: si accendono solo durante
        // l'attesa della riga a cui appartengono (vedi TryShowIndex).
        if (lines != null)
        {
            foreach (var l in lines)
            {
                if (l.enemySpawner != null)
                    l.enemySpawner.SetActive(false);
                if (l.waterSpawner != null)
                    l.waterSpawner.SetActive(false);
                if (l.waterItem != null)
                    l.waterItem.SetActive(false);
            }
        }

        if (lines != null && lines.Length > 0)
        {
            TryShowIndex(0);
        }
        else
        {
            SetCanvasVisible(false);
            Debug.LogWarning("[TutorialManager] Nessuna riga configurata in 'lines': il tutorial non partirà.");
        }
    }

    void Update()
    {
        if (tutorialActive && Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            CompleteTutorial();
            return;
        }

        // L'avanzamento è sempre guidato dal left click, ma solo quando c'è
        // effettivamente una riga a schermo da "chiudere". Se siamo in attesa
        // di una condizione (box nascosto) il click non ha effetto: si aspetta
        // che l'azione di gameplay richiesta venga compiuta.
        bool advancePressed = Input.GetMouseButtonDown(0) ||
                              (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);

        if (tutorialActive && lineVisible && advancePressed)
        {
            AdvanceFromClick();
        }
    }

    // ---------------------------------------------------------------------
    // Navigazione
    // ---------------------------------------------------------------------

    private void AdvanceFromClick()
    {
        HideCurrentLine();
        TryShowIndex(currentIndex + 1);
    }

    private void HideCurrentLine()
    {
        lineVisible = false;
        SetCanvasVisible(false);
    }

    // Prova a mostrare la riga a un dato indice. Se la riga ha una condizione
    // non ancora soddisfatta, NON viene mostrata: resta pendente, il box
    // rimane nascosto e al player vengono dati solo i comandi previsti per
    // quella riga (controlsWhileWaiting), non necessariamente tutti.
    private void TryShowIndex(int index)
    {
        if (index >= lines.Length)
        {
            CompleteTutorial();
            return;
        }

        // Si lascia la riga precedente: spegniamo i suoi eventuali oggetti acqua
        // prima di passare a quella nuova.
        DeactivateCurrentWater();

        currentIndex = index;
        var line = lines[currentIndex];

        // L'acqua si attiva subito all'ingresso nella riga, indipendentemente
        // dal fatto che la riga resti in attesa o venga mostrata immediatamente.
        ActivateWaterIfNeeded(line);

        if (line.conditionType != TutorialConditionType.None && !IsConditionMet(line))
        {
            lineVisible = false;
            SetCanvasVisible(false);
            ApplyControls(line.controlsWhileWaiting);
            ActivateSpawnerForWaitingLine(line);
            return;
        }

        ShowLine(currentIndex);
    }

    private void ShowLine(int index)
    {
        currentIndex = index;
        var line = lines[currentIndex];

        // Si esce dallo stato di attesa: se c'era uno spawner acceso per questa
        // riga, si spegne e si ripulisce il campo dai nemici eventualmente spawnati.
        DeactivateCurrentSpawner();

        lineVisible = true;
        SetCanvasVisible(true);
        if (lineTextUI != null)
            lineTextUI.text = line.text;

        // Mentre il box è a schermo il player è considerato "in lettura":
        // nessun comando attivo, a prescindere dal tipo di riga.
        ApplyControls(PlayerControlFlags.None);

        OnLineChanged?.Invoke(currentIndex, line);
    }

    private void CompleteTutorial()
    {
        tutorialActive = false;
        lineVisible = false;
        SetCanvasVisible(false);
        DeactivateCurrentSpawner();
        DeactivateCurrentWater();
        ApplyControls(PlayerControlFlags.All);
        OnTutorialCompleted?.Invoke();
        StartCoroutine(EndTutorial());
    }

    private IEnumerator EndTutorial()
    {
        yield return new WaitForSeconds(1f);
        UnityEngine.SceneManagement.SceneManager.LoadScene(2);
    }

    private void SetCanvasVisible(bool visible)
    {
        if (tutorialCanvas != null)
            tutorialCanvas.SetActive(visible);
    }

    // ---------------------------------------------------------------------
    // Comandi del player
    // ---------------------------------------------------------------------

    // Applica ai comandi del player ESATTAMENTE le flag richieste: abilita quelle
    // presenti E disabilita esplicitamente quelle assenti (altrimenti un comando
    // abilitato in una riga precedente resterebbe attivo anche dove non dovrebbe).
    // TODO: richiede in Player i corrispettivi DisableMovement/DisableDash/
    // DisableAttack/DisableInventory accanto agli Enable* già presenti.
    private void ApplyControls(PlayerControlFlags flags)
    {
        if (player == null) return;

        bool movement = (flags & PlayerControlFlags.Movement) != 0;
        bool dash = (flags & PlayerControlFlags.Dash) != 0;
        bool attack = (flags & PlayerControlFlags.Attack) != 0;
        bool inventory = (flags & PlayerControlFlags.Inventory) != 0;

        if (movement) player.EnableMovement(); else player.DisableMovement();
        if (dash) player.EnableDash(); else player.DisableDash();
        if (attack) player.EnableAttack(); else player.DisableAttack();
        if (inventory) player.EnableInventory(); else player.DisableInventory();
    }

    // ---------------------------------------------------------------------
    // Spawn nemici durante l'attesa
    // ---------------------------------------------------------------------

    private void ActivateSpawnerForWaitingLine(TutorialLine line)
    {
        if (line.spawnEnemiesWhileWaiting && line.enemySpawner != null)
        {
            line.enemySpawner.SetActive(true);
            activeSpawnerLine = line;
        }
    }

    // Spegne lo spawner della riga attualmente in attesa (se c'è) e distrugge
    // tutti i nemici ancora presenti sul campo, per non lasciarli "orfani"
    // quando si esce dalla finestra di attesa di quella riga.
    private void DeactivateCurrentSpawner()
    {
        if (activeSpawnerLine == null) return;

        if (activeSpawnerLine.enemySpawner != null)
            activeSpawnerLine.enemySpawner.SetActive(false);

        DestroyAllSpawnedEnemies();
        activeSpawnerLine = null;
    }

    /// <summary>Distrugge tutti i GameObject con componente EnemyBase presenti sul campo.</summary>
    public void DestroyAllSpawnedEnemies()
    {
        EnemyBase[] enemies = FindObjectsOfType<EnemyBase>();
        foreach (var enemy in enemies)
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }
    }

    // ---------------------------------------------------------------------
    // Attivazione acqua
    // ---------------------------------------------------------------------

    private void ActivateWaterIfNeeded(TutorialLine line)
    {
        if (!line.activateWater) return;

        if (line.waterSpawner != null) line.waterSpawner.SetActive(true);
        if (line.waterItem != null) line.waterItem.SetActive(true);
        activeWaterLine = line;
    }

    private void DeactivateCurrentWater()
    {
        if (activeWaterLine == null) return;

        if (activeWaterLine.waterSpawner != null) activeWaterLine.waterSpawner.SetActive(false);
        if (activeWaterLine.waterItem != null) activeWaterLine.waterItem.SetActive(false);
        activeWaterLine = null;
    }

    // ---------------------------------------------------------------------
    // Valutazione condizioni
    // ---------------------------------------------------------------------

    private bool IsConditionMet(TutorialLine line)
    {
        switch (line.conditionType)
        {
            case TutorialConditionType.KillEnemies:
                return totalKilledEnemies >= line.requiredAmount;
            case TutorialConditionType.DrinkWater:
                return drankWater;
            case TutorialConditionType.UseItems:
                return totalUsedItems >= line.requiredAmount;
            case TutorialConditionType.BarIsCharged:
                return barIsCharged;
            case TutorialConditionType.ChargeBarEmpty:
                return !barIsCharged;
            case TutorialConditionType.DashCount:
                return totalDashCount >= line.requiredAmount;
            default:
                return true;
        }
    }

    // Chiamata dopo ogni aggiornamento di una variabile universale: se la riga
    // pendente aspettava esattamente quella condizione, il box viene mostrato.
    private void CheckCurrentCondition()
    {
        if (!tutorialActive) return;
        if (lineVisible) return; // il box è già a schermo, non c'è nulla in attesa
        if (currentIndex < 0 || currentIndex >= lines.Length) return;

        var current = lines[currentIndex];
        if (current.conditionType != TutorialConditionType.None && IsConditionMet(current))
        {
            ShowLine(currentIndex);
        }
    }

    // ---------------------------------------------------------------------
    // Funzioni pubbliche da chiamare dal resto del gioco (Enemy, Player, Item...)
    // ---------------------------------------------------------------------

    /// <summary>Da chiamare quando un nemico viene ucciso (es. in Enemy.Die()).</summary>
    public void RegisterEnemyKilled(int amount = 1)
    {
        totalKilledEnemies += amount;
        CheckCurrentCondition();
    }

    /// <summary>Da chiamare quando il player beve acqua per la prima volta.</summary>
    public void RegisterWaterDrank()
    {
        drankWater = true;
        CheckCurrentCondition();
    }

    /// <summary>Da chiamare ogni volta che il player usa un item (consumabile, arma, ecc.).</summary>
    public void RegisterItemUsed(int amount = 1)
    {
        totalUsedItems += amount;
        CheckCurrentCondition();
    }

    /// <summary>Da chiamare quando la barra (es. carica speciale/energia) raggiunge il massimo.</summary>
    public void RegisterBarCharged()
    {
        barIsCharged = true;
        CheckCurrentCondition();
    }

    /// <summary>
    /// Setter esplicito per la barra, utile se può anche scaricarsi (es. il player
    /// consuma la carica prima di procedere e la barra torna sotto soglia).
    /// Controlla sempre la condizione corrente, dato che sia BarIsCharged che
    /// ChargeBarEmpty dipendono da questo stesso valore.
    /// </summary>
    public void SetBarCharged(bool charged)
    {
        barIsCharged = charged;
        CheckCurrentCondition();
    }

    public void RegisterBarEmpty() => SetBarCharged(false);

    /// <summary>Da chiamare ogni volta che il player esegue un dash.</summary>
    public void RegisterDash(int amount = 1)
    {
        totalDashCount += amount;
        CheckCurrentCondition();
    }

    // ---------------------------------------------------------------------
    // Utility
    // ---------------------------------------------------------------------

    public TutorialLine GetCurrentLine()
    {
        if (currentIndex < 0 || currentIndex >= lines.Length) return null;
        return lines[currentIndex];
    }

    public int GetCurrentIndex() => currentIndex;

    public bool IsTutorialActive() => tutorialActive;

    public bool IsLineVisible() => lineVisible;

    /// <summary>Riporta il tutorial all'inizio e azzera le variabili universali.</summary>
    public void ResetTutorial()
    {
        totalKilledEnemies = 0;
        drankWater = false;
        totalUsedItems = 0;
        barIsCharged = false;
        totalDashCount = 0;
        tutorialActive = true;
        currentIndex = -1;
        DeactivateCurrentSpawner();
        DeactivateCurrentWater();
        TryShowIndex(0);
    }
}