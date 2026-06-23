using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "Scriptable Objects/EnemyStats")]
public class EnemyStats : ScriptableObject
{
    [Header("Salute")]
    public float maxHealth = 20f;

    [Header("Movimento")]
    public float moveSpeed = 3.5f;
    public float chargeSpeed = 6.0f; // Velocità durante la carica
    public float chargeWindupTime = 0.5f; // Tempo di esitazione (es. mezzo secondo)
    public float acceleration = 8f;
    public float angularSpeed = 120f;

    [Header("Attacco")]
    public float damage = 10f;
    public float attackRange = 2.0f;
    public float attackCooldown = 1.0f;
    public float aggroRadius = 10f;

    [Header("Loot (Drop alla morte)")]
    [Range(0f, 1f)]
    public float dropChance = 0.5f;

    //riferimento alla LootTable
    [Tooltip("Trascina qui il file Loot Table per questo nemico")]
    public LootTable lootTable;
}