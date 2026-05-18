using UnityEngine;

[CreateAssetMenu(fileName = "Drug", menuName = "Scriptable Objects/Drug")]
public class Drug : ScriptableObject
{
    public DrugType drugType;
    public string drugDisplayName;
    public string subtitle;
    public string description;
    public Sprite icon;

    public AttackType attackType;
    public float attackRateMultiplier;
    public float damageMultiplier;
    public bool damageOverTime;
    [ConditionalHide("damageOverTime")]
    public float damageDecayRate;
    [ConditionalHide("damageOverTime")]
    public AnimationCurve decayCurve;
    public float healthMultiplier;
    public float StaminaRecoverMultiplier;
    public float speedMultiplier;
    public float badTripChance;
}
