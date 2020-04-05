using UnityEngine;
using SorangonToolset.EnhancedSceneManager.Internal;

namespace SorangonToolset.EnhancedSceneManager {
    /// <summary>
    /// A utility class for the Enhanced Scene Manager
    /// </summary>
    internal static class EnhancedSceneManagerUtils {
        #region Editor
#if UNITY_EDITOR
        /// <summary>
        /// Load the current referenced scene list in the current Startup Scene Manager Setting
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        private static void LoadSceneList() {
            EnhancedSceneManager.SetCurrentSceneList(StartupSceneManagerSetting.GetCurrentSceneList());
        }
#endif
    }
    #endregion
}
