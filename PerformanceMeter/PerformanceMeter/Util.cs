using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;

namespace PerformanceMeter
{
    public class Util
    {
        private MelonLogger.Instance logger;

        public Util(MelonLogger.Instance logger)
        {
            this.logger = logger;
        }

        /**
         * Retrieve the root (most parent) transform from the given transform.
         */
        public Transform GetRootTransform(Transform transform)
        {
            if (transform == null)
            {
                return null;
            }

            Transform currentTransform = transform;
            while (currentTransform.parent != null)
            {
                currentTransform = currentTransform.parent;
            }

            return currentTransform;
        }

        /**
         * Prints out the game object tree/hierarchy below the given root object to the logs.
         * Useful for finding objects to clone/instantiate :)
         */
        public void LogGameObjectHierarchy(Transform root, int indentLevel = 0)
        {
            if (root == null)
            {
                return;
            }

            string tabs = "";
            for (int i = 0; i < indentLevel; i++)
            {
                tabs += "\t";
            }

            // Root
            logger.Msg(string.Format(
                "{0}{1} at local position {2} (global position {3}) with rotation {4}",
                tabs,
                root.name,
                root.localPosition,
                root.position,
                root.rotation
            ));

            // Children
            for (int i = 0; i < root.childCount; i++)
            {
                LogGameObjectHierarchy(root.GetChild(i), indentLevel++);
            }
        }

        public void SetTMProText(Transform parent, string text)
        {
            if (parent == null)
            {
                return;
            }

            TMPro.TMP_Text textComponent = parent.GetComponent<TMPro.TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = text;
            }
        }
    }
}
