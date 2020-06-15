//Created by Julien Delaunay, see more on https://github.com/Sorangon/Enhanced-Scene-Manager

using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using SorangonToolset.EnhancedSceneManager.CoreEditor.Build;

namespace SorangonToolset.EnhancedSceneManager.CoreEditor {
    /// <summary>
    /// The editor scene manager to manage scene bundles in editor
    /// </summary>
    public static class EditorEnhancedSceneManager{
		#region Properties
		private static SceneBundleList CurrentSceneList => EnhancedSceneManager.GetCurrentSceneList();
		#endregion

		#region Editor Scene Loading
		/// <summary>
		/// Load a new scene group and unload the current one by group reference, editor only
		/// </summary>
		/// <param name="groupName">The target group</param>
		public static void OpenSceneBundle(SceneBundle newBundle) {
			//This value will be incremented each persisant scene loaded, will be checked later to get missing persistant scenes
			var persistantScenesCheckCount = 0;

			//Register dirty scenes
			var dirtyScenes = new List<Scene>();

			var saveFlag = false;

			SceneAsset[] persistantScenesAssets = SceneBundleEditor.GetBundleScenesAssets(CurrentSceneList.PersistantScenesBundle);
			bool hasPersistantBundle = persistantScenesAssets != null;
			for (int i = 0; i < EditorSceneManager.sceneCount; i++) {
				//Filter persistant scenes
				Scene scene = EditorSceneManager.GetSceneAt(i);

				//Check if opended scenes are dirty to call a save pannel
				if (scene.isDirty && !saveFlag) {
					saveFlag = true;
				}

				//Check if iterated scene is persisant
				var persisantFlag = false;
				if(hasPersistantBundle) {
					for (int j = 0; j < persistantScenesAssets.Length; j++) {
						if (scene.name == persistantScenesAssets[j].name) {
							persisantFlag = true;
						}
					}
				}

				if (!persisantFlag) {
					dirtyScenes.Add(scene);
				}
				else {
					//Doesn't add persistant scenes to dirty list
					persistantScenesCheckCount++; //Count persistant scenes to check if each one is correctly loaded 				
				}
			}

			//Asks the user to save the scenes if a dirty one is detected
			if (saveFlag) {
				EditorSceneManager.SaveModifiedScenesIfUserWantsTo(dirtyScenes.ToArray());
			}

			//Check if all the scenes will be unload
			var fullLoad = EditorSceneManager.sceneCount <= dirtyScenes.Count;

			//Unload current scene except persistant ones, skip one scene if all scenes have to be reloaded
			for (int i = fullLoad ? 1 : 0; i < dirtyScenes.Count; i++) {
				EditorSceneManager.CloseScene(dirtyScenes[i], true);
			}


			//Load scenes in the bundle
			SceneAsset[] newBundleSceneAssets = SceneBundleEditor.GetBundleScenesAssets(newBundle);
			var scenesPaths = new string[newBundleSceneAssets.Length];

			for (int i = 0; i < newBundleSceneAssets.Length; i++) {
				scenesPaths[i] = AssetDatabase.GetAssetPath(newBundleSceneAssets[i]);
			}

			//Threat first scene
			var firstSceneLoadMode = fullLoad ? OpenSceneMode.Single : OpenSceneMode.Additive;
			Scene active = EditorSceneManager.OpenScene(scenesPaths[0], firstSceneLoadMode);

			for (int i = 1; i < scenesPaths.Length; i++) {
				EditorSceneManager.OpenScene(scenesPaths[i], OpenSceneMode.Additive);
			}

			//Check if persistant scenes are correctly loaded
			if (hasPersistantBundle && persistantScenesCheckCount != persistantScenesAssets.Length) {
				//Re-open persistant scenes
				for (int i = 0; i < persistantScenesAssets.Length; i++) {
					var scenePath = AssetDatabase.GetAssetPath(persistantScenesAssets[i]);
					Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
					EditorSceneManager.MoveSceneBefore(scene, EditorSceneManager.GetSceneAt(i));
				}
			}

			//Active the first loaded scene of the bundle
			EditorSceneManager.SetActiveScene(active);
		}
        #endregion

        #region Manage Scene List
		/// <summary>
		/// Set a scene bundle list has current in the application
		/// </summary>
		/// <param name="list"></param>
		public static void SetSceneBundleListHasCurrent(SceneBundleList list) {
			Type sceneBundleListType = typeof(EnhancedSceneManager);
			MethodInfo setCurrentSceneListMethod = sceneBundleListType.GetMethod("SetCurrentSceneList", BindingFlags.NonPublic | BindingFlags.Static);
			setCurrentSceneListMethod.Invoke(null, new object[] {list});
			EnhancedSceneBuildManager.UpdateBuildScenes();
			
		}
        #endregion
    }
}
