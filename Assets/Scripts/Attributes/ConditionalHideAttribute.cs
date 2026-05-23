using UnityEngine;

public class ConditionalHideAttribute : PropertyAttribute
{
    public string conditionalSourceField;
    
    // Per bool
    public bool requiredBool;
    
    // Per enum/int
    public int requiredEnumValue;
    
    public enum ConditionType { Bool, Enum }
    public ConditionType conditionType;

    // Costruttore esistente (bool) — invariato
    public ConditionalHideAttribute(string conditionalSourceField, bool requiredValue = true)
    {
        this.conditionalSourceField = conditionalSourceField;
        this.requiredBool = requiredValue;
        this.conditionType = ConditionType.Bool;
    }

    // Nuovo costruttore per enum
    public ConditionalHideAttribute(string conditionalSourceField, int requiredEnumValue)
    {
        this.conditionalSourceField = conditionalSourceField;
        this.requiredEnumValue = requiredEnumValue;
        this.conditionType = ConditionType.Enum;
    }
}