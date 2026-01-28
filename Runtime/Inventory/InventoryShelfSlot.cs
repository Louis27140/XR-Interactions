using Louis.Core.Inventory;
using Louis.XR.Interactions.Grab;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Louis.XR.Interactions.Inventory
{
    public class InventoryShelfSlot : MonoBehaviour
    {
        [SerializeField] private XRSocketInteractor socket;
        [SerializeField] private Transform snapPoint;
        [SerializeField] private InventoryBehaviour inventoryBehaviour;

        [SerializeField] private TMP_Text itemName;

        private InventoryItem currentItem;

        public bool IsEmpty => currentItem == null;

        private void Reset()
        {
            if (!socket) socket = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
        }

        private void OnEnable()
        {
            if (!socket) return;
            socket.selectEntered.AddListener(OnSelectEntered);
            socket.selectExited.AddListener(OnSelectExited);
        }

        private void OnDisable()
        {
            if (!socket) return;
            socket.selectEntered.RemoveListener(OnSelectEntered);
            socket.selectExited.RemoveListener(OnSelectExited);
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            var incomingItem = args.interactableObject.transform.GetComponentInParent<InventoryItem>();
            if (currentItem == null)
            {
                PlaceItem(incomingItem);
                return;
            }

            var emptySlot = FindEmptySlot();
            if (emptySlot != null)
            {
                StartCoroutine(MoveToEmptySlot(args, emptySlot));
            } else
            {
                StartCoroutine(RejectItem(args));
            }

        }

        private void OnSelectExited(SelectExitEventArgs args)
        {
            if (currentItem == null) return;

            inventoryBehaviour?.Inventory?.TryRemove(currentItem.definition, 1);
            currentItem = null;

            itemName.gameObject.SetActive(false);
        }

        private void PlaceItem(InventoryItem item)
        {
            currentItem = item;
            itemName.gameObject.SetActive(true);
            itemName.text = currentItem.definition.displayName;
            inventoryBehaviour?.Inventory?.TryAdd(currentItem.definition, 1);
        }

        private IEnumerator MoveToEmptySlot(SelectEnterEventArgs args, InventoryShelfSlot targetSlot)
        {
            // Annuler la s�lection actuelle
            socket.interactionManager.SelectExit(socket, args.interactableObject);
            yield return null;

            // Placer dans le slot vide
            targetSlot.socket.interactionManager.SelectEnter(targetSlot.socket, args.interactableObject);
        }

        private IEnumerator RejectItem(SelectEnterEventArgs args)
        {
            yield return null;
            socket.interactionManager.SelectExit(socket, args.interactableObject);
        }

        private InventoryShelfSlot FindEmptySlot()
        {
            if(inventoryBehaviour == null) return null;

            var allSlots = inventoryBehaviour.GetComponentsInChildren<InventoryShelfSlot>();

            return allSlots.FirstOrDefault(slot => slot != this && slot.IsEmpty);
        }

        public void SetActive(bool active)
        {
            if (currentItem != null)
            {
                currentItem.GetComponentsInChildren<MeshRenderer>().ToList().ForEach(renderer => renderer.enabled = active);

                XRGenericInteractable xrInteractable = currentItem.GetComponent<XRGenericInteractable>();
                if (xrInteractable != null)
                {
                    xrInteractable.AllowGrab = active;
                }
            }
        }
    }
}
