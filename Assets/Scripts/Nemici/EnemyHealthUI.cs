using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour
{
    [Header("Riferimenti UI")]
    public Image[] heartImages; // Trascina qui i 4 cuori nell'ordine
    public Sprite fullHeart;
    public Sprite halfHeart;
    public Sprite emptyHeart;

    public void UpdateHearts(float currentHealth)
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            // Ogni cuore gestisce 1 punto vita
            // i=0 gestisce vita 0-1, i=1 gestisce vita 1-2, ecc.
            float threshold = i + 1;

            if (currentHealth >= threshold)
            {
                heartImages[i].sprite = fullHeart;
                heartImages[i].enabled = true;
            }
            else if (currentHealth >= threshold - 0.5f)
            {
                heartImages[i].sprite = halfHeart;
                heartImages[i].enabled = true;
            }
            else
            {
                // Cuore vuoto o rimosso
                heartImages[i].sprite = emptyHeart; 
                // heartImages[i].enabled = false; // Opzionale: nascondi se non vuoi vedere il vuoto
            }
        }
    }
}