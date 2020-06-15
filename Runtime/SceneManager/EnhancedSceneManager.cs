//Created by Julien Delaunay, see more on https://github.com/Sorangon/Enhanced-Scene-Manager

using System.Collections.Generic;
using System.Collections;
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
		private static SceneBundle currentSceneBundle = null;
		private static Scene setActiveScene;
		private static bool isLoading = false;
		private static bool isUnloading = false;

		private static EnhancedSceneOrchestrator sceneOrchestrator = null;
		#endregion

		#region Properties
		public static SceneBundle CurrentSceneBundle => currentSceneBundle;
        #endregion

        #region Events and Delegates
        public delegate void SceneLoadingAction();
		public static event SceneLoadingAction onTriggerLoading, onSceneAllLoaded, onSceneAllUnloaded;
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

        #region Initialize
		/// <summary>
		/// Clear the cache calues
		/// </summary>
		[RuntimeInitializeOnLoadMethod]
		private static void ClearCache() {
			isLoading = false;
			isUnloading = false;
		}
        #endregion

        #region Scenes Loading
        /// <summary>
        /// Load a new scene bundle and unload the current one
        /// </summary>
        /// <param name="groupName">The target bundle</param>
        public static void LoadSceneBundle(SceneBundle bundle, bool async = false) {
			if(!CanLoad(bundle)) return;
			onTriggerLoading?.Invoke();
			if(sceneOrchestrator == null) InstantiateSceneOrchestrator();
			sceneOrchestrator.StartLoadingCoroutine(LoadingProcessCoroutine(bundle, async)); //Call a start coroutine on the scene oorchestrator
		}
		
		/// <summary>
		/// Load scenes in bundle
		/// </summary>
		private static void LoadScenes(SceneBundle bundle, bool hasToFullyReload, int persistantScenesCount) {

			//Get scenes in the bundle
			var scenes = bundle.GetScenes();

			SceneManager.LoadScene(scenes[0], hasToFullyReload ? LoadSceneMode.Single : LoadSceneMode.Additive);
			setActiveScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1); //Get the loaded scene to set it as active later

			for(int i = 1; i < scenes.Length; i++) {
				SceneManager.LoadScene(scenes[i], LoadSceneMode.Additive);
			}

			if(persistantScenesCount >= 0) {
				//Check if persistant scenes are correctly loaded
				if(persistantScenesCount != currentSceneList.PersistantScenesBundle.ScenesCount) {
					//Re-open persistant scenes
					for(int i = 0; i < currentSceneList.PersistantScenesBundle.ScenesCount; i++) {
						SceneManager.LoadScene(currentSceneList.PersistantScenesBundle.GetSceneAtID(i), LoadSceneMode.Additive);
					}
				}
			}
		}

		/// <summary>
		/// Check if the bundle can be loaded
		/// </summary>
		/// <returns></returns>
		private static bool CanLoad(SceneBundle targetBundle) {
			//Return false if trying to load a bundle that doesn't belong to the current scene list
			if(!currentSceneList.HasBundleInList(targetBundle)) {
				Debug.LogError("This scene bundle doesn't belong to the scene list! It cannot be loaded");
				return false;
			}

			//Return false if trying to load persistant scene bundle
			if(targetBundle == currentSceneList.PersistantScenesBundle) {
				Debug.LogError("Persistant scene bundle is automaticaly and permanently loaded, you cannot load it manually");
				return false;
			}

			//Return false if a bundle is currently loading
			if(isLoading) {
				Debug.LogError("Cannot load another bundle while the Enhanced Scene Manager is loading scenes");
				return false;
			}

			//Return false if a bundle is currently unloading
			if(isUnloading) {
				Debug.LogError("Cannot load another bundle while the Enhanced Scene Manager is unloading scenes");
				return false;
			}

			return true;
		}

		/// <summary>
		/// Returns all the scene that shoud be unloaded
		/// </summary>
		/// <param name="hasToFullyReload"></param>
		/// <param name="persistantScenesCount"></param>
		/// <returns></returns>
		private static List<Scene> GetScenesToUnload(out bool hasToFullyReload, out int persistantScenesCount) {
			hasToFullyReload = false;
			//This value will be incremented each persisant scene loaded, will be checked later to get missing persistant scenes
			persistantScenesCount = currentSceneList.PersistantScenesBundle != null ?  0 : -1; //Pesistant are equal to -1 if any persistant level has been referenced

			var dirtyScenes = new List<Scene>();

			if(persistantScenesCount >= 0) {
				//Filter persistant scenes if there are referenced
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
						persistantScenesCount++; //Count persistant scenes to check if each one is correctly loaded 
					} else {
						dirtyScenes.Add(scene);
					}
				}
			} else {
				//Simply add all scenes if there isn't persistant bundle
				for(int i = 0; i < SceneManager.sceneCount; i++) {
					//Filter persistant scenes
					Scene scene = SceneManager.GetSceneAt(i);
					dirtyScenes.Add(scene);
				}
			}

			//Check if all the scenes will be unload
			hasToFullyReload = SceneManager.sceneCount <= dirtyScenes.Count;
			return dirtyScenes;
		}


        #endregion

        #region Coroutines
		/// <summary>
		/// The loading process coroutine
		/// </summary>
		/// <param name="loadAsync"></param>
		/// <returns></returns>
		private static IEnumerator LoadingProcessCoroutine(SceneBundle bundle, bool loadAsync) {
			List<Scene> scenesToUnload = GetScenesToUnload(out bool hasToFullyReload, out int persistantScenesCount);
			yield return UnloadingAsyncCoroutine(scenesToUnload, hasToFullyReload);

			//Load scenes
			isLoading = true;
			if(loadAsync) {
				yield return LoadingAsyncCoroutine(bundle, hasToFullyReload, persistantScenesCount);
			} else {
				LoadScenes(bundle, hasToFullyReload, persistantScenesCount);
			}

			//Has to wait for a complete frame
			yield return null;

			isLoading = false;
			SceneManager.SetActiveScene(setActiveScene);
			currentSceneBundle = bundle;
			onSceneAllLoaded?.Invoke();

			sceneOrchestrator.StopLoadingCoroutine(); //Stops the coroutine handler
			yield return null;
		}

		/// <summary>
		/// Async loading coroutine
		/// </summary>
		/// <param name="bundle"></param>
		/// <returns></returns>
		private static IEnumerator LoadingAsyncCoroutine(SceneBundle bundle, bool hasToFullyReload, int persistantScenesCount) {
			//Get scenes in the bundle
			var scenes = bundle.GetScenes();

			AsyncOperation loadingOp;
			loadingOp = SceneManager.LoadSceneAsync(scenes[0], hasToFullyReload ? LoadSceneMode.Single : LoadSceneMode.Additive);
			yield return loadingOp;

			setActiveScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1); //Get the loaded scene to set it as active later

			for(int i = 1; i < scenes.Length; i++) {
				loadingOp = SceneManager.LoadSceneAsync(scenes[i], LoadSceneMode.Additive);
				yield return loadingOp;
			}

			if(persistantScenesCount >= 0) {
				//Check if persistant scenes are correctly loaded
				if(persistantScenesCount != currentSceneList.PersistantScenesBundle.ScenesCount) {
					//Re-open persistant scenes
					for(int i = 0; i < currentSceneList.PersistantScenesBundle.ScenesCount; i++) {
						loadingOp = SceneManager.LoadSceneAsync(currentSceneList.PersistantScenesBundle.GetSceneAtID(i), LoadSceneMode.Additive);
						yield return loadingOp;
					}
				}
			}

			yield return null;
		}

		/// <summary>
		/// Async unloading coroutine
		/// </summary>
		/// <param name="bundle"></param>
		/// <returns></returns>
		private static IEnumerator UnloadingAsyncCoroutine(List<Scene> scenesToUnload, bool hasToFullyReload) {
			isUnloading = true;

			//Unload current scene except persistant ones, skip one scene if all scenes have to be reloaded
			for(int i = hasToFullyReload ? 1 : 0; i < scenesToUnload.Count; i++) {
				AsyncOperation unload = SceneManager.UnloadSceneAsync(scenesToUnload[i]);
				yield return unload;
			}

			isUnloading = false;
			onSceneAllUnloaded?.Invoke();

			yield return null;
		}
        #endregion

        #region Scene Orchestrator
		/// <summary>
		/// Instantaite abd initialize a scene orchestrator
		/// </summary>
		private static void InstantiateSceneOrchestrator() {
			EnhancedSceneOrchestrator prefab = Resources.Load<EnhancedSceneOrchestrator>("SceneOrchestrator");
			sceneOrchestrator = Object.Instantiate(prefab);
			Object.DontDestroyOnLoad(sceneOrchestrator);
		}
        #endregion
    }
}
