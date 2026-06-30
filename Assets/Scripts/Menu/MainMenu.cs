using UnityEngine;
using UnityEngine.SceneManagement; // Fondamentale per cambiare scena!

public class MainMenu : MonoBehaviour
{
    // Questa è la funzione che collegheremo al tasto Start
    public void Gioca()
    {
        // Carica la scena successiva nella coda dei Build Settings
        // In alternativa puoi usare: SceneManager.LoadScene("NomeDellaTuaScena");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}