using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Editor.PixelGrid
{
    /// <summary>
    /// 선택한 UI의 좌표와 크기를 도트 격자에 맞추는 EditorWindow.
    /// </summary>
    public class PixelGridWindow : EditorWindow
    {
        #region Nested Classes
        /// <summary>
        /// 결과 목록 항목.
        /// </summary>
        private readonly struct Entry
        {
            #region Constructors
            public Entry(string assetPath, PixelGridFinding finding)
            {
                AssetPath = assetPath;
                Finding = finding;
            }
            #endregion


            #region Properties
            public string AssetPath { get; }
            public PixelGridFinding Finding { get; }
            #endregion
        }
        #endregion


        #region Private Variables
        private const string MENU_PATH = "Tools/BounceOrBreak/UI/Pixel Grid";
        private const string TITLE = "Pixel Grid";
        private const string UNDO_SNAP = "Snap UI To Pixel Grid";
        private const string HIERARCHY_HEADER = "Hierarchy";
        private const string DIALOG_APPLY = "Apply";
        private const string DIALOG_CANCEL = "Cancel";
        private const char KEY_SEPARATOR = ',';

        private const string DESCRIPTION =
            "Keeps UI values on the dot grid, where 1 unit is 1 dot. "
            + "Select prefabs, scenes or folders in the Project window, or objects in the Hierarchy.\n\n"
            + "UI의 좌표와 크기가 도트 격자(1유닛 = 1도트)에 맞는지 검사하고 보정합니다. "
            + "대상은 Project 창 또는 Hierarchy에서 선택합니다.";

        private static readonly GUIContent TARGET_LABEL = new GUIContent("Target", "현재 선택된 대상");
        private static readonly GUIContent CHECK_BUTTON = new GUIContent("Check", "정수가 아닌 값과 스프라이트 원본 크기의 정수배가 아닌 Image를 찾습니다. 값은 변경하지 않습니다.");
        private static readonly GUIContent SNAP_BUTTON = new GUIContent("Snap", "정수가 아닌 값을 반올림합니다. Image의 크기는 변경하지 않습니다.");

        private readonly List<Entry> _entries = new List<Entry>();

        private PixelGridTargets _targets;
        private Vector2 _scrollPosition;
        private string _selectionKey = string.Empty;
        private string _status = string.Empty;
        #endregion


        #region Unity Lifecycle
        private void OnEnable()
        {
            RefreshTargets();
        }

        private void OnInspectorUpdate()
        {
            // 폴더 트리의 선택은 Selection.objects에 반영되지 않으므로 선택 변경을 직접 비교
            if (GetSelectionKey() == _selectionKey)
            {
                return;
            }

            RefreshTargets();
            Repaint();
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(DESCRIPTION, MessageType.None);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField(TARGET_LABEL, new GUIContent(DescribeTargets()));

            using (new EditorGUI.DisabledScope(_targets.IsEmpty))
            {
                EditorGUILayout.BeginHorizontal();

                // OnGUI 도중에 씬을 로드하거나 대화상자를 표시하면 GUI 레이아웃이 깨지므로 OnGUI 이후에 수행
                if (GUILayout.Button(CHECK_BUTTON) == true)
                {
                    EditorApplication.delayCall += Check;
                }

                if (GUILayout.Button(SNAP_BUTTON) == true)
                {
                    EditorApplication.delayCall += Snap;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(_status, EditorStyles.wordWrappedLabel);

            DrawEntries();
        }
        #endregion


        #region Private Methods
        [MenuItem(MENU_PATH)]
        private static void Open()
        {
            GetWindow<PixelGridWindow>(TITLE);
        }

        private static string GetSelectionKey()
        {
            StringBuilder key = new StringBuilder();

            foreach (string guid in Selection.assetGUIDs)
            {
                key.Append(guid).Append(KEY_SEPARATOR);
            }

            foreach (Object selected in Selection.objects)
            {
                if (selected != null)
                {
                    key.Append(selected.GetHashCode()).Append(KEY_SEPARATOR);
                }
            }

            return key.ToString();
        }

        private static void Select(Entry entry)
        {
            // 에셋을 검사한 경우 Component가 유효하지 않으므로 에셋을 선택
            Object target = entry.AssetPath == null
                ? entry.Finding.Component
                : AssetDatabase.LoadMainAssetAtPath(entry.AssetPath);

            if (target == null)
            {
                return;
            }

            Selection.activeObject = target;
            EditorGUIUtility.PingObject(target);
        }

        private void RefreshTargets()
        {
            _selectionKey = GetSelectionKey();
            _targets = PixelGridTargets.FromSelection();
        }

        private string DescribeTargets()
        {
            if (_targets.IsEmpty == true)
            {
                return "Nothing selected";
            }

            if (_targets.HasAssets == true)
            {
                return $"{_targets.PrefabPaths.Count} prefab(s), {_targets.ScenePaths.Count} scene(s)";
            }

            return $"{_targets.HierarchyRoots.Count} object(s) in Hierarchy";
        }

        private void DrawEntries()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            string header = null;

            foreach (Entry entry in _entries)
            {
                string currentHeader = entry.AssetPath ?? HIERARCHY_HEADER;

                if (currentHeader != header)
                {
                    header = currentHeader;

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(header, EditorStyles.boldLabel);
                }

                if (EditorGUILayout.LinkButton(entry.Finding.Location) == true)
                {
                    Select(entry);
                }

                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.LabelField(entry.Finding.Detail, EditorStyles.wordWrappedLabel);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private bool ConfirmSnap()
        {
            // Hierarchy의 오브젝트는 Undo가 가능하므로 확인 생략
            if (_targets.HasAssets == false)
            {
                return true;
            }

            string message =
                $"Snap values in {_targets.PrefabPaths.Count} prefab(s) and {_targets.ScenePaths.Count} scene(s)?\n"
                + "This cannot be undone.\n\n"
                + $"프리팹 {_targets.PrefabPaths.Count}개, 씬 {_targets.ScenePaths.Count}개를 수정합니다.\n"
                + "이 작업은 Undo할 수 없습니다.";

            return EditorUtility.DisplayDialog(TITLE, message, DIALOG_APPLY, DIALOG_CANCEL);
        }

        private void AddEntries(GameObject root, string assetPath)
        {
            foreach (PixelGridFinding finding in PixelGridScanner.FindOffGridValues(root))
            {
                _entries.Add(new Entry(assetPath, finding));
            }

            foreach (PixelGridFinding finding in PixelGridScanner.FindNonNativeImages(root))
            {
                _entries.Add(new Entry(assetPath, finding));
            }
        }

        private void Check()
        {
            RefreshTargets();

            _entries.Clear();

            if (_targets.TryPrepare(out string error) == false)
            {
                _status = error;
                Repaint();
                return;
            }

            _targets.Visit((root, assetPath) =>
            {
                AddEntries(root, assetPath);

                return false;
            });

            _status = $"Found {_entries.Count} issue(s).";

            Repaint();
        }

        private void Snap()
        {
            RefreshTargets();

            if (ConfirmSnap() == false)
            {
                return;
            }

            _entries.Clear();

            if (_targets.TryPrepare(out string error) == false)
            {
                _status = error;
                Repaint();
                return;
            }

            int count = 0;

            _targets.Visit((root, assetPath) =>
            {
                int snapped = PixelGridScanner.Snap(root, assetPath == null);

                count += snapped;

                // Snap으로 보정할 수 없는 항목이 남았는지 함께 검사
                AddEntries(root, assetPath);

                return snapped > 0;
            });

            Undo.SetCurrentGroupName(UNDO_SNAP);

            _status = $"Snapped {count} value(s). {_entries.Count} issue(s) left.";

            Repaint();
        }
        #endregion
    }
}