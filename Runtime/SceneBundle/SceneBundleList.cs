//Created by Julien Delaunay, see more on https://github.com/Sorangon/Enhanced-Scene-Manager

using UnityEngine;

namespace SorangonToolset.EnhancedSceneManager {
	/// <summary>
	/// A scene list contains multiple scene groups, it will manage all the scene to load or to make persistant
	/// </summary>
	[CreateAssetMenu(menuName = "Enhanced Scene Manager/Scene Bundle List", fileName = "NewSceneBundleList", order = 150)]
	public class SceneBundleList : ScriptableObject{
		#region Settings
		[HideInInspector, SerializeField, Tooltip("The bundle of persistant scenes that are are never unloaded")]
		private SceneBundle persistantScenesBundle = null;

		[HideInInspector, SerializeField, Tooltip("All scene bundles that should be loaded through the application runtime")]
		private SceneBundle[] scenesBundles = { };
		#endregion

		#region Properties
		/// <summary> The bundle of the persistant game scenes </summary>
		public SceneBundle PersistantScenesBundle => persistantScenesBundle;

		/// <summary> All the scene bundles of this list </summary>
		public SceneBundle[] ScenesBundles => scenesBundles;

		/// <summary> The default scene bundle of the list </summary>
		public SceneBundle DefaultSceneBundle => scenesBundles[0];
		#endregion

		#region Check Bundles
		/// <summary>
		/// Check if the input bundle is contained in the level list
		/// </summary>
		public bool HasBundleInList(SceneBundle bundleToCheck) {
			foreach(SceneBundle bundle in scenesBundles) {
				if(bundleToCheck == bundle) return true;
			}

			return false;
		}
        #endregion
    }
}