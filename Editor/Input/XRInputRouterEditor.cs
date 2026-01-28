#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Louis.XR.Interactions.Input;

namespace Louis.XR.Interactions.Input.Editor
{
    [CustomEditor(typeof(XRInputRouter))]
    public class XRInputRouterEditor : UnityEditor.Editor
    {
        private bool showActiveInputs = true;
        private bool showLoadedActions = false;
        private Vector2 scrollPos;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Debug info available only in Play Mode", MessageType.Info);
                return;
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Runtime Debug Info", EditorStyles.boldLabel);
            
            // Boutons d'actions
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Dump to Console", GUILayout.Height(30)))
            {
                XRInputRouter.DumpDebugInfo();
            }
            if (GUILayout.Button("Clear Context Stack", GUILayout.Height(30)))
            {
                XRInputRouter.ClearContextStack();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Stats rapides
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Active Inputs:", XRInputRouter.GetActiveInputCount().ToString(), EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
            
            // Active Inputs
            showActiveInputs = EditorGUILayout.Foldout(showActiveInputs, "Active InputDefinitions", true);
            if (showActiveInputs)
            {
                EditorGUI.indentLevel++;
                var activeInputs = XRInputRouter.GetActiveInputNames();
                
                if (activeInputs.Count == 0)
                {
                    EditorGUILayout.LabelField("No active inputs", EditorStyles.miniLabel);
                }
                else
                {
                    scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.MaxHeight(200));
                    foreach (var inputName in activeInputs)
                    {
                        EditorGUILayout.LabelField("• " + inputName, EditorStyles.miniLabel);
                    }
                    EditorGUILayout.EndScrollView();
                }
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(5);
            
            // Test d'actions
            EditorGUILayout.LabelField("Test Actions", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Test 'Move' (Left)"))
            {
                var router = (XRInputRouter)target;
                router.TestAction("Move", XRHandSide.Left);
            }
            if (GUILayout.Button("Test 'Primary Button' (Right)"))
            {
                var router = (XRInputRouter)target;
                router.TestAction("Primary Button", XRHandSide.Right);
            }
            EditorGUILayout.EndHorizontal();
            
            // Auto-refresh en play mode
            if (Application.isPlaying)
            {
                Repaint();
            }
        }
    }
}
#endif
