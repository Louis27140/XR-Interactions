using Louis.Core.Keyboard;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class XRKeyboardManager : MonoBehaviour
{
    [Header("XR Keyboard Manager")]
    [SerializeField] private Camera xrCamera;
    [SerializeField] private GameObject keyboardPrefab;
    [SerializeField] private KeyboardController keyboardController;

    [SerializeField] private XRUIInputModule inputModule;

    [SerializeField] private float distance = 0.5f;
    [SerializeField] private Vector2 offset = new Vector2(0, -0.2f);

    private GameObject keyboardInstance;

    private TMP_InputField currentInputField;

    private void Awake()
    {
        if (xrCamera == null)
        {
            xrCamera = Camera.main;
        }

        if (inputModule == null)
            inputModule = FindObjectOfType<XRUIInputModule>();


        inputModule.pointerDown += PointerDown;

    }

    private void OnTextChanged(string text)
    {
        if (currentInputField == null)
            return;

        currentInputField.text = text;

    }

    private void PointerDown(GameObject element, PointerEventData data)
    {
        if (element == null)
        {
            currentInputField = null;
            keyboardInstance?.SetActive(false);
            return;
        }

        if (keyboardInstance != null && element.transform.IsChildOf(keyboardInstance.transform))
            return;

        var field = element.GetComponentInParent<TMP_InputField>();
        if (field != null)
        {
            currentInputField = field;

            if (keyboardInstance == null || !keyboardInstance.activeSelf)
            {
                OpenKeyboard();
            }
            else
            {
                keyboardController.CurrentText = currentInputField.text;
            }
        }
    }

    public void OpenKeyboard()
    {
        if (keyboardInstance == null)
        {
            keyboardInstance = Instantiate(keyboardPrefab, transform);

            keyboardController = keyboardInstance.GetComponent<KeyboardController>();
            keyboardController.OnTextChanged += OnTextChanged;
        }

        keyboardController.CurrentText = currentInputField.text;

        PositionKeyboard();
        keyboardInstance.SetActive(true);
    }

    public void CloseKeyboard()
    {
        if (keyboardInstance != null)
            keyboardInstance.SetActive(false);
    }

    private void PositionKeyboard()
    {
        if (xrCamera == null || keyboardInstance == null)
            return;
        
        var cam = xrCamera.transform;

        var pos = cam.position + cam.forward * distance + cam.up * offset.x + cam.right * offset.y;

        keyboardInstance.transform.position = pos;
        keyboardInstance.transform.rotation = Quaternion.LookRotation(pos - cam.position);
    }
}
