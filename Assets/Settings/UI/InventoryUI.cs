using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    // Invece di un array generico, colleghiamo i 3 slot fissi sullo schermo: [0] = Sinistra, [1] = Centro (Selezionato), [2] = Destra
    public Image[] slotIcons;          // Icone degli oggetti nei 3 quadratini
    public Image[] slotBackgrounds;    // Sfondi dei 3 quadratini (per gestire il colore selezionato/non selezionato)

    [Header("Face UI")]
    public Image faceDisplay;
    public Sprite happySprite;
    public Sprite sadSprite;

    [Header("Colors")]
    public Color selectedColor = Color.yellow;
    public Color unselectedColor = Color.white;

    public void RefreshUI(Inventory inventory, AttackType currentAttackType)
    {
        if (inventory == null) return;

        ItemData[] items = inventory.GetItems();
        int selectedIndex = inventory.GetSelectedIndex();
        int size = inventory.GetInventorySize();

        // Gestiamo i 3 slot fissi della UI: 
        // i = 0 -> Sinistra (precedente)
        // i = 1 -> Centro (selezionato)
        // i = 2 -> Destra (successivo)
        for (int i = 0; i < 3; i++)
        {
            int targetIndex = 0;

            if (i == 0)
            {
                // Slot di sinistra: elemento precedente
                targetIndex = (selectedIndex - 1 + size) % size;
            }
            else if (i == 1)
            {
                // Slot centrale: elemento attualmente selezionato
                targetIndex = selectedIndex;
            }
            else if (i == 2)
            {
                // Slot di destra: elemento successivo
                targetIndex = (selectedIndex + 1) % size;
            }

            // Assegnazione dell'icona se l'oggetto esiste ed è valido
            if (targetIndex < items.Length && items[targetIndex] != null)
            {
                slotIcons[i].sprite = items[targetIndex].icon;
                slotIcons[i].enabled = true;
            }
            else
            {
                slotIcons[i].sprite = null;
                slotIcons[i].enabled = false;
            }

            // Gestione dei colori/sfondi: Evidenziamo solo il blocco centrale (indice 1)
            if (i < slotBackgrounds.Length && slotBackgrounds[i] != null)
            {
                slotBackgrounds[i].color = (i == 1) ? selectedColor : unselectedColor;
            }
        }

        // 2. Logica della Faccina (inalterata)
        if (faceDisplay != null && happySprite != null && sadSprite != null)
        {
            if (currentAttackType == AttackType.DEFAULT)
            {
                faceDisplay.sprite = happySprite;
            }
            else
            {
                faceDisplay.sprite = sadSprite;
            }
        }
    }
}