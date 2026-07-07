using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public static bool GiocoInPausa = false;

    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private PlayerInputManager playerInputManager;

    private void Start()
    {
        Time.timeScale = 1f;
        GiocoInPausa = false;

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (GiocoInPausa)
                Riprendi();
            else
                Pausa();
        }
    }

    public void Riprendi()
    {
        SetPausa(false);
    }

    public void Pausa()
    {
        SetPausa(true);
    }

    private void SetPausa(bool pausa)
    {
        GiocoInPausa = pausa;

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(pausa);
        else
            Debug.LogError("PauseMenuUI non assegnato nel PauseMenu.");

        Time.timeScale = pausa ? 0f : 1f;

        FMODUnity.RuntimeManager.GetBus("bus:/").setPaused(pausa);

        if (playerInputManager != null)
        {
            if (pausa)
                playerInputManager.DisableAllControls();
            else
                playerInputManager.EnableAllControls();
        }
        else
        {
            Debug.LogWarning("PlayerInputManager non assegnato nel PauseMenu.");
        }

        Cursor.lockState = pausa ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = pausa;
    }
}