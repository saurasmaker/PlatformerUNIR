using UnityEngine;
using UnityEditor;

namespace CustomEditor
{
    // Define el atributo ReadOnly para el Inspector
    public class ReadOnlyAttribute : PropertyAttribute { }

    // Custom PropertyDrawer para el atributo ReadOnly
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false; // Hace que el campo sea de solo lectura
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true; // Restaura la habilitación de la GUI
        }
    }
}