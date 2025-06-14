using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine.AI;
using UnityEngine;
using UnityEditor;

#if UNITY_2018_3_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif

namespace NavMeshPlus.Components.Editors
{
    public class NavMeshAssetManager : ScriptableSingleton<NavMeshAssetManager>
    {
        internal struct AsyncBakeOperation
        {
            public NavMeshSurface surface;
            public NavMeshData bakeData;
            public AsyncOperation bakeOperation;
        }

        List<AsyncBakeOperation> m_BakeOperations = new List<AsyncBakeOperation>();
        internal List<AsyncBakeOperation> GetBakeOperations() { return m_BakeOperations; }

        struct SavedPrefabNavMeshData
        {
            public NavMeshSurface surface;
            public NavMeshData navMeshData;
        }

        List<SavedPrefabNavMeshData> m_PrefabNavMeshDataAssets = new List<SavedPrefabNavMeshData>();

        static string GetAndEnsureTargetPath(NavMeshSurface surface)
        {
            var activeScenePath = surface.gameObject.scene.path;
            var targetPath = "Assets";

            if (!string.IsNullOrEmpty(activeScenePath))
            {
                targetPath = Path.Combine(Path.GetDirectoryName(activeScenePath), Path.GetFileNameWithoutExtension(activeScenePath));
            }
#if UNITY_2018_3_OR_NEWER
            else
            {
                var prefabStage = PrefabStageUtility.GetPrefabStage(surface.gameObject);
                var isPartOfPrefab = prefabStage != null && prefabStage.IsPartOfPrefabContents(surface.gameObject);

                if (isPartOfPrefab)
                {
#if UNITY_2020_1_OR_NEWER
                    var assetPath = prefabStage.assetPath;
#else
                    var assetPath = prefabStage.prefabAssetPath;
#endif
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        var prefabDirectoryName = Path.GetDirectoryName(assetPath);
                        if (!string.IsNullOrEmpty(prefabDirectoryName))
                            targetPath = prefabDirectoryName;
                    }
                }
            }
#endif
            if (!Directory.Exists(targetPath))
                Directory.CreateDirectory(targetPath);

            return targetPath;
        }

        static void CreateNavMeshAsset(NavMeshSurface surface)
        {
            var targetPath = GetAndEnsureTargetPath(surface);

            var combinedAssetPath = Path.Combine(targetPath, "NavMesh-" + surface.name + ".asset");
            combinedAssetPath = AssetDatabase.GenerateUniqueAssetPath(combinedAssetPath);
            AssetDatabase.CreateAsset(surface.navMeshData, combinedAssetPath);
        }

        NavMeshData GetNavMeshAssetToDelete(NavMeshSurface navSurface)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(navSurface) && !PrefabUtility.IsPartOfModelPrefab(navSurface))
            {
                var parentSurface = PrefabUtility.GetCorrespondingObjectFromSource(navSurface) as NavMeshSurface;
                if (parentSurface && navSurface.navMeshData == parentSurface.navMeshData)
                    return null;
            }

#if UNITY_2018_3_OR_NEWER
            var prefabStage = PrefabStageUtility.GetPrefabStage(navSurface.gameObject);
            var isPartOfPrefab = prefabStage != null && prefabStage.IsPartOfPrefabContents(navSurface.gameObject);
            if (isPartOfPrefab && IsCurrentPrefabNavMeshDataStored(navSurface))
                return null;
#endif

            return navSurface.navMeshData;
        }

        bool IsCurrentPrefabNavMeshDataStored(NavMeshSurface surface)
        {
            if (surface == null)
                return false;

            foreach (var storedAssetInfo in m_PrefabNavMeshDataAssets)
            {
                if (storedAssetInfo.surface == surface)
                    return storedAssetInfo.navMeshData == surface.navMeshData;
            }

            return false;
        }


        void ClearSurface(NavMeshSurface navSurface)
        {
            var hasNavMeshData = navSurface.navMeshData != null;
            StoreNavMeshDataIfInPrefab(navSurface);

            var assetToDelete = GetNavMeshAssetToDelete(navSurface);
            navSurface.RemoveData();

            if (hasNavMeshData)
            {
                SetNavMeshData(navSurface, null);
                EditorSceneManager.MarkSceneDirty(navSurface.gameObject.scene);
            }

            if (assetToDelete)
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(assetToDelete));
        }

        public void StartBakingSurfaces(UnityEngine.Object[] surfaces)
        {
            EditorApplication.update -= UpdateAsyncBuildOperations;
            EditorApplication.update += UpdateAsyncBuildOperations;

            foreach (NavMeshSurface surf in surfaces)
            {
                StoreNavMeshDataIfInPrefab(surf);

                var bakeData = InitializeBakeData(surf);
                var bakeOperation = surf.UpdateNavMesh(bakeData);

                var oper = new AsyncBakeOperation
                {
                    bakeData = bakeData,
                    bakeOperation = bakeOperation,
                    surface = surf
                };

                m_BakeOperations.Add(oper);
            }
        }

        static NavMeshData InitializeBakeData(NavMeshSurface surface)
        {
            var emptySources = new List<NavMeshBuildSource>();
            var emptyBounds = new Bounds();
            return UnityEngine.AI.NavMeshBuilder.BuildNavMeshData(surface.GetBuildSettings(), emptySources, emptyBounds, surface.transform.position, surface.transform.rotation);
        }

        void UpdateAsyncBuildOperations()
        {
            foreach (var oper in m_BakeOperations)
            {
                if (oper.surface == null || oper.bakeOperation == null)
                    continue;

                if (oper.bakeOperation.isDone)
                {
                    var surface = oper.surface;
                    var delete = GetNavMeshAssetToDelete(surface);
                    if (delete != null)
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(delete));

                    surface.RemoveData();
                    SetNavMeshData(surface, oper.bakeData);

                    if (surface.isActiveAndEnabled)
                        surface.AddData();
                    CreateNavMeshAsset(surface);
                    EditorSceneManager.MarkSceneDirty(surface.gameObject.scene);
                }
            }

            m_BakeOperations.RemoveAll(o => o.bakeOperation == null || o.bakeOperation.isDone);
            if (m_BakeOperations.Count == 0)
                EditorApplication.update -= UpdateAsyncBuildOperations;
        }

        public bool IsSurfaceBaking(NavMeshSurface surface)
        {
            if (surface == null)
                return false;

            foreach (var oper in m_BakeOperations)
            {
                if (oper.surface == surface)
                    return true;
            }

            return false;
        }

        public void ClearSurfaces(UnityEngine.Object[] surfaces)
        {
            foreach (NavMeshSurface s in surfaces)
                ClearSurface(s);
        }

        static void SetNavMeshData(NavMeshSurface navSurface, NavMeshData navMeshData)
        {
            var so = new SerializedObject(navSurface);
            var navMeshDataProperty = so.FindProperty("m_NavMeshData");
            navMeshDataProperty.objectReferenceValue = navMeshData;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

#if UNITY_2018_3_OR_NEWER
        void StoreNavMeshDataIfInPrefab(NavMeshSurface surfaceToStore)
        {
            var prefabStage = PrefabStageUtility.GetPrefabStage(surfaceToStore.gameObject);
            var isPartOfPrefab = prefabStage != null && prefabStage.IsPartOfPrefabContents(surfaceToStore.gameObject);
            if (!isPartOfPrefab)
                return;

            foreach (var storedAssetInfo in m_PrefabNavMeshDataAssets)
                if (storedAssetInfo.surface == surfaceToStore)
                    return;

            if (m_PrefabNavMeshDataAssets.Count == 0)
            {
                PrefabStage.prefabSaving -= DeleteStoredNavMeshDataAssetsForOwnedSurfaces;
                PrefabStage.prefabSaving += DeleteStoredNavMeshDataAssetsForOwnedSurfaces;

                PrefabStage.prefabStageClosing -= ForgetUnsavedNavMeshDataChanges;
                PrefabStage.prefabStageClosing += ForgetUnsavedNavMeshDataChanges;
            }

            var isDataOwner = true;
            if (PrefabUtility.IsPartOfPrefabInstance(surfaceToStore) && !PrefabUtility.IsPartOfModelPrefab(surfaceToStore))
            {
                var basePrefabSurface = PrefabUtility.GetCorrespondingObjectFromSource(surfaceToStore) as NavMeshSurface;
                isDataOwner = basePrefabSurface == null || surfaceToStore.navMeshData != basePrefabSurface.navMeshData;
            }

            m_PrefabNavMeshDataAssets.Add(new SavedPrefabNavMeshData
            {
                surface = surfaceToStore,
                navMeshData = isDataOwner ? surfaceToStore.navMeshData : null
            });
        }

        void DeleteStoredNavMeshDataAssetsForOwnedSurfaces(GameObject gameObjectInPrefab)
        {
            var surfaces = gameObjectInPrefab.GetComponentsInChildren<NavMeshSurface>(true);
            foreach (var surface in surfaces)
                DeleteStoredPrefabNavMeshDataAsset(surface);
        }

        void DeleteStoredPrefabNavMeshDataAsset(NavMeshSurface surface)
        {
            for (var i = m_PrefabNavMeshDataAssets.Count - 1; i >= 0; i--)
            {
                var storedAssetInfo = m_PrefabNavMeshDataAssets[i];
                if (storedAssetInfo.surface == surface)
                {
                    var storedNavMeshData = storedAssetInfo.navMeshData;
                    if (storedNavMeshData != null && storedNavMeshData != surface.navMeshData)
                    {
                        var assetPath = AssetDatabase.GetAssetPath(storedNavMeshData);
                        AssetDatabase.DeleteAsset(assetPath);
                    }

                    m_PrefabNavMeshDataAssets.RemoveAt(i);
                    break;
                }
            }

            if (m_PrefabNavMeshDataAssets.Count == 0)
            {
                PrefabStage.prefabSaving -= DeleteStoredNavMeshDataAssetsForOwnedSurfaces;
                PrefabStage.prefabStageClosing -= ForgetUnsavedNavMeshDataChanges;
            }
        }

        void ForgetUnsavedNavMeshDataChanges(PrefabStage prefabStage)
        {
            if (prefabStage == null)
                return;

            var allSurfacesInPrefab = prefabStage.prefabContentsRoot.GetComponentsInChildren<NavMeshSurface>(true);
            NavMeshSurface surfaceInPrefab = null;
            var index = 0;
            do
            {
                if (allSurfacesInPrefab.Length > 0)
                    surfaceInPrefab = allSurfacesInPrefab[index];

                for (var i = m_PrefabNavMeshDataAssets.Count - 1; i >= 0; i--)
                {
                    var storedPrefabInfo = m_PrefabNavMeshDataAssets[i];
                    if (storedPrefabInfo.surface == null)
                    {
                        if (storedPrefabInfo.navMeshData != null)
                        {
                            var assetPath = AssetDatabase.GetAssetPath(storedPrefabInfo.navMeshData);
                            AssetDatabase.DeleteAsset(assetPath);
                        }

                        m_PrefabNavMeshDataAssets.RemoveAt(i);
                    }
                    else if (surfaceInPrefab != null && storedPrefabInfo.surface == surfaceInPrefab)
                    {
                        var baseSurface = PrefabUtility.GetCorrespondingObjectFromSource(surfaceInPrefab) as NavMeshSurface;
                        if (baseSurface == null || surfaceInPrefab.navMeshData != baseSurface.navMeshData)
                        {
                            var assetPath = AssetDatabase.GetAssetPath(surfaceInPrefab.navMeshData);
                            AssetDatabase.DeleteAsset(assetPath);
                        }

                        m_PrefabNavMeshDataAssets.RemoveAt(i);
                    }
                }
            } while (++index < allSurfacesInPrefab.Length);

            if (m_PrefabNavMeshDataAssets.Count == 0)
            {
                PrefabStage.prefabSaving -= DeleteStoredNavMeshDataAssetsForOwnedSurfaces;
                PrefabStage.prefabStageClosing -= ForgetUnsavedNavMeshDataChanges;
            }
        }
#endif
    }
}
