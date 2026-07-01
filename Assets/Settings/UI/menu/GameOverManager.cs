using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Interfaccia UI")]
    public GameObject gameOverMenuUI;

    // Questa funzione verrà chiamata dallo script SharedHealth quando la vita arriva a 0
    public void AttivaGameOver()
    {
        gameOverMenuUI.SetActive(true); // Mostra il pannello rosso/nero
        Time.timeScale = 0f;            // Blocca il gioco

        // Mette in pausa la musica di FMOD
        FMODUnity.RuntimeManager.GetBus("bus:/").setPaused(true);
    }

    // Funzione per il tasto "Riprova"
    public void Riprova()
    {
        SbloccaGiocoEAudio();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Funzione per il tasto "Menu Principale"
    public void TornaAlMenu()
    {
        SbloccaGiocoEAudio();
        SceneManager.LoadScene(0);
    }

    // Una piccola funzione di supporto per non ripetere il codice
    private void SbloccaGiocoEAudio()
    {
        Time.timeScale = 1f; // Rimette il tempo a velocità normale
        FMODUnity.RuntimeManager.GetBus("bus:/").setPaused(false); // Sblocca l'audio
    }
}