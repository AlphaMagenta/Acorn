using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using System.Linq;

namespace Acorn {

    [CustomPropertyDrawer(typeof(MultiSelectEnum))]
    public class MultiSelectEnumProperty : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // property.intValue = EditorGUI.MaskField(position, label, property.intValue, property.enumNames);
            var attr = (attribute as MultiSelectEnum);
            // Unity hack to display enums correctly when they contain None
            var enumNames = property.enumNames.Skip(attr.skip).ToList();
            enumNames.Add("---");   // This is so that it doesn't switch to -1 when Everything is selected
            property.intValue = EditorGUI.MaskField(position, label, property.intValue, enumNames.ToArray());
        }

    }
}
