using System.Collections;
using UnityEngine;

namespace SorangonToolset.EnhancedSceneManager.Internal {
    /// <summary>
    /// This class exist only to manage async loading coroutines
    /// </summary>
    [DefaultExecutionOrder(-1000)] //Execute before startup scene manager
    public class EnhancedSceneOrchestrator : MonoBehaviour {
        #region Coroutines
        private IEnumerator loadingCoroutine = null;
        #endregion

        #region Callbacks
        private void Awake() {
            DontDestroyOnLoad(this);
        }
        #endregion

        #region Process Coroutines
        /// <summary>
        /// Starts the loading coroutine
        /// </summary>
        /// <param name="coroutine"></param>
        internal void StartLoadingCoroutine (IEnumerator coroutine) {
            loadingCoroutine = coroutine;
            StartCoroutine(loadingCoroutine);
        }

        /// <summary>
        /// Stops processing the loading coroutine 
        /// </summary>
        internal void StopLoadingCoroutine() {
            if(loadingCoroutine != null) {
                StartCoroutine(loadingCoroutine);
            }
        }
        #endregion
    }
}
