using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field)]
public class ConditionalHideAttribute : PropertyAttribute
{
    public string conditionField;
    public ConditionalHideAttribute(string conditionField)
    {
        this.conditionField = conditionField;
    }
}