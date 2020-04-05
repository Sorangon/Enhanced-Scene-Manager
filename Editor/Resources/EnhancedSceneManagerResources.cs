//Created by Julien Delaunay, see more on https://github.com/Sorangon/Enhanced-Scene-Manager

using UnityEngine;
using SorangonToolset.EnhancedSceneManager.Internal;
using UnityEditor;

internal class EnhancedSceneManagerResources : ResourceAsset<EnhancedSceneManagerResources>{
    #region Data
    [SerializeField] 
    private SceneAsset startupScene = null;
    #endregion

    #region Accessors
    public SceneAsset StartupScene => startupScene;
    #endregion
}
