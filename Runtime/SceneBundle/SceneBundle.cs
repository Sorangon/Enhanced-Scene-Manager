//Created by Julien Delaunay, see more on https://github.com/Sorangon/Enhanced-Scene-Manager

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SorangonToolset.EnhancedSceneManager {

	/// <summary>
	/// Scene Bundle regroup multiple unity scenes, can be loaded from EnhancedSceneManager
	/// </summary>
	[CreateAssetMenu(menuName = "Enhanced Scene Manager/Scene Bundle", fileName = "NewSceneBundle", order = 150)]
	public class SceneBundle : ScriptableObject {
		#region Data
		[SerializeField] private string[] scenes = new string[1];
		[SerializeField, HideInInspector, TextArea] private string description = string.Empty;
        #endregion

#if UNITY_EDITOR
        #region Editor Data
        [SerializeField, HideInInspector] internal SceneAsset[] sceneAssets = new SceneAsset[1];
        #endregion
#endif

        #region Properties
        public int ScenesCount => scenes.Length;
		public string Description => description;
		#endregion

		#region Callbacks
#if UNITY_EDITOR
		private void Reset() {
			scenes = new string[1];
			sceneAssets = new SceneAsset[1];
		}
#endif
        #endregion

        #region Utils
		/// <summary>
		/// Load this scene bundle
		/// </summary>
		public void Load() {
			EnhancedSceneManager.LoadSceneBundle(this);
		}
        #endregion

        #region Scene Labels
        /// <summary>
        /// Return the scene label at a specific index
        /// </summary>
        /// <param name="index"></param>
        public string GetSceneAtID(int index) {
			return scenes[index];
		}

		/// <summary>
		/// Returns the scenes labels of the bundle
		/// </summary>
		/// <returns></returns>
		public string[] GetScenes() {
			return scenes.Clone() as string[];
		}
        #endregion

		#region Utils
		/// <summary>
		/// Returns true if the bundle contains the input scene
		/// </summary>
		/// <param name="sceneName"></param>
		/// <returns></returns>
		public bool ContainsScene(string sceneName) {
#if UNITY_EDITOR
			foreach(SceneAsset sceneAsset in sceneAssets) {
				if(sceneAsset.name == sceneName) {
					return true;
				}
			}
#endif
			foreach(string sn in scenes) {
				if(sn == sceneName) {
					return true;
				}
			}

			return false;
		}
		#endregion
	}
}
