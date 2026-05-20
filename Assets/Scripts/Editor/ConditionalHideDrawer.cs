using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ConditionalHideAttribute))]
public class ConditionalHideDrawer : PropertyDrawer
{
    private bool IsConditionMet(SerializedProperty property, ConditionalHideAttribute attr)
    {
        string path = property.propertyPath.Contains(".")
            ? property.propertyPath[..property.propertyPath.LastIndexOf('.')] + "." + attr.conditionalSourceField
            : attr.conditionalSourceField;

        SerializedProperty sourceField = property.serializedObject.FindProperty(path);
        if (sourceField == null || sourceField.propertyType != SerializedPropertyType.Boolean)
            return true;

        return sourceField.boolValue == attr.requiredValue;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (IsConditionMet(property, (ConditionalHideAttribute)attribute))
            EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (IsConditionMet(property, (ConditionalHideAttribute)attribute))
            return EditorGUI.GetPropertyHeight(property, label);

        return -EditorGUIUtility.standardVerticalSpacing; // collassa lo spazio
    }
}