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
		private static Scene setActiveScene;
		private static bool isLoading = false;
		private static bool isUnloading = false;
		private static LoadingSceneHandler loadingHandler = null;
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
			GameObject handlerGo = new GameObject();
			GameObject.DontDestroyOnLoad(handlerGo);
			loadingHandler = handlerGo.AddComponent<LoadingSceneHandler>();
			loadingHandler.StartCoroutine(LoadingProcessCoroutine(bundle, async));
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

			//Check if persistant scenes are correctly loaded
			if(persistantScenesCount != currentSceneList.PersistantScenesBundle.ScenesCount) {
				//Re-open persistant scenes
				for(int i = 0; i < currentSceneList.PersistantScenesBundle.ScenesCount; i++) {
					SceneManager.LoadScene(currentSceneList.PersistantScenesBundle.GetSceneAtID(i), LoadSceneMode.Additive);
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
			persistantScenesCount = 0;
			//This value will be incremented each persisant scene loaded, will be checked later to get missing persistant scenes
			persistantScenesCount = 0;
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
					persistantScenesCount++; //Count persistant scenes to check if each one is correctly loaded 
				} else {
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
			onSceneAllLoaded?.Invoke();


			Object.Destroy(loadingHandler.gameObject);//Destroy the coroutine handler
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

			AsyncOperation asyncOp;
			asyncOp = SceneManager.LoadSceneAsync(scenes[0], hasToFullyReload ? LoadSceneMode.Single : LoadSceneMode.Additive);

			while(asyncOp.isDone) {
				yield return null;
			}

			setActiveScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1); //Get the loaded scene to set it as active later

			for(int i = 1; i < scenes.Length; i++) {
				asyncOp = SceneManager.LoadSceneAsync(scenes[i], LoadSceneMode.Additive);
				while(asyncOp.isDone) {
					yield return null;
				}
			}

			//Check if persistant scenes are correctly loaded
			if(persistantScenesCount != currentSceneList.PersistantScenesBundle.ScenesCount) {
				//Re-open persistant scenes
				for(int i = 0; i < currentSceneList.PersistantScenesBundle.ScenesCount; i++) {
					asyncOp = SceneManager.LoadSceneAsync(currentSceneList.PersistantScenesBundle.GetSceneAtID(i), LoadSceneMode.Additive);
					while(asyncOp.isDone) {
						yield return null;
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

				while(unload.isDone) {
					yield return null;
				}
			}

			isUnloading = false;
			onSceneAllUnloaded.Invoke();

			yield return null;
		}
        #endregion
    }
}

/// <summary>
/// This class is just a coroutine handler used by the scene manager to use coroutines
/// </summary>
public class LoadingSceneHandler : MonoBehaviour { }
