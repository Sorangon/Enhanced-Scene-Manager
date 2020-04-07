//Created by Julien Delaunay, see more on https://github.com/Sorangon/Enhanced-Scene-Manager

using UnityEngine;

namespace SorangonToolset.EnhancedSceneManager.Internal {
    /// <summary>
    /// Initialize the Enhanced Scene Manager on application start
    /// </summary>
    [DefaultExecutionOrder(-500)] //Execute after scene orcherstrator
    public sealed class StartupSceneManager : MonoBehaviour {
        #region Current
        private static bool initialized = false;
        #endregion

        #region Editor Callbacks
        private void Reset() {
            Debug.LogError("Warning : This component is used by the Enhanced Scene Manager to initialize it, only one instance should exist, if at least one instance were initialized, " +
                "this instance will be destroyed");
        }
        #endregion

        #region Callbacks
        private void Awake() {
            if(!initialized) {
                Initialize();
                initialized = true;
            } else {
                Destroy(this);
            }

        }
        #endregion

        #region Initialize
        /// <summary>
        /// Initalize the scene system assigning the current scene list in the Enhanced Scene Manager and loading the default Scene Bundle
        /// </summary>
        private void Initialize() {
            EnhancedSceneManager.LoadSceneBundle(EnhancedSceneManager.GetCurrentSceneList().DefaultSceneBundle);
        }
        #endregion
    }
}
