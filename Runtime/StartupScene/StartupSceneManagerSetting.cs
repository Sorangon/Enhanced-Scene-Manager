//Created by Julien Delaunay, see more on https://github.com/Sorangon/Enhanced-Scene-Manager

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SorangonToolset.EnhancedSceneManager.Internal {
    /// <summary>
    /// The setting used by the the Startup Scene Manager containing the current scene list used
    /// </summary>
    public sealed class StartupSceneManagerSetting : ScriptableObject {
        #region Settings
        [SerializeField] SceneBundleList _sceneList = null;
        #endregion

        #region Current
        static StartupSceneManagerSetting instance = null;
        #endregion

        #region Properties
        internal SceneBundleList CurrentSceneList {
            get => _sceneList;
            set { 
                _sceneList = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }
        #endregion

        #region Accessors
        internal static StartupSceneManagerSetting Instance {
            get {

                if(instance == null) {
                    instance = GetAssetInResources();
                }

                return instance;
            }
        }
        #endregion

        #region Callbacks
        private void OnEnable() {
            if(instance != null && instance != this) {
                Debug.LogWarning("An instance of Startup Scene Manager Setting already exist, delete this one");
                DestroyImmediate(this, true);
            }
        }
        #endregion

        /// <summary>
        /// Returns the current scene list
        /// </summary>
        /// <returns></returns>
        internal static SceneBundleList GetCurrentSceneList() {
            return Instance._sceneList;
        }

        /// <summary>
        /// Load the startup scene manager setting or create one if missing
        /// </summary>
        /// <returns></returns>
        internal static StartupSceneManagerSetting GetAssetInResources() {
            var setting = Resources.Load<StartupSceneManagerSetting>("StartupSceneManagerSetting");
            return setting;
        }
    }
}