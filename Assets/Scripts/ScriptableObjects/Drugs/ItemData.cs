using UnityEngine;

[CreateAssetMenu(fileName = "DrugData", menuName = "Scriptable Objects/DrugData")]
public class ItemData : ScriptableObject
{
    public ItemType itemType;

    [ConditionalHide("itemType", (int)ItemType.DRUG)]
    public DrugType drugType;
    public string displayName;
    public string subtitle;
    public string description;
    public Sprite icon;

    public AttackType attackType;
    public float attackRateMultiplier;
    public bool damageOverTime;
    [ConditionalHide("damageOverTime", true)]   // visibile solo se damageOverTime == true
    public float damageChangeTime;
    [ConditionalHide("damageOverTime", true)]
    public AnimationCurve damageCurve;

    [ConditionalHide("damageOverTime", false)]  // visibile solo se damageOverTime == false
    public float damageMultiplier;
    public float healthMultiplier;
    public float StaminaRecoverMultiplier;
    public float speedMultiplier;
    [ConditionalHide("itemType", (int)ItemType.DRUG)]
    public float badTripChance;
}
