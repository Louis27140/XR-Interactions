using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Louis.XR.Interactions.Grab
{

    public class XRAnchor : MonoBehaviour
    {
        public HandUsage handside = HandUsage.Both;

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
            // couleur de base selon la main
            Color c = handside switch
            {
                HandUsage.Left => Color.cyan,
                HandUsage.Right => Color.magenta,
                HandUsage.Both => Color.yellow,
                _ => Color.white
            };

            // --- position de l'anchor ---
            Gizmos.color = c;
            Gizmos.DrawSphere(transform.position, gizmoSphereRadius);

            // --- orientation (forward / up) ---
            // forward = direction "devant" de la main quand ça snap
            Gizmos.DrawLine(transform.position,
                            transform.position + transform.forward * gizmoForwardLength);

            // petite barre up pour voir le twist
            Gizmos.DrawLine(transform.position,
                            transform.position + transform.up * gizmoUpLength);

            // --- rayon de snap (debug, pour visualiser la zone où le grab peut s'ancrer) ---
            Gizmos.color = new Color(c.r, c.g, c.b, 0.2f);
            Gizmos.DrawWireSphere(transform.position, debugSnapRadius);
        }


    }

    public enum HandUsage
    {
        Left, Right, Both
    }

}
