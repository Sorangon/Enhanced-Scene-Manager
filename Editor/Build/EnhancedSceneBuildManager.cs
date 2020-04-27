//Created by Julien Delaunay, see more on https://github.com/Sorangon/Enhanced-Scene-Manager

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace SorangonToolset.EnhancedSceneManager.CoreEditor.Build {
    public class EnhancedSceneBuildManager : IPreprocessBuildWithReport{
        #region Singleton
        /// <summary>
        /// Create an instance only to preprocess build and check Scene list and bundles datas
        /// </summary>
        private static EnhancedSceneBuildManager instance = null;

        /// <summary>
        /// An instance should always exist
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        private static void Initialize() {
            instance = new EnhancedSceneBuildManager();
        }
        #endregion

        #region Update Build Scenes
        /// <summary>
        /// Update all build scenes, check if 
        /// </summary>
        public static void UpdateBuildScenes() {
            SceneBundleList currentSceneList = EnhancedSceneManager.GetCurrentSceneList();
            if(currentSceneList == null) return;

            //Create the container of build scenes
            var buildSettingsScenes = new List<EditorBuildSettingsScene>();

            //Add Startup Scene
            SceneAsset startupSceneAsset = Resources.Load<SceneAsset>("StartupScene");
            var startupScene = new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(startupSceneAsset), true);
            buildSettingsScenes.Add(startupScene);

            bool BuildContainsScene(string path) {
                for(int i = 0; i < buildSettingsScenes.Count; i++) {
                    if(buildSettingsScenes[i].path == path) return true;
                }
                return false;
            }

            //Add persistant scenes
            if(currentSceneList.PersistantScenesBundle != null) {
                SceneAsset[] peristantScenes = SceneBundleEditor.GetBundleScenesAssets(currentSceneList.PersistantScenesBundle);
                foreach(SceneAsset sceneAsset in peristantScenes) {
                    var persistantBuildScene = new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(sceneAsset), true);

                    if(!BuildContainsScene(persistantBuildScene.path)) {
                        buildSettingsScenes.Add(persistantBuildScene);
                    }
                }
            }

            //Add scenes
            foreach(SceneBundle bundle in currentSceneList.ScenesBundles) {
                SceneAsset[] sceneAssets = SceneBundleEditor.GetBundleScenesAssets(bundle);
                foreach(SceneAsset sceneAsset in sceneAssets) {
                    var buildScene = new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(sceneAsset), true);

                    if(!BuildContainsScene(buildScene.path)) {
                        buildSettingsScenes.Add(buildScene);
                    }
                }
            }

            //Replace the current build setting scenes with new list
            EditorBuildSettings.scenes = buildSettingsScenes.ToArray();
        }

        /// <summary>
        /// Check if the input scene is a build setting scene
        /// </summary>
        /// <returns></returns>
        public static bool IsSceneInBuild(SceneAsset scene) {
            string registeredScenePath = AssetDatabase.GetAssetPath(scene);
            string[] scenesPaths = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);

            for(int i = 0; i < scenesPaths.Length; i++) {
                if(registeredScenePath == scenesPaths[i]) {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Manage Datas
        /// <summary>
        /// Check the scene data and scene bundles
        /// </summary>
        /// <returns></returns>
        private static void CheckDatas() {
            SceneBundleList currentSceneList = EnhancedSceneManager.GetCurrentSceneList();
            List<SerializedProperty> propertiesToCheck = new List<SerializedProperty>();

            if(currentSceneList.PersistantScenesBundle != null) {
                SerializedObject sceneBundleObject = new SerializedObject(currentSceneList.PersistantScenesBundle);
                sceneBundleObject.FindProperty("sceneAssets").CleanNullOrSimilarRefs();
            }

            SerializedObject sceneListSO = new SerializedObject(currentSceneList);
            SerializedProperty sceneBundlesProperty = sceneListSO.FindProperty("scenesBundles");
            for(int i = 0; i < sceneBundlesProperty.arraySize; i++) {
                SceneBundleEditor.GenerateSceneLabels(new SerializedObject(sceneBundlesProperty.GetArrayElementAtIndex(i).objectReferenceValue));
            }

            //Reupdate the build scenes
            UpdateBuildScenes();
        }
        #endregion

        #region Preprocess Builds
        public int callbackOrder => -500;

        public void OnPreprocessBuild(BuildReport report) {
            CheckDatas();
        }
        #endregion
    }
}
