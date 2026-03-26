using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Scripts.Editor
{
    public static class SelectionUtil
    {
        [NotNull]
        private static readonly List<GameObject> goCache = new List<GameObject>();

        [NotNull]
        private static readonly List<Component> compCache = new List<Component>();

        private static readonly HashSet<GameObject> goLookup = new HashSet<GameObject>();

        #region Selection

        [MenuItem("Tools/Select/All", priority = 50)]
        public static void SelectAll()
        {
            SelectByMatchingComponents(comps => true);
        }

        [MenuItem("Tools/Select/Empty", priority = 51)]
        public static void SelectEmpty()
        {
            SelectByMatchingComponents(comps => comps.Count == 0 || (comps.Count == 1 && comps[0] is Transform));
        }

        [MenuItem("Tools/Select/Non Empty", priority = 52)]
        public static void SelectNonEmpty()
        {
            SelectByMatchingComponents(comps => comps.Count > 1);
        }

        [MenuItem("Tools/Select/Invert Selection", priority = 53)]
        public static void InvertSelection()
        {
            goLookup.Clear();
            goCache.Clear();

            var selectedGos = Selection.gameObjects;
            var allGos = Object.FindObjectsOfType<GameObject>();

            foreach (var go in selectedGos)
            {
                goLookup.Add(go);
            }

            foreach (var go in allGos)
            {
                if (!goLookup.Contains(go))
                {
                    goCache.Add(go);
                    goLookup.Add(go);
                }
            }

            SetSelection(goCache);
            
            goLookup.Clear();
            goCache.Clear();
        } 

        #endregion

        #region Render selection

        [MenuItem("Tools/Select/Renderer/Any", priority = 100)]
        public static void SelectRenderer()
        {
            SelectByMatchingComponents(comps => comps.Count > 1 && comps.Any(c => c is Renderer));
        }

        [MenuItem("Tools/Select/Renderer/Mesh Renderer", priority = 101)]
        public static void SelectMeshRenderer()
        {
            SelectByMatchingComponents(comps => comps.Count > 1 && comps.Any(c => c is MeshRenderer));
        }

        [MenuItem("Tools/Select/Renderer/Skinned Mesh Renderer", priority = 102)]
        public static void SelectSkinnedMeshRenderer()
        {
            SelectByMatchingComponents(comps => comps.Count > 1 && comps.Any(c => c is SkinnedMeshRenderer));
        }

        #endregion

        #region Collider selection

        [MenuItem("Tools/Select/Collider/Any", priority = 110)]
        public static void SelectCollider()
        {
            SelectByMatchingComponents(comps => comps.Count > 1 && comps.Any(c => c is Collider));
        }

        [MenuItem("Tools/Select/Collider/Box", priority = 111)]
        public static void SelectBoxCollider()
        {
            SelectByMatchingComponents(comps => comps.Count > 1 && comps.Any(c => c is BoxCollider));
        }

        [MenuItem("Tools/Select/Collider/Sphere", priority = 112)]
        public static void SelectSphereCollider()
        {
            SelectByMatchingComponents(comps => comps.Count > 1 && comps.Any(c => c is SphereCollider));
        }

        [MenuItem("Tools/Select/Collider/Capsule", priority = 113)]
        public static void SelectCapsuleCollider()
        {
            SelectByMatchingComponents(comps => comps.Count > 1 && comps.Any(c => c is CapsuleCollider));
        }

        [MenuItem("Tools/Select/Collider/Terrain", priority = 114)]
        public static void SelectTerrainCollider()
        {
            SelectByMatchingComponents(comps => comps.Count > 1 && comps.Any(c => c is TerrainCollider));
        }

        [MenuItem("Tools/Select/Collider/Mesh", priority = 115)]
        public static void SelectMeshCollider()
        {
            SelectByMatchingComponents(comps => comps.Count > 1 && comps.Any(c => c is MeshCollider));
        }

        [MenuItem("Tools/Select/Collider/Convex Mesh", priority = 116)]
        public static void SelectConvexMeshCollider()
        {
            SelectByMatchingComponents(comps => comps.Count > 1 && comps.Any(c => c is MeshCollider m && m.convex));
        } 

        [MenuItem("Tools/Select/Collider/Concave Mesh", priority = 117)]
        public static void SelectConcaveMeshCollider()
        {
            SelectByMatchingComponents(comps => comps.Count > 1 && comps.Any(c => c is MeshCollider m && m.convex));
        }

        #endregion

        #region Light selection

        [MenuItem("Tools/Select/Lights/Any", priority = 120)]
        public static void SelectLight()
        {
            SelectByMatchingComponents(comps => comps.Count > 1 && comps.Any(c => c is Light));
        }

        [MenuItem("Tools/Select/Lights/Point", priority = 121)]
        public static void SelectPointLight()
        {
            SelectByMatchingComponents(comps => comps.Count > 1 && comps.Any(c => c is Light l && l.type == LightType.Point));
        }

        [MenuItem("Tools/Select/Lights/Spot", priority = 122)]
        public static void SelectSpotLight()
        {
            SelectByMatchingComponents(comps => comps.Count > 1 && comps.Any(c => c is Light l && l.type == LightType.Spot));
        }

        [MenuItem("Tools/Select/Lights/Directional", priority = 123)]
        public static void SelectDirectionalLight()
        {
            SelectByMatchingComponents(comps => comps.Count > 1 && comps.Any(c => c is Light l && l.type == LightType.Directional));
        }

        [MenuItem("Tools/Select/Lights/Area", priority = 124)]
        public static void SelectAreaLight()
        {
            SelectByMatchingComponents(comps => comps.Count > 1 && comps.Any(c => c is Light l && l.type == LightType.Area));
        }

        [MenuItem("Tools/Select/Lights/Rectangle", priority = 125)]
        public static void SelectRectangleLight()
        {
            SelectByMatchingComponents(comps => comps.Count > 1 && comps.Any(c => c is Light l && l.type == LightType.Rectangle));
        }

        [MenuItem("Tools/Select/Lights/Disc", priority = 126)]
        public static void SelectDiscLight()
        {
            SelectByMatchingComponents(comps => comps.Count > 1 && comps.Any(c => c is Light l && l.type == LightType.Disc));
        }

        #endregion

        #region Other Selection

        [MenuItem("Tools/Select/Rigid body", priority = 1000)]
        public static void SelectRigidBody()
        {
            SelectByMatchingComponents(comps => comps.Count > 1 && comps.Any(c => c is Rigidbody));
        } 

        #endregion

        private static void SelectByMatchingComponents(Predicate<IList<Component>> selectByComponents)
        {
            goCache.Clear();
            compCache.Clear();

            var gos = Object.FindObjectsOfType<GameObject>();

            foreach (var go in gos)
            {
                go.GetComponents(compCache);

                if (selectByComponents(compCache))
                {
                    goCache.Add(go);
                }

                compCache.Clear();
            }

            SetSelection(goCache);
            goCache.Clear();
        }

        private static void SetSelection(IList<GameObject> gos)
        {
            var selection = new Object[gos.Count];
            for (var i = 0; i < gos.Count; i++)
            {
                selection[i] = gos[i];
            }

            Selection.objects = selection;
        }
    }
}
