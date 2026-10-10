using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeJelly.BounceOrBreak.Editor.PixelGrid
{
    /// <summary>
    /// 검사 대상을 모으고 로드, 처리, 저장을 수행하는 클래스.
    /// 프리팹과 씬은 파일을 로드해 처리하고, Hierarchy에서 선택한 오브젝트는 현재 상태 그대로 처리합니다.
    /// </summary>
    public class PixelGridTargets
    {
        #region Nested Classes
        /// <summary>
        /// 대상의 루트마다 호출되는 콜백.
        /// </summary>
        /// <param name="assetPath">루트가 속한 에셋의 경로. Hierarchy의 오브젝트이면 null</param>
        /// <returns>저장이 필요하면 true</returns>
        public delegate bool RootVisitor(GameObject root, string assetPath);
        #endregion


        #region Constructors
        private PixelGridTargets(
            List<string> prefabPaths,
            List<string> scenePaths,
            List<GameObject> hierarchyRoots)
        {
            PrefabPaths = prefabPaths;
            ScenePaths = scenePaths;
            HierarchyRoots = hierarchyRoots;
        }
        #endregion


        #region Properties
        public IReadOnlyList<string> PrefabPaths { get; }

        public IReadOnlyList<string> ScenePaths { get; }

        public IReadOnlyList<GameObject> HierarchyRoots { get; }

        public bool HasAssets => PrefabPaths.Count > 0 || ScenePaths.Count > 0;

        public bool IsEmpty => HasAssets == false && HierarchyRoots.Count == 0;
        #endregion


        #region Private Variables
        private const string PREFAB_EXTENSION = ".prefab";
        private const string SCENE_EXTENSION = ".unity";
        private const string PREFAB_FILTER = "t:Prefab";
        private const string SCENE_FILTER = "t:Scene";
        #endregion


        #region Public Methods
        /// <summary>
        /// 현재 선택에서 대상을 모읍니다.
        /// Project 창의 선택을 우선하며, 없으면 Hierarchy의 선택을 사용합니다.
        /// </summary>
        public static PixelGridTargets FromSelection()
        {
            List<string> prefabPaths = new List<string>();
            List<string> scenePaths = new List<string>();
            List<GameObject> hierarchyRoots = new List<GameObject>();

            foreach (string guid in Selection.assetGUIDs)
            {
                AddAssetPaths(AssetDatabase.GUIDToAssetPath(guid), prefabPaths, scenePaths);
            }

            if (prefabPaths.Count == 0 && scenePaths.Count == 0)
            {
                foreach (Transform transform in Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.Editable | SelectionMode.ExcludePrefab))
                {
                    hierarchyRoots.Add(transform.gameObject);
                }
            }

            return new PixelGridTargets(prefabPaths, scenePaths, hierarchyRoots);
        }

        /// <summary>
        /// folders에 포함된 모든 프리팹과 씬을 모읍니다.
        /// </summary>
        public static PixelGridTargets FromFolders(params string[] folders)
        {
            List<string> prefabPaths = new List<string>();
            List<string> scenePaths = new List<string>();

            foreach (string folder in folders)
            {
                AddAssetPaths(folder, prefabPaths, scenePaths);
            }

            return new PixelGridTargets(prefabPaths, scenePaths, new List<GameObject>());
        }

        /// <summary>
        /// 대상을 처리할 수 있는 상태인지 확인합니다.
        /// 씬이 대상이면 현재 씬의 저장 여부를 묻는 대화상자를 표시합니다.
        /// </summary>
        /// <param name="error">처리할 수 없는 이유. 처리할 수 있으면 null</param>
        public bool TryPrepare(out string error)
        {
            error = null;

            if (EditorApplication.isPlayingOrWillChangePlaymode == true)
            {
                error = "Exit Play Mode first.";
                return false;
            }

            if (HasAssets == false)
            {
                return true;
            }

            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                error = "Close Prefab Mode first.";
                return false;
            }

            if (ScenePaths.Count > 0 && EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo() == false)
            {
                error = "Cancelled.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 대상의 루트마다 visitor를 호출하고, 변경된 에셋을 저장합니다.
        /// </summary>
        public void Visit(RootVisitor visitor)
        {
            foreach (GameObject root in HierarchyRoots)
            {
                visitor(root, null);
            }

            foreach (string path in PrefabPaths)
            {
                VisitPrefab(path, visitor);
            }

            if (ScenePaths.Count > 0)
            {
                VisitScenes(visitor);
            }
        }
        #endregion


        #region Private Methods
        private static void AddAssetPaths(string path, List<string> prefabPaths, List<string> scenePaths)
        {
            if (AssetDatabase.IsValidFolder(path) == true)
            {
                string[] folders = { path };

                foreach (string guid in AssetDatabase.FindAssets(PREFAB_FILTER, folders))
                {
                    AddUnique(AssetDatabase.GUIDToAssetPath(guid), prefabPaths);
                }

                foreach (string guid in AssetDatabase.FindAssets(SCENE_FILTER, folders))
                {
                    AddUnique(AssetDatabase.GUIDToAssetPath(guid), scenePaths);
                }

                return;
            }

            if (path.EndsWith(PREFAB_EXTENSION) == true)
            {
                AddUnique(path, prefabPaths);
            }
            else if (path.EndsWith(SCENE_EXTENSION) == true)
            {
                AddUnique(path, scenePaths);
            }
        }

        private static void AddUnique(string path, List<string> paths)
        {
            if (paths.Contains(path) == false)
            {
                paths.Add(path);
            }
        }

        /// <summary>
        /// 현재 로드된 씬의 구성을 반환합니다.
        /// </summary>
        /// <returns>현재 씬 구성. 저장된 적 없는 씬이 로드돼 있으면 null</returns>
        private static SceneSetup[] CaptureSceneSetup()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (string.IsNullOrEmpty(SceneManager.GetSceneAt(i).path) == true)
                {
                    return null;
                }
            }

            return EditorSceneManager.GetSceneManagerSetup();
        }

        private static void VisitPrefab(string path, RootVisitor visitor)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);

            try
            {
                if (visitor(root, path) == true)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void VisitScene(string path, RootVisitor visitor)
        {
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

            try
            {
                bool isChanged = false;

                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    if (visitor(root, path) == true)
                    {
                        isChanged = true;
                    }
                }

                if (isChanged == true)
                {
                    EditorSceneManager.SaveScene(scene);
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private void VisitScenes(RootVisitor visitor)
        {
            SceneSetup[] setup = CaptureSceneSetup();

            // 파일에 저장된 값을 기준으로 처리하기 위해 빈 씬에서 새로 로드
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            try
            {
                foreach (string path in ScenePaths)
                {
                    VisitScene(path, visitor);
                }
            }
            finally
            {
                if (setup != null)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(setup);
                }
            }
        }
        #endregion
    }
}