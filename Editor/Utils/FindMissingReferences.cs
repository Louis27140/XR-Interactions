using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Louis.XR.Interactions.Editor.Utils
{
    /// <summary>
    /// Utilitaire pour trouver les r�f�rences null/manquantes dans la sc�ne
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
                                Debug.LogWarning($"[R�f�rence Null] {GetFullPath(go)} > {component.GetType().Name}.{sp.name}", go);
                                nullRefCount++;
                            }
                        }
                    }
                }
            }

            if (missingCount == 0 && nullRefCount == 0)
            {
                Debug.Log("<color=green> Aucune r�f�rence manquante trouv�e dans la sc�ne!</color>");
            }
            else
            {
                Debug.Log($"<color=yellow>Analyse termin�e: {missingCount} scripts manquants, {nullRefCount} r�f�rences null</color>");
            }
        }

        [MenuItem("Tools/Louis/Find Missing References in Selected")]
        public static void FindMissingReferencesInSelected()
        {
            if (Selection.gameObjects.Length == 0)
            {
                Debug.LogWarning("Aucun GameObject s�lectionn�");
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
                ? "<color=green> Aucun script manquant sur la s�lection</color>"
                : $"<color=red>{missingCount} scripts manquants trouv�s</color>");
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