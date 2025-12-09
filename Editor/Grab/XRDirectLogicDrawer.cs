#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Louis.XR.Interactions.Grab.Editor
{
    [CustomPropertyDrawer(typeof(XRDirectLogicBase), true)]
    public class XRDirectLogicDrawer : PropertyDrawer
    {
        private static Type[] _types;
        private static string[] _typeNames;

        private void EnsureTypes()
        {
            if (_types != null) return;

            var list = new List<Type>();

            foreach (var t in TypeCache.GetTypesDerivedFrom<XRDirectLogicBase>())
            {
                if (!t.IsAbstract && !t.IsGenericType && t.IsClass)
                    list.Add(t);
            }

            _types = list.ToArray();
            _typeNames = _types.Select(t => t.Name).ToArray();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineH = EditorGUIUtility.singleLineHeight;
            float v = EditorGUIUtility.standardVerticalSpacing;

            // 1) ligne du popup (toujours)
            float h = lineH;

            // Si pas d'instance -> juste popup + marge bas
            if (property.managedReferenceValue == null)
                return h + v; // <-- padding bas

            // 2) ligne du foldout
            h += v + lineH;

            // 3) enfants si expanded
            if (property.isExpanded)
            {
                var copy = property.Copy();
                var end = copy.GetEndProperty();
                bool first = true;

                while (copy.NextVisible(first) && !SerializedProperty.EqualContents(copy, end))
                {
                    float childH = EditorGUI.GetPropertyHeight(copy, true);
                    h += childH + v;
                    first = false;
                }
            }

            // petit espace sous le bloc pour éviter de coller la prop suivante
            return h + v * 2f;
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EnsureTypes();

            EditorGUI.BeginProperty(position, label, property);

            float lineH = EditorGUIUtility.singleLineHeight;
            float v = EditorGUIUtility.standardVerticalSpacing;

            // --- 1) Prépare la liste de noms avec "(None)" ----
            string[] displayNames;
            if (_typeNames != null && _typeNames.Length > 0)
            {
                displayNames = new string[_typeNames.Length + 1];
                displayNames[0] = "(None)";
                Array.Copy(_typeNames, 0, displayNames, 1, _typeNames.Length);
            }
            else
            {
                displayNames = new[] { "(None)" };
            }

            // --- 2) Popup type ---
            Rect popupRect = new Rect(
                position.x,
                position.y,
                position.width,
                lineH
            );

            int currentIndex = 0; // 0 = None

            if (!string.IsNullOrEmpty(property.managedReferenceFullTypename))
            {
                var parts = property.managedReferenceFullTypename.Split(' ');
                if (parts.Length == 2)
                {
                    string asmName = parts[0];
                    string typeName = parts[1];
                    string full = $"{typeName}, {asmName}";

                    var currentType = Type.GetType(full);
                    if (currentType != null && _types != null)
                    {
                        int idx = Array.IndexOf(_types, currentType);
                        if (idx >= 0)
                            currentIndex = idx + 1; // +1 car 0 = None
                    }
                }
            }

            int newIndex = EditorGUI.Popup(popupRect, label.text, currentIndex, displayNames);

            if (newIndex != currentIndex)
            {
                if (newIndex <= 0)
                {
                    property.managedReferenceValue = null;
                }
                else
                {
                    var newType = _types[newIndex - 1];
                    property.managedReferenceValue = Activator.CreateInstance(newType);
                    property.isExpanded = true; // petite QoL : auto-expand
                }
            }

            // Si pas d'instance -> pas de foldout / children
            if (property.managedReferenceValue == null)
            {
                EditorGUI.EndProperty();
                return;
            }

            // --- 3) Foldout line ---
            float foldY = popupRect.y + lineH + v;
            Rect foldRect = new Rect(
                position.x + 15,
                foldY,
                position.width - 15,
                lineH
            );

            property.isExpanded = EditorGUI.Foldout(foldRect, property.isExpanded, GUIContent.none);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                var child = property.Copy();
                var end = child.GetEndProperty();
                bool first = true;

                float y = foldY + lineH + v;

                while (child.NextVisible(first) && !SerializedProperty.EqualContents(child, end))
                {
                    float h = EditorGUI.GetPropertyHeight(child, true);
                    Rect r = new Rect(position.x, y, position.width, h);
                    EditorGUI.PropertyField(r, child, true);
                    y += h + v;
                    first = false;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }
    }
}
#endif
