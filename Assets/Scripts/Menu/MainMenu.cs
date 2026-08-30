using UnityEngine;
using UnityEngine.SceneManagement; // Fondamentale per cambiare scena!

public class MainMenu : MonoBehaviour
{
    // Questa è la funzione che collegheremo al tasto Start
    public void Gioca()
    {
        // Carica la scena successiva nella coda dei Build Settings
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // Questa è la nuova funzione da collegare al tasto Esci
    public void ChiudiGioco()
    {
        Debug.Log("Il gioco si sta chiudendo...");

        // Chiude l'applicazione quando il gioco è esportato (Build)
        Application.Quit();

        // Ferma il Play Mode nell'Editor di Unity
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}