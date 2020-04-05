//Created by Julien Delaunay, see more on https://github.com/Sorangon/Enhanced-Scene-Manager

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SorangonToolset.EnhancedSceneManager.Internal {
    /// <summary>
    /// This type allow you to create asset singleton to access a resource pack through a static reference, not affected by a file path
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ResourceAsset<T> : ScriptableObject where T : ScriptableObject{
        #region Instance
        private static T instance;

        /// <summary>
        /// Return the current instance of this object type
        /// </summary>
        public static T Instance {
            get {

                //Check if an asset exists 
                if(instance == null) {
#if UNITY_EDITOR
                    string[] guid = AssetDatabase.FindAssets("t:" + typeof(T).Name);
                    if(guid.Length > 0) {
                        instance = (T)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid[0]), typeof(T));
                    }
#endif
                }

                if(instance == null) {
                    Debug.LogError(typeof(T).Name + " instance is null. Ensure this class always has an instance !");
                }

                return instance;
            }
        }
        #endregion

        #region Callbacks
        private void OnEnable() {
            //Check if an instance exist and if the reference is the current asset reference is equal to the instance one
            T currentInstance = Instance;
            if(currentInstance != null && currentInstance != this) {
                DestroyImmediate(this);
            }
        }
        #endregion
    }
}
