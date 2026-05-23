using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private int inventorySize;
    private Item[] items;
    public int selectedIndex = 0;

    void Awake()
    {
        items = new Item[inventorySize];
    }

    public void AddItem(Item item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                items[i] = item;
                break;
            }
        }
    }

    public void AddItem(Item item, int index)
    {
        items[index] = item;
    }
    public void AddItemInHead(Item item)
    {
        // Adds item in first slot and shifts the other items to the right
        for (int i = inventorySize - 1; i > 0; i--)
        {
            items[i] = items[i - 1];
        }
        items[0] = item;
    }

    public Item GetItem(int index)
    {
        return items[index];
    }

    public void RemoveItem(int index)
    {
        items[index] = null;
    }

    public void RemoveItem(Item item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == item)
            {
                items[i] = null;
                break;
            }
        }
    }

    public void ClearInventory()
    {
        items = new Item[inventorySize];
    }

    public int GetItemIndex(Item item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == item)
            {
                return i;
            }
        }
        return -1;
    }

    public bool IsInventoryFull()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                return false;
            }
        }
        return true;
    }

    public bool IsInventoryEmpty()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
            {
                return false;
            }
        }
        return true;
    }

    public int GetInventorySize()
    {
        return inventorySize;
    }

    public Item[] GetItems()
    {
        return items;
    }

    public void SetItems(Item[] newItems)
    {
        items = newItems;
    }

    public void SetInventorySize(int newSize)
    {
        // Set inventory size without losing items
        inventorySize = newSize;
        Item[] newItems = new Item[inventorySize];
        for (int i = 0; i < inventorySize; i++)
        {
            newItems[i] = items[i];
        }
        items = newItems;
    }

    public void SetSelectedIndex(int index)
    {
        selectedIndex = index;
    }

    public int GetSelectedIndex()
    {
        return selectedIndex;
    }

    public void GoLeft()
    {
        if (selectedIndex > 0)
        {
            selectedIndex--;
        }
        else
        {
            selectedIndex = inventorySize - 1;
        }
    }

    public void GoRight()
    {
        if (selectedIndex < inventorySize - 1)
        {
            selectedIndex++;
        }
        else
        {
            selectedIndex = 0;
        }
    }

    
}
