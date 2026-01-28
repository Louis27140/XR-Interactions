using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Louis.XR.Interactions.Utils.Anchors
{

    public class XRAnchor : MonoBehaviour
    {

        public AnchorRole role = AnchorRole.Any;
        public HandUsage handside = HandUsage.None;

        public float priority = 1f;

        public string anchorTag = "";

        [Header("Gizmos")]
        public float gizmoSphereRadius = 0.01f;
        public float gizmoForwardLength = 0.08f;
        public float gizmoUpLength = 0.05f;

        // si tu veux visualiser la distance de snap utilisée dans ta DirectAnchorLogic
        public float debugSnapRadius = 0.12f;

        private void OnDrawGizmos()
        {
            Color c = handside switch
            {
                HandUsage.None => Color.gray,
                HandUsage.Left => Color.cyan,
                HandUsage.Right => Color.magenta,
                HandUsage.Both => Color.yellow,
                _ => Color.white
            };

            Gizmos.color = c;
            Gizmos.DrawSphere(transform.position, gizmoSphereRadius);

            Gizmos.DrawLine(transform.position,
                            transform.position + transform.forward * gizmoForwardLength);

            // petite barre up pour voir le twist
            Gizmos.DrawLine(transform.position,
                            transform.position + transform.up * gizmoUpLength);

            Gizmos.color = new Color(c.r, c.g, c.b, 0.2f);
            Gizmos.DrawWireSphere(transform.position, debugSnapRadius);
        }


    }

    public enum HandUsage
    {
        None, Left, Right, Both
    }

    public enum AnchorRole
    {
        Grab,
        Inventory,
        SocketOverride,
        Any
    }
}
