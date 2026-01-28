#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Louis.XR.Interactions.Grab.Logic;
using UnityEditor;
using UnityEngine;

namespace Louis.XR.Interactions.Grab.Editor
{
    [CustomPropertyDrawer(typeof(XRSocketLogicBase), true)]
    public class XRSocketLogicDrawer : PropertyDrawer
    {
        private static Type[] _cachedTypes;
        private static string[] _cachedTypeNames;

        private void EnsureTypesCache()
        {
            if (_cachedTypes != null) return;

            var types = new List<Type>();
            foreach (var type in TypeCache.GetTypesDerivedFrom<XRSocketLogicBase>())
            {
                if (!type.IsAbstract && !type.IsGenericType && type.IsClass)
                    types.Add(type);
            }

            _cachedTypes = types.OrderBy(t => t.Name).ToArray();
            _cachedTypeNames = _cachedTypes.Select(t => ObjectNames.NicifyVariableName(t.Name)).ToArray();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            if (property.managedReferenceValue == null)
                return height + spacing;

            height += spacing + EditorGUIUtility.singleLineHeight;

            if (property.isExpanded)
            {
                SerializedProperty iterator = property.Copy();
                SerializedProperty end = iterator.GetEndProperty();
                bool enterChildren = true;

                while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
                {
                    height += EditorGUI.GetPropertyHeight(iterator, true) + spacing;
                    enterChildren = false;
                }
            }

            return height + spacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EnsureTypesCache();

            EditorGUI.BeginProperty(position, label, property);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            Rect currentRect = new Rect(position.x, position.y, position.width, lineHeight);

            string[] displayNames = new string[_cachedTypeNames.Length + 1];
            displayNames[0] = "None";
            Array.Copy(_cachedTypeNames, 0, displayNames, 1, _cachedTypeNames.Length);

            int currentIndex = GetCurrentTypeIndex(property);

            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUI.Popup(currentRect, label.text, currentIndex, displayNames);

            if (EditorGUI.EndChangeCheck())
            {
                if (newIndex == 0)
                {
                    property.managedReferenceValue = null;
                }
                else
                {
                    Type selectedType = _cachedTypes[newIndex - 1];
                    property.managedReferenceValue = Activator.CreateInstance(selectedType);
                    property.isExpanded = true;
                }
                property.serializedObject.ApplyModifiedProperties();
            }

            if (property.managedReferenceValue == null)
            {
                EditorGUI.EndProperty();
                return;
            }

            currentRect.y += lineHeight + spacing;
            currentRect.x += 15;
            currentRect.width -= 15;

            property.isExpanded = EditorGUI.Foldout(currentRect, property.isExpanded, "Settings", true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                SerializedProperty iterator = property.Copy();
                SerializedProperty end = iterator.GetEndProperty();
                bool enterChildren = true;

                currentRect.y += lineHeight + spacing;
                currentRect.x = position.x;
                currentRect.width = position.width;

                while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
                {
                    float propertyHeight = EditorGUI.GetPropertyHeight(iterator, true);
                    currentRect.height = propertyHeight;

                    EditorGUI.PropertyField(currentRect, iterator, true);

                    currentRect.y += propertyHeight + spacing;
                    enterChildren = false;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        private int GetCurrentTypeIndex(SerializedProperty property)
        {
            if (string.IsNullOrEmpty(property.managedReferenceFullTypename))
                return 0;

            string[] parts = property.managedReferenceFullTypename.Split(' ');
            if (parts.Length != 2)
                return 0;

            string assemblyName = parts[0];
            string typeName = parts[1];
            string fullTypeName = $"{typeName}, {assemblyName}";

            Type currentType = Type.GetType(fullTypeName);
            if (currentType == null)
                return 0;

            int index = Array.IndexOf(_cachedTypes, currentType);
            return index >= 0 ? index + 1 : 0;
        }
    }
}
#endif
