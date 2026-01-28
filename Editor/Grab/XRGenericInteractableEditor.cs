using UnityEditor;
using UnityEngine;

namespace Louis.XR.Interactions.Grab.Editor
{
    [CustomEditor(typeof(XRGenericInteractable)), CanEditMultipleObjects]
    public class XRGenericInteractableEditor : UnityEditor.Editor
    {
        // Properties
        private SerializedProperty _directLogicProp;
        private SerializedProperty _remoteLogicProp;
        private SerializedProperty _socketLogicProp;
        private SerializedProperty _useHandleProp;

        private SerializedProperty m_OnGrabPreset;
        private SerializedProperty m_OnReleasePreset;
        private SerializedProperty m_OnHoverPreset;

        // Foldout states
        private bool _showGrabLogic = true;
        private bool _showInteractionSettings = false;
        private bool _showColliders = false;
        private bool _showFilters = false;
        private bool _showAttachSettings = false;
        private bool _showMovementSettings = false;
        private bool _showThrowSettings = false;
        private bool _showEvents = false;

        // Excluded properties (hidden completely)
        private static readonly string[] ExcludedProperties =
        {
            "m_Script",
            "directLogic",
            "remoteLogic",
            "socketLogic",
            "useHandle",
            "m_MovementType",
            "m_AddDefaultGrabTransformers",
            "m_StartingSingleGrabTransformers",
            "m_StartingMultipleGrabTransformers"
        };

        private void OnEnable()
        {
            _directLogicProp = serializedObject.FindProperty("directLogic");
            _remoteLogicProp = serializedObject.FindProperty("remoteLogic");
            _socketLogicProp = serializedObject.FindProperty("socketLogic");
            _useHandleProp = serializedObject.FindProperty("useHandle");

            m_OnGrabPreset = serializedObject.FindProperty("onGrabPreset");
            m_OnReleasePreset = serializedObject.FindProperty("onReleasePreset");
            m_OnHoverPreset = serializedObject.FindProperty("onHoverPreset");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawHeader();
            EditorGUILayout.Space(5);

            DrawGrabLogicSection();
            DrawInteractionSettingsSection();
            DrawCollidersSection();
            DrawFiltersSection();
            DrawAttachSettingsSection();
            DrawMovementSettingsSection();
            DrawThrowSettingsSection();
            DrawEventsSection();

            EditorGUILayout.Space(5);
            DrawInfoBox();

            serializedObject.ApplyModifiedProperties();
        }

        private new void DrawHeader()
        {
            EditorGUILayout.LabelField("XR Generic Interactable", EditorStyles.boldLabel);
        }

        private void DrawGrabLogicSection()
        {
            _showGrabLogic = EditorGUILayout.Foldout(_showGrabLogic, "Grab Logic", true);
            if (_showGrabLogic)
            {
                EditorGUI.indentLevel++;
                DrawProperty(_directLogicProp, "Direct Logic");
                DrawProperty(_remoteLogicProp, "Remote Logic");
                DrawProperty(_socketLogicProp, "Socket Logic");
                EditorGUILayout.Space(2);
                DrawProperty(_useHandleProp, "Use Handle");
                EditorGUILayout.Space(2);
                DrawProperty(m_OnGrabPreset, "On Grab Preset");
                DrawProperty(m_OnReleasePreset, "On Release Preset");
                DrawProperty(m_OnHoverPreset, "On Hover Preset");
                EditorGUI.indentLevel--;
            }
        }

        private void DrawInteractionSettingsSection()
        {
            _showInteractionSettings = EditorGUILayout.Foldout(_showInteractionSettings, "Interaction Settings", true);
            if (_showInteractionSettings)
            {
                EditorGUI.indentLevel++;
                DrawProperty("m_InteractionManager");
                DrawProperty("m_InteractionLayerMask");
                DrawProperty("m_InteractionLayers");
                DrawProperty("m_DistanceCalculationMode");
                DrawProperty("m_SelectMode");
                DrawProperty("m_FocusMode");
                DrawProperty("m_CustomReticle");
                EditorGUI.indentLevel--;
            }
        }

        private void DrawCollidersSection()
        {
            _showColliders = EditorGUILayout.Foldout(_showColliders, "Colliders", true);
            if (_showColliders)
            {
                EditorGUI.indentLevel++;
                DrawProperty("m_Colliders");
                EditorGUI.indentLevel--;
            }
        }

        private void DrawFiltersSection()
        {
            _showFilters = EditorGUILayout.Foldout(_showFilters, "Filters", true);
            if (_showFilters)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.LabelField("Gaze", EditorStyles.miniBoldLabel);
                DrawProperty("m_AllowGazeInteraction");
                DrawProperty("m_AllowGazeSelect");
                DrawProperty("m_OverrideGazeTimeToSelect");
                DrawProperty("m_GazeTimeToSelect");
                DrawProperty("m_OverrideTimeToAutoDeselectGaze");
                DrawProperty("m_TimeToAutoDeselectGaze");
                DrawProperty("m_AllowGazeAssistance");
                
                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField("Hover Filters", EditorStyles.miniBoldLabel);
                DrawProperty("m_StartingHoverFilters");
                
                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField("Select Filters", EditorStyles.miniBoldLabel);
                DrawProperty("m_StartingSelectFilters");
                
                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField("Interaction Strength Filters", EditorStyles.miniBoldLabel);
                DrawProperty("m_StartingInteractionStrengthFilters");
                
                EditorGUI.indentLevel--;
            }
        }

        private void DrawAttachSettingsSection()
        {
            _showAttachSettings = EditorGUILayout.Foldout(_showAttachSettings, "Attach Settings", true);
            if (_showAttachSettings)
            {
                EditorGUI.indentLevel++;
                DrawProperty("m_AttachTransform");
                DrawProperty("m_SecondaryAttachTransform");
                DrawProperty("m_UseDynamicAttach");
                DrawProperty("m_MatchAttachPosition");
                DrawProperty("m_MatchAttachRotation");
                DrawProperty("m_SnapToColliderVolume");
                DrawProperty("m_ReinitializeDynamicAttachEverySingleGrab");
                DrawProperty("m_AttachPointCompatibilityMode");
                EditorGUI.indentLevel--;
            }
        }

        private void DrawMovementSettingsSection()
        {
            _showMovementSettings = EditorGUILayout.Foldout(_showMovementSettings, "Movement Settings", true);
            if (_showMovementSettings)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.HelpBox("Movement Type is overridden by Direct Logic (movementOverride)", MessageType.Info);
                
                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField("Velocity", EditorStyles.miniBoldLabel);
                DrawProperty("m_VelocityDamping");
                DrawProperty("m_VelocityScale");
                DrawProperty("m_AngularVelocityDamping");
                DrawProperty("m_AngularVelocityScale");
                
                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField("Position Tracking", EditorStyles.miniBoldLabel);
                DrawProperty("m_TrackPosition");
                DrawProperty("m_SmoothPosition");
                DrawProperty("m_SmoothPositionAmount");
                DrawProperty("m_TightenPosition");
                
                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField("Rotation Tracking", EditorStyles.miniBoldLabel);
                DrawProperty("m_TrackRotation");
                DrawProperty("m_SmoothRotation");
                DrawProperty("m_SmoothRotationAmount");
                DrawProperty("m_TightenRotation");
                
                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField("Scale Tracking", EditorStyles.miniBoldLabel);
                DrawProperty("m_TrackScale");
                DrawProperty("m_SmoothScale");
                DrawProperty("m_SmoothScaleAmount");
                DrawProperty("m_TightenScale");
                
                EditorGUI.indentLevel--;
            }
        }

        private void DrawThrowSettingsSection()
        {
            _showThrowSettings = EditorGUILayout.Foldout(_showThrowSettings, "Throw Settings", true);
            if (_showThrowSettings)
            {
                EditorGUI.indentLevel++;
                DrawProperty("m_ThrowOnDetach");
                DrawProperty("m_RetainTransformParent");
                DrawProperty("m_ForceGravityOnDetach");
                EditorGUI.indentLevel--;
            }
        }

        private void DrawEventsSection()
        {
            _showEvents = EditorGUILayout.Foldout(_showEvents, "Events", true);
            if (_showEvents)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.LabelField("Hover Events", EditorStyles.miniBoldLabel);
                DrawProperty("m_FirstHoverEntered");
                DrawProperty("m_LastHoverExited");
                DrawProperty("m_HoverEntered");
                DrawProperty("m_HoverExited");
                
                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField("Select Events", EditorStyles.miniBoldLabel);
                DrawProperty("m_FirstSelectEntered");
                DrawProperty("m_LastSelectExited");
                DrawProperty("m_SelectEntered");
                DrawProperty("m_SelectExited");
                DrawProperty("m_SelectCanceled");
                
                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField("Activate Events", EditorStyles.miniBoldLabel);
                DrawProperty("m_Activated");
                DrawProperty("m_Deactivated");
                
                EditorGUI.indentLevel--;
            }
        }

        private void DrawInfoBox()
        {
            EditorGUILayout.HelpBox(
                "This is a modular XR interaction system. Configure grab behaviors using Direct/Remote/Socket Logic.",
                MessageType.Info
            );
        }

        private void DrawProperty(SerializedProperty property, string label = null, string tooltip = null)
        {
            if (property == null) return;

            GUIContent content = string.IsNullOrEmpty(label)
                ? new GUIContent(property.displayName, tooltip)
                : new GUIContent(label, tooltip);

            EditorGUILayout.PropertyField(property, content);
        }

        private void DrawProperty(string propertyPath, string label = null, string tooltip = null)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyPath);
            DrawProperty(property, label, tooltip);
        }
    }
}
