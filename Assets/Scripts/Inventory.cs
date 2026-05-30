using UnityEngine;

public class Inventory 
{
    private int inventorySize;
    private ItemData[] items;
    private int selectedIndex = 0;

    public Inventory(int size)
    {
        inventorySize = size;
        items = new ItemData[inventorySize];
    }


    public void AddItem(ItemData item)
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

    public void AddItem(ItemData item, int index)
    {
        items[index] = item;
    }
    public void AddItemInHead(ItemData item)
    {
        // Adds item in first slot and shifts the other items to the right
        for (int i = inventorySize - 1; i > 0; i--)
        {
            items[i] = items[i - 1];
        }
        items[0] = item;
    }

    public ItemData GetItem(int index)
    {
        return items[index];
    }

    public void RemoveItem(int index)
    {
        items[index] = null;
    }

    public void RemoveItem(ItemData item)
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
        items = new ItemData[inventorySize];
    }

    public int GetItemIndex(ItemData item)
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

    public ItemData[] GetItems()
    {
        return items;
    }

    public void SetItems(ItemData[] newItems)
    {
        items = newItems;
    }

    public void SetInventorySize(int newSize)
    {
        // Set inventory size without losing items
        inventorySize = newSize;
        ItemData[] newItems = new ItemData[inventorySize];
        for (int i = 0; i < inventorySize; i++)
        {
            newItems[i] = items[i];
        }
        items = newItems;
    }

    public void SetSelectedIndex(int index)
    {
        selectedIndex = index;
        Debug.Log("Selected index: " + (selectedIndex + 1));
    }

    public int GetSelectedIndex()
    {
        return selectedIndex;
    }

    public ItemData GetSelectedItem()
    {
        return items[selectedIndex];
    }

    public void SortItems()
    {
        //If there are null spaces, they go to the last slots
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                for (int j = i + 1; j < items.Length; j++)
                {
                    if (items[j] != null)
                    {
                        items[i] = items[j];
                        items[j] = null;
                        break;
                    }
                }
            }
        }
    }
}
