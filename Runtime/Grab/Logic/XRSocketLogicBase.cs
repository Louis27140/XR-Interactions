using System;
using UnityEngine;

namespace Louis.XR.Interactions.Grab.Logic
{
    [Serializable]
    public class XRSocketLogicBase
    {
        /// <summary>
        /// Appel� quand un socket commence � s�lectionner l'objet
        /// </summary>
        /// <returns>True si le socket peut prendre l'objet</returns>
        public virtual bool OnSelectEntering(XRContext ctx) => true;

        /// <summary>
        /// Appel� quand le socket a s�lectionn� l'objet
        /// </summary>
        public virtual void OnSelectEntered(XRContext ctx){}

        /// <summary>
        /// Appel� quand le socket commence � rel�cher l'objet
        /// </summary>
        public virtual void OnSelectExiting(XRContext ctx){}

        /// <summary>
        /// Appel� quand le socket a rel�ch� l'objet
        /// </summary>
        public virtual void OnSelectExited(XRContext ctx){}

        /// <summary>
        /// Appel� chaque frame pendant que l'objet est dans le socket
        /// </summary>
        public virtual void Process(XRContext ctx){}
    }
}