using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static bool GiocoInPausa = false;
    public GameObject pauseMenuUI;

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
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GiocoInPausa = false;

        // Riprende tutto l'audio dal Master Bus di FMOD
        FMODUnity.RuntimeManager.GetBus("bus:/").setPaused(false);
    }

    void Pausa()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GiocoInPausa = true;

        // Mette in pausa tutto l'audio dal Master Bus di FMOD
        FMODUnity.RuntimeManager.GetBus("bus:/").setPaused(true);
    }
}