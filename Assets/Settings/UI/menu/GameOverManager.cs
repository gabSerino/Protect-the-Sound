using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Aggiunto per il testo UI

public class GameOverManager : MonoBehaviour
{
    public static bool GameOverAttivo { get; private set; } = false;
    public static int NemiciUccisi { get; private set; } = 0; // Traccia lo score

    [Header("Interfaccia UI")]
    [SerializeField] private GameObject gameOverMenuUI;
    [SerializeField] private TextMeshProUGUI scoreText; // Riferimento al testo dello score

    [Header("References")]
    [SerializeField] private PlayerInputManager playerInputManager;

    private void Awake()
    {
        // Reset dello score ogni volta che si ricarica la scena
        NemiciUccisi = 0;
    }

    // Metodo da chiamare quando muore un nemico
    public static void AggiungiUccisione()
    {
        NemiciUccisi++;
    }

    public void AttivaGameOver()
    {
        if (GameOverAttivo) return;

        GameOverAttivo = true;
        PauseMenu.GiocoInPausa = false;

        // Aggiorna il testo prima di attivare l'interfaccia
        if (scoreText != null)
            scoreText.text = $"{NemiciUccisi}";

        if (gameOverMenuUI != null)
            gameOverMenuUI.SetActive(true);
        else
            Debug.LogError("GameOverMenuUI non assegnato nel GameOverManager.");

        if (playerInputManager != null)
            playerInputManager.DisableAllControls();

        Time.timeScale = 0f;

        FMODUnity.RuntimeManager.GetBus("bus:/").setPaused(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Riprova()
    {
        SbloccaGiocoEAudio();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void TornaAlMenu()
    {
        SbloccaGiocoEAudio();
        SceneManager.LoadScene(0);
    }

    private void SbloccaGiocoEAudio()
    {
        GameOverAttivo = false;
        PauseMenu.GiocoInPausa = false;

        Time.timeScale = 1f;

        FMODUnity.RuntimeManager.GetBus("bus:/").setPaused(false);

        if (playerInputManager != null)
            playerInputManager.EnableAllControls();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}