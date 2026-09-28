using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CodeJelly.BounceOrBreak.Editor.PixelGrid
{
    /// <summary>
    /// UI의 좌표와 크기가 도트 격자에 맞는지 검사하고 보정하는 클래스.
    /// 프리팹 인스턴스에서는 override된 값만 대상으로 합니다.
    /// </summary>
    public static class PixelGridScanner
    {
        #region Nested Classes
        /// <summary>
        /// 값마다 호출되는 콜백.
        /// </summary>
        /// <returns>값을 변경했으면 true</returns>
        private delegate bool ValueVisitor(Component component, SerializedProperty property, float value);
        #endregion


        #region Properties
        /// <summary>
        /// 도트 격자 검사의 대상 속성 목록.
        /// </summary>
        public static IReadOnlyList<PixelGridRule> LayoutRules => LAYOUT_RULES;
        #endregion


        #region Private Variables
        private const string SPRITE_PROPERTY = "m_Sprite";
        private const string SIZE_X_PROPERTY = "m_SizeDelta.x";
        private const string SIZE_Y_PROPERTY = "m_SizeDelta.y";

        private static readonly PixelGridRule[] LAYOUT_RULES =
        {
            new PixelGridRule(typeof(RectTransform), false, "m_AnchoredPosition.x", "m_AnchoredPosition.y", SIZE_X_PROPERTY, SIZE_Y_PROPERTY),
            new PixelGridRule(typeof(HorizontalOrVerticalLayoutGroup), false, "m_Padding.m_Left", "m_Padding.m_Right", "m_Padding.m_Top", "m_Padding.m_Bottom", "m_Spacing"),
            new PixelGridRule(typeof(LayoutElement), true, "m_MinWidth", "m_MinHeight", "m_PreferredWidth", "m_PreferredHeight"),
        };
        #endregion


        #region Public Methods
        /// <summary>
        /// 정수가 아닌 값을 찾습니다.
        /// </summary>
        public static List<PixelGridFinding> FindOffGridValues(GameObject root)
        {
            List<PixelGridFinding> findings = new List<PixelGridFinding>();

            Visit(root, LAYOUT_RULES, false, (component, property, value) =>
            {
                if (IsWhole(value) == false)
                {
                    findings.Add(new PixelGridFinding(component, GetLocation(root, component), $"{component.GetType().Name}.{property.propertyPath} = {value}"));
                }

                return false;
            });

            return findings;
        }

        /// <summary>
        /// 스프라이트 원본 크기의 정수배가 아닌 Image를 찾습니다.
        /// </summary>
        public static List<PixelGridFinding> FindNonNativeImages(GameObject root)
        {
            List<PixelGridFinding> findings = new List<PixelGridFinding>();

            foreach (Image image in root.GetComponentsInChildren<Image>(true))
            {
                if (CanCheckNativeMultiple(image) == false || HasOwnSize(image) == false)
                {
                    continue;
                }

                Vector2 size = image.rectTransform.sizeDelta;
                Rect spriteRect = image.sprite.rect;

                if (IsNativeMultiple(size, spriteRect.size, image.preserveAspect) == true)
                {
                    continue;
                }

                findings.Add(new PixelGridFinding(image, GetLocation(root, image), $"{size.x}x{size.y} is not a whole multiple of {image.sprite.name} ({spriteRect.width}x{spriteRect.height})"));
            }

            return findings;
        }

        /// <summary>
        /// 정수가 아닌 값을 반올림합니다.
        /// </summary>
        /// <returns>변경한 값의 개수</returns>
        public static int Snap(GameObject root, bool useUndo)
        {
            return Apply(root, LAYOUT_RULES, Mathf.Round, useUndo);
        }

        /// <summary>
        /// rules에 해당하는 값을 convert의 반환값으로 변경합니다.
        /// </summary>
        /// <returns>변경한 값의 개수</returns>
        public static int Apply(
            GameObject root,
            IReadOnlyList<PixelGridRule> rules,
            Func<float, float> convert,
            bool useUndo)
        {
            return Visit(root, rules, useUndo, (component, property, value) =>
            {
                float converted = convert(value);

                if (Mathf.Approximately(value, converted) == true)
                {
                    return false;
                }

                Write(property, converted);

                return true;
            });
        }
        #endregion


        #region Private Methods
        private static bool IsWhole(float value)
        {
            return Mathf.Approximately(value, Mathf.Round(value));
        }

        private static bool IsWholeMultiple(float ratio)
        {
            return ratio >= 1f && IsWhole(ratio) == true;
        }

        private static bool IsNativeMultiple(Vector2 size, Vector2 spriteSize, bool preserveAspect)
        {
            float ratioX = size.x / spriteSize.x;
            float ratioY = size.y / spriteSize.y;

            // Preserve Aspect는 두 축 중 작은 배율을 사용
            if (preserveAspect == true)
            {
                return IsWholeMultiple(Mathf.Min(ratioX, ratioY));
            }

            return IsWholeMultiple(ratioX) == true && Mathf.Approximately(ratioX, ratioY) == true;
        }

        private static bool IsDriven(Component component)
        {
            return component is RectTransform rectTransform && rectTransform.drivenByObject != null;
        }

        /// <summary>
        /// component의 값이 원본 프리팹에서 상속되는지 확인합니다.
        /// </summary>
        private static bool IsPrefabInherited(Component component)
        {
            return PrefabUtility.IsPartOfPrefabInstance(component) == true
                && PrefabUtility.IsAddedComponentOverride(component) == false;
        }

        /// <summary>
        /// 정수배 검사의 대상인 Image인지 확인합니다.
        /// </summary>
        private static bool CanCheckNativeMultiple(Image image)
        {
            Sprite sprite = image.sprite;

            if (sprite == null || sprite.texture == null)
            {
                return false;
            }

            if (image.type != Image.Type.Simple || sprite.texture.filterMode != FilterMode.Point)
            {
                return false;
            }

            RectTransform rectTransform = image.rectTransform;

            if (IsDriven(rectTransform) == true || rectTransform.anchorMin != rectTransform.anchorMax)
            {
                return false;
            }

            Vector2 size = rectTransform.sizeDelta;

            return size.x > 0f && size.y > 0f;
        }

        /// <summary>
        /// image의 크기를 원본 프리팹이 아닌 검사 대상이 정의하는지 확인합니다.
        /// </summary>
        private static bool HasOwnSize(Image image)
        {
            RectTransform rectTransform = image.rectTransform;

            if (IsPrefabInherited(rectTransform) == false)
            {
                return true;
            }

            HashSet<string> sizePaths = CollectOverriddenPaths(rectTransform);

            if (sizePaths.Contains(SIZE_X_PROPERTY) == true || sizePaths.Contains(SIZE_Y_PROPERTY) == true)
            {
                return true;
            }

            return CollectOverriddenPaths(image).Contains(SPRITE_PROPERTY);
        }

        /// <summary>
        /// component에 기록된 override의 propertyPath를 모읍니다.
        /// </summary>
        private static HashSet<string> CollectOverriddenPaths(Component component)
        {
            HashSet<string> paths = new HashSet<string>();

            UnityEngine.Object source = PrefabUtility.GetCorrespondingObjectFromSource(component);
            GameObject instanceRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(component);
            PropertyModification[] modifications = PrefabUtility.GetPropertyModifications(instanceRoot);

            if (modifications == null)
            {
                return paths;
            }

            foreach (PropertyModification modification in modifications)
            {
                // 인스턴스 전체의 override 중 component에 해당하는 것만 사용
                if (modification.target == source)
                {
                    paths.Add(modification.propertyPath);
                }
            }

            return paths;
        }

        private static string GetLocation(GameObject root, Component component)
        {
            Transform current = component.transform;
            string location = current.name;

            while (current != root.transform && current.parent != null)
            {
                current = current.parent;
                location = $"{current.name}/{location}";
            }

            return location;
        }

        private static float Read(SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.Integer ? property.intValue : property.floatValue;
        }

        private static void Write(SerializedProperty property, float value)
        {
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                property.intValue = Mathf.RoundToInt(value);
                return;
            }

            property.floatValue = value;
        }

        /// <summary>
        /// rules에 해당하는 값마다 visitor를 호출합니다.
        /// </summary>
        /// <returns>visitor가 변경한 값의 개수</returns>
        private static int Visit(
            GameObject root,
            IReadOnlyList<PixelGridRule> rules,
            bool useUndo,
            ValueVisitor visitor)
        {
            int count = 0;

            foreach (PixelGridRule rule in rules)
            {
                foreach (Component component in root.GetComponentsInChildren(rule.ComponentType, true))
                {
                    // 레이아웃이 제어하는 값은 수정해도 다시 계산되므로 제외
                    if (IsDriven(component) == true)
                    {
                        continue;
                    }

                    count += Visit(component, rule, useUndo, visitor);
                }
            }

            return count;
        }

        private static int Visit(
            Component component,
            PixelGridRule rule,
            bool useUndo,
            ValueVisitor visitor)
        {
            bool isPrefabInherited = IsPrefabInherited(component);
            HashSet<string> overriddenPaths = isPrefabInherited == true ? CollectOverriddenPaths(component) : null;

            using (SerializedObject serializedObject = new SerializedObject(component))
            {
                int count = 0;

                foreach (string path in rule.PropertyPaths)
                {
                    // 수정 시 불필요한 override가 생성되는 것을 방지
                    if (isPrefabInherited == true && overriddenPaths.Contains(path) == false)
                    {
                        continue;
                    }

                    SerializedProperty property = serializedObject.FindProperty(path);

                    if (property == null)
                    {
                        continue;
                    }

                    float value = Read(property);

                    if (rule.IsNegativeUnset == true && value < 0f)
                    {
                        continue;
                    }

                    if (visitor(component, property, value) == true)
                    {
                        count++;
                    }
                }

                if (count == 0)
                {
                    return count;
                }

                if (useUndo == true)
                {
                    serializedObject.ApplyModifiedProperties();
                }
                else
                {
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }

                return count;
            }
        }
        #endregion
    }
}