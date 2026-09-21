using NUnit.Framework;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

namespace CodeJelly.BounceOrBreak.Tests
{
    /// <summary>
    /// Addressables 등록 규칙을 검증하는 테스트 클래스.
    /// </summary>
    public class AddressableRegistrationTests
    {
        #region Private Variables
        /// <summary>
        /// Localization 등 패키지가 생성해 등록 구성을 직접 정할 수 없는 그룹
        /// </summary>
        private static readonly HashSet<string> PACKAGE_MANAGED_GROUPS = new HashSet<string> { "Localization-Locales" };

        /// <summary>
        /// 검사 대상 최상위 폴더의 이름 규칙. 번호 접두사가 없는 외부 패키지 폴더는 제외
        /// </summary>
        private static readonly Regex PROJECT_FOLDER_PATTERN = new Regex(@"^Assets/[0-9]{2}\.");

        private const string ASSETS_ROOT = "Assets";
        private const string DEFAULT_ADDRESS_PREFIX = "Assets/";
        private const string ASSET_GUID_PROPERTY = "m_AssetGUID";
        #endregion


        #region Public Methods
        /// <summary>
        /// 등록된 에셋을 다른 에셋이 직접 참조하지 않습니다.
        /// </summary>
        [Test]
        public void Entries_AreNotReferencedDirectly()
        {
            Dictionary<string, string> addressByPath = CollectEntryAddressByPath();

            foreach (string assetPath in CollectProjectAssetPaths())
            {
                // AssetReference는 GUID 문자열이라 의존성에 잡히지 않는다
                foreach (string dependencyPath in AssetDatabase.GetDependencies(assetPath, false))
                {
                    if (dependencyPath == assetPath)
                    {
                        continue;
                    }

                    Assert.IsFalse(addressByPath.ContainsKey(dependencyPath),
                        $"'{assetPath}' directly references the Addressable entry '{dependencyPath}'. " +
                        "Reference it with an AssetReference, or remove the entry from Addressables.");
                }
            }
        }

        /// <summary>
        /// AssetReference가 가리키는 대상이 모두 등록돼 있습니다.
        /// </summary>
        [Test]
        public void AssetReferences_PointToRegisteredAssets()
        {
            HashSet<string> registeredGuids = new HashSet<string>();

            foreach (string assetPath in CollectEntryAddressByPath().Keys)
            {
                registeredGuids.Add(AssetDatabase.AssetPathToGUID(assetPath));
            }

            int checkedCount = 0;

            foreach (string assetPath in CollectProjectAssetPaths())
            {
                foreach (SerializedObject serializedObject in CollectSerializedObjects(assetPath))
                {
                    SerializedProperty property = serializedObject.GetIterator();

                    while (property.NextVisible(true) == true)
                    {
                        if (property.propertyType != SerializedPropertyType.Generic)
                        {
                            continue;
                        }

                        SerializedProperty guidProperty = property.FindPropertyRelative(ASSET_GUID_PROPERTY);

                        // 비워 두는 것이 정상인 AssetReference가 있는 경우 건너뜀
                        if (guidProperty == null || string.IsNullOrEmpty(guidProperty.stringValue) == true)
                        {
                            continue;
                        }

                        Assert.IsTrue(registeredGuids.Contains(guidProperty.stringValue),
                            $"'{assetPath}' references an unregistered asset " +
                            $"'{AssetDatabase.GUIDToAssetPath(guidProperty.stringValue)}' through an AssetReference. " +
                            "An AssetReference can only load Addressable assets.");
                        checkedCount++;
                    }
                }
            }

            // AssetReference를 하나도 못 찾으면 무조건 통과하므로 검사 횟수를 검증
            Assert.Greater(checkedCount, 0, "No AssetReference found in project assets.");
        }

        /// <summary>
        /// 등록 주소가 기본값인 에셋 경로가 아닙니다.
        /// </summary>
        [Test]
        public void Entries_DoNotUseDefaultPathAddress()
        {
            foreach (KeyValuePair<string, string> entry in CollectEntryAddressByPath())
            {
                Assert.IsFalse(entry.Value.StartsWith(DEFAULT_ADDRESS_PREFIX),
                    $"Address of '{entry.Key}' is still the default asset path. Give it a name.");
            }
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 검사 대상 그룹의 등록 항목을 에셋 경로 기준으로 모읍니다.
        /// </summary>
        private static Dictionary<string, string> CollectEntryAddressByPath()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

            // 설정을 못 찾으면 무조건 통과하므로 로드 자체를 검증
            Assert.IsNotNull(settings, "No AddressableAssetSettings found.");

            Dictionary<string, string> addressByPath = new Dictionary<string, string>();

            foreach (AddressableAssetGroup group in settings.groups)
            {
                if (group == null || PACKAGE_MANAGED_GROUPS.Contains(group.Name) == true)
                {
                    continue;
                }

                foreach (AddressableAssetEntry entry in group.entries)
                {
                    addressByPath[entry.AssetPath] = entry.address;
                }
            }

            Assert.IsNotEmpty(addressByPath, "No Addressable entry found.");

            return addressByPath;
        }

        /// <summary>
        /// 에셋 하나에서 직렬화 필드를 들고 있는 대상을 모두 모읍니다.
        /// </summary>
        private static IEnumerable<SerializedObject> CollectSerializedObjects(string assetPath)
        {
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

            if (asset is GameObject prefab)
            {
                foreach (MonoBehaviour behaviour in prefab.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (behaviour != null)
                    {
                        yield return new SerializedObject(behaviour);
                    }
                }
            }
            else if (asset is ScriptableObject)
            {
                yield return new SerializedObject(asset);
            }
        }

        /// <summary>
        /// 검사 대상 폴더의 모든 에셋 경로를 모읍니다.
        /// </summary>
        private static IEnumerable<string> CollectProjectAssetPaths()
        {
            List<string> projectFolders = new List<string>();

            foreach (string folder in AssetDatabase.GetSubFolders(ASSETS_ROOT))
            {
                if (PROJECT_FOLDER_PATTERN.IsMatch(folder) == true)
                {
                    projectFolders.Add(folder);
                }
            }

            Assert.IsNotEmpty(projectFolders, "No asset folder to check.");

            foreach (string guid in AssetDatabase.FindAssets(string.Empty, projectFolders.ToArray()))
            {
                yield return AssetDatabase.GUIDToAssetPath(guid);
            }
        }
        #endregion
    }
}