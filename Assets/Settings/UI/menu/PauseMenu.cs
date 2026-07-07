using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static bool GiocoInPausa = false;
    public GameObject pauseMenuUI;
    public PlayerInputManager playerInputManager;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GiocoInPausa)
            {
                Riprendi();
            }
            else
            {
                Pausa();
            }
        }
    }

    public void Riprendi()
    {
        if(GiocoInPausa == false) return;
        Time.timeScale = 1f;
        GiocoInPausa = false;

        // Riprende tutto l'audio dal Master Bus di FMOD
        FMODUnity.RuntimeManager.GetBus("bus:/").setPaused(false);
        playerInputManager.EnableAllControls();
        pauseMenuUI.SetActive(false);
    }

    void Pausa()
    {
        if(GiocoInPausa == true) return;
        pauseMenuUI.SetActive(true);
        playerInputManager.DisableAllControls();
        Time.timeScale = 0f;
        GiocoInPausa = true;

        // Mette in pausa tutto l'audio dal Master Bus di FMOD
        FMODUnity.RuntimeManager.GetBus("bus:/").setPaused(true);
    }
}