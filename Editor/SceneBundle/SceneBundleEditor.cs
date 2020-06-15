//Created by Julien Delaunay, see more on https://github.com/Sorangon/Enhanced-Scene-Manager

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using SorangonToolset.EnhancedSceneManager.CoreEditor.Build;

namespace SorangonToolset.EnhancedSceneManager.CoreEditor {
    /// <summary>
    /// A custom inspector for Scene Bundles, auto reference scenes in build and get their ID
    /// </summary>
    [CustomEditor(typeof(SceneBundle))]
    public class SceneBundleEditor : Editor {
        #region Current
        private bool hasNullReference = false;
        private bool hasSimialarReference = false;
        #endregion

        #region Properties
        private SerializedProperty SceneAssets => sceneAssetsList.serializedProperty;
        #endregion

        #region Serialized Properties
        private ReorderableList sceneAssetsList;
        private SerializedProperty sceneLabels;
        private SerializedProperty description;

        /// <summary>
        /// Initialize the current object inspector
        /// </summary>
        private void FindProperties() {
            SetupSceneAssetsReorderableList();
            sceneLabels = serializedObject.FindProperty("scenes");
            description = serializedObject.FindProperty("description");
        }

        /// <summary>
        /// Setup the scene assets reorderable list display and callbacks
        /// </summary>
        private void SetupSceneAssetsReorderableList() {
            sceneAssetsList = new ReorderableList(serializedObject, serializedObject.FindProperty("sceneAssets"), true, true, true, true) {
                drawHeaderCallback = rect => {
                    EditorGUI.LabelField(rect, "Scenes");
                },

                drawElementCallback = (rect, index, a, h) => {
                    Rect propertyRect = rect;
                    propertyRect.height = EditorGUIUtility.singleLineHeight;
                    GUIContent label = new GUIContent();

                    if(index == 0) {
                        label.text = "Active Scene";
                        EditorUtils.BeginColorField(EditorUtils.validColor);
                    } else {
                        label.text = "Scene " + index;
                    }

                    SceneBundleList currentSceneList = EnhancedSceneManager.GetCurrentSceneList();

                    EditorGUI.BeginChangeCheck();
                    SerializedProperty property = sceneAssetsList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(propertyRect, property, label);

                    if(EditorGUI.EndChangeCheck()) {
                        if(currentSceneList.PersistantScenesBundle != target && (property.objectReferenceValue as SceneAsset).IsPersistantScene()) {
                            Debug.LogWarning("Cannot register persistant scenes");
                            property.objectReferenceValue = null;
                        } else {
                            //Update the generated labels
                            UpdateScenesLabels();

                            //If this scene doesn't exist in the build settings, refresh it
                            if(property.objectReferenceValue != null && !EnhancedSceneBuildManager.IsSceneInBuild(property.objectReferenceValue as SceneAsset)) {
                                EnhancedSceneBuildManager.UpdateBuildScenes();
                            }
                        }

                    }

                    if(index == 0) {
                        EditorUtils.EndColorField();
                    }
                },

                onReorderCallback = list => {
                    UpdateScenesLabels();
                },

                onAddCallback = list => {
                    AddScene();
                },

                onCanRemoveCallback = list => {
                    return list.index != 0;
                },

                onRemoveCallback = list => {
                    SerializedProperty prop = list.serializedProperty;
                    if(prop.GetArrayElementAtIndex(list.index) != null) {
                        prop.DeleteArrayElementAtIndex(list.index);
                    }
                    prop.DeleteArrayElementAtIndex(list.index);
                    UpdateScenesLabels();
                    EnhancedSceneBuildManager.UpdateBuildScenes();
                }
            };
        }
        #endregion

        #region Callbacks
        private void OnEnable() {
            Undo.undoRedoPerformed += OnUndo;
            FindProperties();
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            sceneAssetsList.DoLayoutList();

            if(hasSimialarReference) {
                EditorGUILayout.HelpBox("Similar Scene Asset reference, remove ot or it will deleted once you finished editing this object", MessageType.Warning);
            }

            if(hasNullReference) {
                EditorGUILayout.HelpBox("Missing scene asset, cannot generate scene labels correcty. Ensure that each scene asset references aren't null in the Scene Bundle", MessageType.Error);
            }
            EditorGUILayout.Separator();

            if(sceneLabels.arraySize > 0) {
                DisplayGeneratedLabelsPannel();
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(description);
            
            GUILayout.Space(20f);
            EditorUtils.BeginColorField(EditorUtils.validColor);
            if(GUILayout.Button("Open")) {
                EditorEnhancedSceneManager.OpenSceneBundle(target as SceneBundle);
            }
            EditorUtils.EndColorField();

            if(GUILayout.Button("Add Opened Scenes")) {
                AddOpenedScenes();
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void OnDisable() {
            if(hasNullReference || hasSimialarReference) {
                sceneAssetsList.serializedProperty.CleanNullOrSimilarRefs();
            }
            UpdateScenesLabels();
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            Undo.undoRedoPerformed -= OnUndo;
        }
        #endregion

        #region Pannels     

        /// <summary>
        /// Display all the scenes labels generated
        /// </summary>
        private void DisplayGeneratedLabelsPannel() {
            EditorGUILayout.LabelField("Generated Labels");
            EditorGUILayout.BeginVertical("HelpBox");
            for(int i = 0; i < sceneLabels.arraySize; i++) {
                if(SceneAssets.GetArrayElementAtIndex(i).objectReferenceValue == null) continue;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(sceneLabels.GetArrayElementAtIndex(i).stringValue);
                EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(30f));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Manage Scenes 
        /// <summary>
        /// Add scenes to the list, check if there are scenes in the selection to automaticaly add it to the list
        /// </summary>
        private void AddScene() {
            //Check if Scene Assets are selected
            SceneAsset[] selectedAssets = EditorUtils.GetSelectedObjectsOfType<SceneAsset>();
            bool updateBuildSettingFlag = false;

            if(selectedAssets != null) {
                //Add all selected scenes in the list
                int addOffset = SceneAssets.arraySize > 0 ? 1 : 0;
                int fromIndex = SceneAssets.arraySize - addOffset;
                for(int i = 0; i < selectedAssets.Length; i++) {
                    if(selectedAssets[i].IsPersistantScene()) continue; //Cannot add persistant scenes
                    SceneAssets.InsertArrayElementAtIndex(fromIndex + i);
                    SceneAssets.GetArrayElementAtIndex(fromIndex + addOffset + i).objectReferenceValue = selectedAssets[i];
                    if(!EnhancedSceneBuildManager.IsSceneInBuild(selectedAssets[i]) && !updateBuildSettingFlag) {
                        updateBuildSettingFlag = true;
                    }
                }
            } else {
                if(SceneAssets.arraySize > 0) {
                    SceneAssets.InsertArrayElementAtIndex(SceneAssets.arraySize - 1);
                } else {
                    SceneAssets.InsertArrayElementAtIndex(0);
                }
            }

            if(updateBuildSettingFlag) {
                EnhancedSceneBuildManager.UpdateBuildScenes();
            }

            UpdateScenesLabels();
        }

        /// <summary>
        /// Add the currently loaded scenes to the 
        /// </summary>
        private void AddOpenedScenes() {
            for(int i = 0; i < EditorSceneManager.sceneCount; i++) {
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath(EditorSceneManager.GetSceneAt(i).path, typeof(SceneAsset)) as SceneAsset;
                if(SceneAssets.ContainsObjects(sceneAsset)) continue;
                if(sceneAsset.IsPersistantScene()) continue;

                if(SceneAssets.arraySize > 0) {
                    SceneAssets.InsertArrayElementAtIndex(SceneAssets.arraySize - 1);
                } else {
                    SceneAssets.InsertArrayElementAtIndex(0);
                }

                SceneAssets.GetArrayElementAtIndex(SceneAssets.arraySize - 1).objectReferenceValue = sceneAsset;
                UpdateScenesLabels();
            }
        }

        /// <summary>
        /// Update the scene labels in the bundle
        /// </summary>
        private void UpdateScenesLabels() {
            hasNullReference = false;
            hasSimialarReference = false;

            List<string> generatedLabels = new List<string>();

            for(int i = 0; i < SceneAssets.arraySize; i++) {
                Object asset = SceneAssets.GetArrayElementAtIndex(i).objectReferenceValue;
                if(asset == null) {
                    if(!hasNullReference) {
                        hasNullReference = true;
                    }
                    continue;
                }

                if(generatedLabels.Contains(asset.name)) {
                    if(!hasSimialarReference) {
                        hasSimialarReference = true;
                    }
                    continue;
                }

                generatedLabels.Add(asset.name);
            }

            if(!hasNullReference) {
                GenerateSceneLabels(SceneAssets, sceneLabels, generatedLabels);
            }
        }

        /// <summary>
        /// Generate all labels of the target scene bundle
        /// </summary>
        private static void GenerateSceneLabels(SerializedProperty sceneAssets, SerializedProperty sceneLabels, List<string> generatedLabels) {
            sceneLabels.ClearArray();

            for(int i = 0; i < generatedLabels.Count; i++) {
                sceneLabels.InsertArrayElementAtIndex(i);
                sceneLabels.GetArrayElementAtIndex(i).stringValue = generatedLabels[i];
            }
        }


        /// <summary>
        /// Generate all labales of the target scene bundles
        /// </summary>
        /// <param name="obj"></param>
        public static void GenerateSceneLabels(SerializedObject obj) {
            SerializedProperty assets = obj.FindProperty("sceneAssets");
            assets.CleanNullOrSimilarRefs();
            List<string> sceneLabels = new List<string>();

            for(int i = 0; i < assets.arraySize; i++) {
                sceneLabels.Add(assets.GetArrayElementAtIndex(i).objectReferenceValue.name);
            }

            GenerateSceneLabels(
                assets,
                obj.FindProperty("scenes"),
                sceneLabels
                );

            if(obj.targetObject != null) {
                obj.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        #endregion

        #region Undo
        /// <summary>
        /// On undo callback
        /// </summary>
        private void OnUndo() {
            UpdateScenesLabels();
            Repaint();
        }
        #endregion

        #region Utility
        /// <summary>
        /// Return all scene assets in the target bundle, pass by reflection to limit scene assets array access
        /// </summary>
        /// <param name="targetBundle"></param>
        /// <returns></returns>
        internal static SceneAsset[] GetBundleScenesAssets(SceneBundle targetBundle) {
            if(targetBundle == null) return null;

            SerializedObject tbSerialized = new SerializedObject(targetBundle);
            SerializedProperty scenesProp = tbSerialized.FindProperty("sceneAssets");
            SceneAsset[] assets = new SceneAsset[scenesProp.arraySize];
            for(int i = 0; i < assets.Length; i++) {
                assets[i] = scenesProp.GetArrayElementAtIndex(i).objectReferenceValue as SceneAsset;
            }
            return assets;
        }

        #endregion
    }
}