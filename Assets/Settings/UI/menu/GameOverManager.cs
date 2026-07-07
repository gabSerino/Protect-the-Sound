using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static bool GameOverAttivo { get; private set; } = false;

    [Header("Interfaccia UI")]
    [SerializeField] private GameObject gameOverMenuUI;

    [Header("References")]
    [SerializeField] private PlayerInputManager playerInputManager;

    public void AttivaGameOver()
    {
        if (GameOverAttivo) return;

        GameOverAttivo = true;
        PauseMenu.GiocoInPausa = false;

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