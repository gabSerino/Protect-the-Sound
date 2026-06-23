using UnityEngine;

// Definiamo qui la struttura, così è accessibile in tutto il progetto
[System.Serializable]
public struct LootDrop
{
    public ItemData item;

    [Tooltip("Più il numero è alto, più l'oggetto è COMUNE. Es: Acqua=50, LSD=5")]
    public float weight;
}

[CreateAssetMenu(fileName = "NewLootTable", menuName = "Scriptable Objects/Loot Table")]
public class LootTable : ScriptableObject
{
    [Header("Lista Drop e Probabilità")]
    public LootDrop[] drops;
}