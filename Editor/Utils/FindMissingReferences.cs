using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Louis.Editor.Utils
{
    /// <summary>
    /// Utilitaire pour trouver les références null/manquantes dans la scène
    /// </summary>
    public static class FindMissingReferences
    {
        [MenuItem("Tools/Louis/Find Missing References in Scene")]
        public static void FindMissingReferencesInScene()
        {
            var gameObjects = GameObject.FindObjectsOfType<GameObject>(true);
            int missingCount = 0;
            int nullRefCount = 0;

            foreach (var go in gameObjects)
            {
                var components = go.GetComponents<Component>();
                foreach (var component in components)
                {
                    if (component == null)
                    {
                        Debug.LogError($"[Script Manquant] GameObject: {GetFullPath(go)}", go);
                        missingCount++;
                        continue;
                    }

                    SerializedObject so = new SerializedObject(component);
                    SerializedProperty sp = so.GetIterator();

                    while (sp.NextVisible(true))
                    {
                        if (sp.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            if (sp.objectReferenceValue == null
                                && sp.objectReferenceInstanceIDValue != 0)
                            {
                                Debug.LogWarning($"[Référence Null] {GetFullPath(go)} > {component.GetType().Name}.{sp.name}", go);
                                nullRefCount++;
                            }
                        }
                    }
                }
            }

            if (missingCount == 0 && nullRefCount == 0)
            {
                Debug.Log("<color=green> Aucune référence manquante trouvée dans la scène!</color>");
            }
            else
            {
                Debug.Log($"<color=yellow>Analyse terminée: {missingCount} scripts manquants, {nullRefCount} références null</color>");
            }
        }

        [MenuItem("Tools/Louis/Find Missing References in Selected")]
        public static void FindMissingReferencesInSelected()
        {
            if (Selection.gameObjects.Length == 0)
            {
                Debug.LogWarning("Aucun GameObject sélectionné");
                return;
            }

            int missingCount = 0;

            foreach (var go in Selection.gameObjects)
            {
                var components = go.GetComponents<Component>();
                foreach (var component in components)
                {
                    if (component == null)
                    {
                        Debug.LogError($"[Script Manquant] {GetFullPath(go)}", go);
                        missingCount++;
                    }
                }
            }

            Debug.Log(missingCount == 0
                ? "<color=green> Aucun script manquant sur la sélection</color>"
                : $"<color=red>{missingCount} scripts manquants trouvés</color>");
        }

        private static string GetFullPath(GameObject go)
        {
            string path = go.name;
            Transform parent = go.transform.parent;

            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }
    }
}