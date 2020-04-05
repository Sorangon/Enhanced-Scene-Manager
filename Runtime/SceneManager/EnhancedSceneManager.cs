//Created by Julien Delaunay, see more on https://github.com/Sorangon/Enhanced-Scene-Manager

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SorangonToolset.EnhancedSceneManager.Internal;

namespace SorangonToolset.EnhancedSceneManager {
	/// <summary>
	///Manage the scene groups, refers the current used scene list 
	/// </summary>
	public static class EnhancedSceneManager {
		#region Current
		private static SceneBundleList currentSceneList = null;
		private static Scene setActiveScene;
        #endregion

        #region Manage Scene List
		/// <summary>
		/// Returns the current used scene list
		/// </summary>
		/// <returns></returns>
		public static SceneBundleList GetCurrentSceneList() {
			if(currentSceneList == null) {
				currentSceneList = StartupSceneManagerSetting.GetCurrentSceneList();
			}

			return currentSceneList;
		}

		/// <summary>
		/// Sets the current scene list
		/// </summary>
		internal static void SetCurrentSceneList(SceneBundleList sceneList) {
			currentSceneList = sceneList;

#if UNITY_EDITOR
			//Update startup manager setting
			StartupSceneManagerSetting.Instance.CurrentSceneList = currentSceneList;
#endif
		}

		#endregion

		#region Scenes Loading
		/// <summary>
		/// Load a new scene group and unload the current one by group reference
		/// </summary>
		/// <param name="groupName">The target group</param>
		public static void LoadSceneBundle(SceneBundle bundle, bool async = false, LoadSceneMode loadMode = LoadSceneMode.Single) {
			//Return if trying to load a bundle that doesn't belong to the current scene list
			if(!currentSceneList.HasBundleInList(bundle)) {
				Debug.LogError("This scene bundle doesn't belong to the scene list! It cannot be loaded");
				return;
			}

			//Return if trying to load persistant scene bundle
			if(bundle == currentSceneList.PersistantScenesBundle) {
				Debug.LogError("Persistant scene bundle is automaticaly and permanently loaded, you cannot load it manually");
				return;
			}

			SceneManager.sceneLoaded += OnSceneLoaded;

			//This value will be incremented each persisant scene loaded, will be checked later to get missing persistant scenes
			var persistantScenesCheckCount = 0;
			var dirtyScenes = new List<Scene>();

			for(int i = 0; i < SceneManager.sceneCount; i++) {
				//Filter persistant scenes
				Scene scene = SceneManager.GetSceneAt(i);

				//Check if iterated scene is persisant
				var persisantFlag = false;
				for(int j = 0; j < currentSceneList.PersistantScenesBundle.ScenesCount; j++) {
					if(scene.name == currentSceneList.PersistantScenesBundle.GetSceneAtID(j)) {
						persisantFlag = true;
					}
				}

				if(persisantFlag) {
					//Doesn't add persistant scenes to dirty list
					persistantScenesCheckCount++; //Count persistant scenes to check if each one is correctly loaded 
				} else {
					dirtyScenes.Add(scene);
				}
			}

			//Check if all the scenes will be unload
			var fullLoad = SceneManager.sceneCount <= dirtyScenes.Count;

			//Unload current scene except persistant ones, skip one scene if all scenes have to be reloaded
			for(int i = fullLoad ? 1 : 0; i < dirtyScenes.Count; i++) {
				SceneManager.UnloadSceneAsync(dirtyScenes[i]);
			}

			//Get scenes in the bundle
			var scenes = bundle.GetScenes();

			SceneManager.LoadScene(scenes[0], fullLoad ? LoadSceneMode.Single : LoadSceneMode.Additive);
			setActiveScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1); //Get the loaded scene to set it as active later

			for(int i = 1; i < scenes.Length; i++) {
				SceneManager.LoadScene(scenes[i], LoadSceneMode.Additive);
			}

			//Check if persistant scenes are correctly loaded
			if(persistantScenesCheckCount != currentSceneList.PersistantScenesBundle.ScenesCount) {
				//Re-open persistant scenes
				for(int i = 0; i < currentSceneList.PersistantScenesBundle.ScenesCount; i++) {
					SceneManager.LoadScene(currentSceneList.PersistantScenesBundle.GetSceneAtID(i), LoadSceneMode.Additive);
				}
			}
		}

		private static void OnSceneLoaded(Scene scene, LoadSceneMode loadMode) {
			if(scene == setActiveScene) {
				SceneManager.SetActiveScene(setActiveScene);
				SceneManager.sceneLoaded -= OnSceneLoaded;
			}
		}
        #endregion
    }
}