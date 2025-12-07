using UnityEngine;

namespace CodeJelly.BounceOrBreak.Core
{
    /// <summary>
    /// Lazy initialization을 지원하는 Generic Singleton MonoBehaviour.
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        #region Properties
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<T>();

                    if (_instance == null)
                    {
                        SetInstance();
                    }
                }

                return _instance;
            }
        }
        #endregion


        #region Private Variables
        private static T _instance;
        #endregion


        #region Unity Lifecycle
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                if (Application.isPlaying == true)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[Singleton] Duplicate instance of {typeof(T).Name} found. Destroying.");
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
        #endregion


        #region Private Methods
        private static void SetInstance()
        {
            GameObject newObject = new GameObject()
            {
                name = typeof(T).Name + " (Singleton)"
            };
            _instance = newObject.AddComponent<T>();

            if (Application.isPlaying == true)
            {
                DontDestroyOnLoad(newObject);
            }
        }
        #endregion
    }
}