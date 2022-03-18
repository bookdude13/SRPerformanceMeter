using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;

namespace PerformanceMeter
{
    public class UnityUtil
    {
        /// <summary>
        /// Retrieve the root(most parent) transform from the given transform.
        /// </summary>
        /// <param name="transform">Starting Transform in the hierarchy</param>
        /// <returns>Topmost transform in hierarchy. Returns null if transform is null</returns>
        public static Transform GetRootTransform(Transform transform)
        {
            return GetParentTransformByName(transform, null);
        }

        /// <summary>
        /// Go up the hierarchy until the parent with the given name is reached. Returns the topmost element if target not found.
        /// </summary>
        /// <param name="transform">Starting Transform in the hierarchy</param>
        /// <param name="targetName">Name of target parent Transform</param>
        /// <returns>Transform of target if found as a parent, else the root Transform of the hierarchy. Returns null if given transform is null</returns>
        public static Transform GetParentTransformByName(Transform transform, string targetName)
        {
            if (transform == null)
            {
                return null;
            }

            Transform currentTransform = transform;
            while (currentTransform.name != targetName && currentTransform.parent != null)
            {
                currentTransform = currentTransform.parent;
            }

            return currentTransform;
        }

        /**
         * Prints out the game object tree/hierarchy below the given root object to the logs.
         * Useful for finding objects to clone/instantiate :)
         */
        public static void LogGameObjectHierarchy(Transform root, int indentLevel = 0)
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
            MainMod.Log(string.Format(
                "{0}{1} at local position {2} (global position {3}) with rotation {4} (local rotation {5})",
                tabs,
                root.name,
                root.localPosition,
                root.position,
                root.rotation,
                root.localRotation
            ));

            // Children
            for (int i = 0; i < root.childCount; i++)
            {
                LogGameObjectHierarchy(root.GetChild(i), indentLevel + 1);
            }
        }

        public static void SetTMProText(Transform parent, string text)
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
