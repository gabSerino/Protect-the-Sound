using UnityEngine;

public class ConditionalHideAttribute : PropertyAttribute
{
    public string conditionalSourceField;
    public bool requiredValue;

    public ConditionalHideAttribute(string conditionalSourceField, bool requiredValue = true)
    {
        this.conditionalSourceField = conditionalSourceField;
        this.requiredValue = requiredValue;
    }
}