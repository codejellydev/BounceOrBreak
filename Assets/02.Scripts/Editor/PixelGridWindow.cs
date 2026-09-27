using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CodeJelly.BounceOrBreak.Editor
{
    /// <summary>
    /// 선택한 UI의 좌표와 크기를 도트 격자에 맞추는 EditorWindow.
    /// </summary>
    public class PixelGridWindow : EditorWindow
    {
        #region Private Variables
        private const string MENU_PATH = "Tools/BounceOrBreak/UI/Pixel Grid";
        private const string TITLE = "Pixel Grid";
        private const string UNDO_SCALE = "Scale UI Units";
        private const string UNDO_SNAP = "Snap UI To Grid";

        private float _scale = 1f;
        private float _grid = 1f;
        private string _status = string.Empty;
        #endregion


        #region Unity Lifecycle
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Hierarchy에서 선택한 오브젝트와 그 자식 전부에 적용됩니다.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();

            _scale = EditorGUILayout.FloatField("Scale Factor", _scale);

            if (GUILayout.Button("Scale Units") == true)
            {
                ScaleSelection(_scale);
            }

            EditorGUILayout.Space();

            _grid = EditorGUILayout.FloatField("Grid", _grid);

            if (GUILayout.Button("Snap To Grid") == true)
            {
                SnapSelection(_grid);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(_status, EditorStyles.wordWrappedLabel);
        }
        #endregion


        #region Private Methods
        [MenuItem(MENU_PATH)]
        private static void Open()
        {
            GetWindow<PixelGridWindow>(TITLE);
        }

        private static RectOffset Scale(RectOffset offset, float factor)
        {
            return new RectOffset(
                Mathf.RoundToInt(offset.left * factor),
                Mathf.RoundToInt(offset.right * factor),
                Mathf.RoundToInt(offset.top * factor),
                Mathf.RoundToInt(offset.bottom * factor));
        }

        /// <summary>
        /// LayoutElement는 음수를 "미설정"으로 쓰므로 설정된 값에만 적용합니다.
        /// </summary>
        private static float ScaleIfSet(float value, float factor)
        {
            return value < 0f ? value : value * factor;
        }

        private static float Snap(float value, float grid)
        {
            return Mathf.Round(value / grid) * grid;
        }

        private static Vector2 Snap(Vector2 value, float grid)
        {
            return new Vector2(Snap(value.x, grid), Snap(value.y, grid));
        }

        private static RectOffset Snap(RectOffset offset, float grid)
        {
            return new RectOffset(
                Mathf.RoundToInt(Snap(offset.left, grid)),
                Mathf.RoundToInt(Snap(offset.right, grid)),
                Mathf.RoundToInt(Snap(offset.top, grid)),
                Mathf.RoundToInt(Snap(offset.bottom, grid)));
        }

        private static float SnapIfSet(float value, float grid)
        {
            return value < 0f ? value : Snap(value, grid);
        }

        /// <summary>
        /// 유닛을 갖는 값에 계수를 곱합니다.
        /// </summary>
        private void ScaleSelection(float factor)
        {
            if (factor <= 0f)
            {
                _status = "Scale Factor must be positive.";
                return;
            }

            int count = 0;

            foreach (Transform root in Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.Editable))
            {
                foreach (RectTransform rectTransform in root.GetComponentsInChildren<RectTransform>(true))
                {
                    Undo.RecordObject(rectTransform, UNDO_SCALE);
                    rectTransform.anchoredPosition *= factor;
                    rectTransform.sizeDelta *= factor;
                    count++;
                }

                foreach (HorizontalOrVerticalLayoutGroup group in root.GetComponentsInChildren<HorizontalOrVerticalLayoutGroup>(true))
                {
                    Undo.RecordObject(group, UNDO_SCALE);
                    group.padding = Scale(group.padding, factor);
                    group.spacing *= factor;
                }

                foreach (LayoutElement element in root.GetComponentsInChildren<LayoutElement>(true))
                {
                    Undo.RecordObject(element, UNDO_SCALE);
                    element.minWidth = ScaleIfSet(element.minWidth, factor);
                    element.minHeight = ScaleIfSet(element.minHeight, factor);
                    element.preferredWidth = ScaleIfSet(element.preferredWidth, factor);
                    element.preferredHeight = ScaleIfSet(element.preferredHeight, factor);
                }

                foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
                {
                    Undo.RecordObject(text, UNDO_SCALE);
                    text.fontSize *= factor;
                }
            }

            _status = $"Scaled {count} RectTransform(s) by {factor}.";
        }

        /// <summary>
        /// 유닛을 갖는 값을 격자의 배수로 반올림합니다. 글자 크기는 소수여도 무해하므로 제외합니다.
        /// </summary>
        private void SnapSelection(float grid)
        {
            if (grid <= 0f)
            {
                _status = "Grid must be positive.";
                return;
            }

            int count = 0;

            foreach (Transform root in Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.Editable))
            {
                foreach (RectTransform rectTransform in root.GetComponentsInChildren<RectTransform>(true))
                {
                    Undo.RecordObject(rectTransform, UNDO_SNAP);
                    rectTransform.anchoredPosition = Snap(rectTransform.anchoredPosition, grid);
                    rectTransform.sizeDelta = Snap(rectTransform.sizeDelta, grid);
                    count++;
                }

                foreach (HorizontalOrVerticalLayoutGroup group in root.GetComponentsInChildren<HorizontalOrVerticalLayoutGroup>(true))
                {
                    Undo.RecordObject(group, UNDO_SNAP);
                    group.padding = Snap(group.padding, grid);
                    group.spacing = Snap(group.spacing, grid);
                }

                foreach (LayoutElement element in root.GetComponentsInChildren<LayoutElement>(true))
                {
                    Undo.RecordObject(element, UNDO_SNAP);
                    element.minWidth = SnapIfSet(element.minWidth, grid);
                    element.minHeight = SnapIfSet(element.minHeight, grid);
                    element.preferredWidth = SnapIfSet(element.preferredWidth, grid);
                    element.preferredHeight = SnapIfSet(element.preferredHeight, grid);
                }
            }

            _status = $"Snapped {count} RectTransform(s) to grid {grid}.";
        }
        #endregion
    }
}