using UnityEditor;
using UnityEngine;
using Louis.XR.Interactions.Grab;

namespace Louis.XR.Interactions.Grab.Editor
{
    [CustomEditor(typeof(XRGenericInteractable)), CanEditMultipleObjects]
    public class XRGenericInteractableEditor : UnityEditor.Editor
    {
        // Tes champs custom
        private SerializedProperty _directLogicProp;
        private SerializedProperty _remoteLogicProp;
        private SerializedProperty _useHandleProp;

        // Champs à cacher de XRGrabInteractable
        private static readonly string[] _excludedProps =
        {
            "m_Script",
            "directLogic",
            "remoteLogic",
            "useHandle",                    // vérifie le nom exact en Debug mode si besoin
            "m_MovementType",               // Movement Type
            "m_AddDefaultGrabTransformers",
            "m_StartingSingleGrabTransformers",
            "m_StartingMultipleGrabTransformers",

            "m_OnFirstHoverEntered",
            "m_OnLastHoverExited",
            "m_OnHoverEntered",
            "m_OnHoverExited",
            "m_OnFirstSelectEntered",
            "m_OnLastSelectExited",
            "m_OnSelectEntered",
            "m_OnSelectExited",
            "m_OnSelectCanceled",
            "m_OnActivate",
            "m_OnDeactivate",

            "m_FirstFocusEntered",
            "m_FirstFocusExited",
            "m_FocusEntered",
            "m_FocusExited",
            "m_LastFocusExited",

            "m_FirstHoverEntered",
            "m_LastHoverExited",
            "m_HoverEntered",
            "m_HoverExited",
            "m_FirstSelectEntered",
            "m_LastSelectExited",
            "m_SelectEntered",
            "m_SelectExited",
            "m_SelectCanceled",
            "m_Activated",
            "m_Deactivated",

            "m_AttachEaseInTime",
            //"m_VelocityDamping",
            //"m_VelocityScale",
            //"m_AngularVelocityDamping",
            //"m_AngularVelocityScale",
            //"m_TrackPosition",
            //"m_SmoothPosition",
            //"m_SmoothPositionAmount",
            //"m_TightenPosition",
            //"m_TrackRotation",
            //"m_SmoothRotation",
            //"m_SmoothRotationAmount",
            //"m_TightenRotation",
            //"m_TrackScale",
            //"m_SmoothScale",
            //"m_SmoothScaleAmount",
            //"m_TightenScale",
            //"m_ThrowOnDetach",
            "m_ThrowSmoothingDuration",
            "m_ThrowSmoothingCurve",
            "m_ThrowVelocityScale",
            "m_ThrowAngularVelocityScale",
            "m_ForceGravityOnDetach",
            "m_RetainTransformParent",
        };

        private void OnEnable()
        {
            _directLogicProp = serializedObject.FindProperty("directLogic");
            _remoteLogicProp = serializedObject.FindProperty("remoteLogic");
            _useHandleProp = serializedObject.FindProperty("useHandle");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // -------- GRAB LOGIC (propre) --------
            EditorGUILayout.LabelField("Grab Logic", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            if (_directLogicProp != null)
                EditorGUILayout.PropertyField(_directLogicProp);
            if (_remoteLogicProp != null)
                EditorGUILayout.PropertyField(_remoteLogicProp);
            if (_useHandleProp != null)
                EditorGUILayout.PropertyField(_useHandleProp);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            // -------- XR GRAB (nettoyé) --------
            EditorGUILayout.LabelField("XR Grab Settings", EditorStyles.boldLabel);
            DrawPropertiesExcluding(serializedObject, _excludedProps);

            EditorGUILayout.HelpBox(
                "Le Movement Type est défini par ta Direct Logic (movementOverride). " +
                "Les paramètres Movement Type de XRGrabInteractable sont ignorés au grab.",
                MessageType.Info
            );

            serializedObject.ApplyModifiedProperties();
        }
    }
}
