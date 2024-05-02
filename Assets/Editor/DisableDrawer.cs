using UnityEditor;
using UnityEngine;

public class DisableDrawer : PropertyDrawer
{
    // https://forum.unity.com/threads/freebie-disableif-property-attribute.520802/
    // https://www.brechtos.com/hiding-or-disabling-inspector-properties-using-propertydrawers-within-unity-5/
    public override void OnGUI(Rect position, SerializedProperty p, GUIContent label)
    {
        bool oldState = GUI.enabled;

        GUI.enabled = false;
        EditorGUI.PropertyField(position, p, label, true);
        GUI.enabled = oldState;
    }
}
