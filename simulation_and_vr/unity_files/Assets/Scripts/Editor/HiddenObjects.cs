using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Editor
{
    public static class HiddenObjects
    {
        [MenuItem("Tools/Objects/Make hidden visible")]
        public static void MakeHiddenObjectsVisible()
        {
            var gos = GameObject.FindObjectsOfType<GameObject>();
            foreach (var go in gos)
            {
                if (go.hideFlags.HasFlag(HideFlags.HideInHierarchy) || go.hideFlags.HasFlag(HideFlags.HideInInspector))
                {
                    go.hideFlags &= ~(HideFlags.HideInHierarchy | HideFlags.HideInInspector);
                }
            }
        }

        [MenuItem("Tools/Objects/Make selection invisible")]
        public static void MakeSelectionInvisible()
        {
            var gos = Selection.gameObjects;
            foreach (var go in gos)
            {
                go.hideFlags |= HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            }
        }

        [MenuItem("Tools/Objects/Make selection editable")]
        public static void MakeSelectionEditable()
        {
            var gos = Selection.gameObjects;
            foreach (var go in gos)
            {
                go.hideFlags &= ~(HideFlags.NotEditable);
            }
        }

        [MenuItem("Tools/Objects/Make selection non editable")]
        public static void MakeSelectionNonEditable()
        {
            var gos = Selection.gameObjects;
            foreach (var go in gos)
            {
                go.hideFlags |= HideFlags.NotEditable;
            }
        }
    }
}
