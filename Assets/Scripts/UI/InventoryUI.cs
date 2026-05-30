using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    public Image[] itemIcons;
    public Image[] slotBackgrounds;

    [Header("Face UI")]
    public Image faceDisplay;      // Trascina qui l'oggetto "PlayerFace"
    public Sprite happySprite;     // Trascina qui lo sprite Felice
    public Sprite sadSprite;       // Trascina qui lo sprite Triste

    [Header("Colors")]
    public Color selectedColor = Color.yellow;
    public Color unselectedColor = Color.white;

    // Aggiungiamo un parametro per ricevere l'attackType dal Player
    public void RefreshUI(Inventory inventory, AttackType currentAttackType)
    {
        if (inventory == null) return;

        // 1. Logica Inventario (quella che abbiamo già)
        ItemData[] items = inventory.GetItems();
        int selectedIndex = inventory.GetSelectedIndex();

        for (int i = 0; i < itemIcons.Length; i++)
        {
            if (i < items.Length && items[i] != null)
            {
                itemIcons[i].sprite = items[i].icon;
                itemIcons[i].enabled = true;
            }
            else
            {
                itemIcons[i].sprite = null;
                itemIcons[i].enabled = false;
            }

            if (i < slotBackgrounds.Length && slotBackgrounds[i] != null)
            {
                slotBackgrounds[i].color = (i == selectedIndex) ? selectedColor : unselectedColor;
            }
        }

        // 2. NUOVA LOGICA DELLA FACCINA
        if (faceDisplay != null && happySprite != null && sadSprite != null)
        {
            // Se l'attacco è DEFAULT, faccina felice. Altrimenti, triste.
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