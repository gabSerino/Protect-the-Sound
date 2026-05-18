using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ConditionalHideAttribute))]
public class ConditionalHideDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (IsConditionMet(property))
            EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return IsConditionMet(property) ? EditorGUI.GetPropertyHeight(property, label) : 0f;
    }

    private bool IsConditionMet(SerializedProperty property)
    {
        string path = property.propertyPath.Replace(property.name, "");
        SerializedProperty condProp = property.serializedObject.FindProperty(
            path + ((ConditionalHideAttribute)attribute).conditionField
        );
        return condProp != null && condProp.boolValue;
    }
}