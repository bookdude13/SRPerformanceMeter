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
        public static void LogGameObjectHierarchy(MelonLogger.Instance logger, Transform root, int indentLevel = 0)
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
                LogGameObjectHierarchy(logger, root.GetChild(i), indentLevel + 1);
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
                textComponent.SetText(text);
            }
        }

        /// <summary>
        /// Deletes all immediate children from parent whose names aren't in the given array.
        /// If a null array is given, all children are deleted.
        /// </summary>
        /// <param name="parent">Parent of deleted children</param>
        /// <param name="whitelistedNames">GameObject names of immediate children to not delete. If null, all are deleted.</param>
        public static void DeleteChildren(MelonLogger.Instance logger, Transform parent, string[] whitelistedNames = null)
        {
            if (parent == null)
            {
                return;
            }

            foreach (Transform child in parent)
            {
                if (whitelistedNames == null || !whitelistedNames.Contains(child.name))
                {
                    logger.Msg("Deleting child " + child.name);
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Set active status of all immediate children from parent whose names aren't in the given array.
        /// If a null array is given, all children are set.
        /// </summary>
        /// <param name="parent">Parent Transform</param>
        /// <param name="active">Whether children should be active or not</param>
        /// <param name="excludedNames">GameObject names of immediate children to not set. If null, all are set.</param>
        public static void SetChildrenActive(Transform parent, bool active, string[] excludedNames = null)
        {
            if (parent == null)
            {
                return;
            }

            foreach (Transform child in parent)
            {
                if (excludedNames == null || !excludedNames.Contains(child.name))
                {
                    child.gameObject.SetActive(active);
                }
            }
        }
    }
}
