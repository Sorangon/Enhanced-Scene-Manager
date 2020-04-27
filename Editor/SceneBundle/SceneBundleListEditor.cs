//Created by Julien Delaunay, see more on https://github.com/Sorangon/Enhanced-Scene-Manager

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using SorangonToolset.EnhancedSceneManager.CoreEditor.Build;

namespace SorangonToolset.EnhancedSceneManager.CoreEditor {
    /// <summary>
    /// The custom inspector drawer class of the Scene List asset, refresh the build setting scenes for each scene added
    /// </summary>
    [CustomEditor(typeof(SceneBundleList))]
    public class SceneBundleListEditor : Editor{
        #region Current
        private bool hasSimilarReferences = false;
        private bool hasNullReferences = false;
        private bool updateBuildSettingsFlag = false;
        #endregion

        #region Serialized Properties
        private SerializedProperty persistantSceneBundle;
        private ReorderableList sceneBundleList;
        
        /// <summary>
        /// Bind serialized properties
        /// </summary>
        private void FindProperties() {
            persistantSceneBundle = serializedObject.FindProperty("persistantScenesBundle");
            SetupSceneBundleReorderableList();
        }

        /// <summary>
        /// Setup the the behaviour and displayed parameters of the levels reorderable list
        /// </summary>
        private void SetupSceneBundleReorderableList() {
            sceneBundleList = new ReorderableList(serializedObject, serializedObject.FindProperty("scenesBundles"), true, true, true, true) {

                drawHeaderCallback = rect => {
                    EditorGUI.LabelField(rect, "Scene Bundles");
                },

                //Draw the inspector and fields of a level bundle container
                drawElementCallback = (rect, index, a, h) => {
                    Rect propertyRect = rect;
                    rect.height = EditorGUIUtility.singleLineHeight;
                    GUIContent label = new GUIContent();

                    if(index == 0) {
                        label.text = "Default Bundle";
                        EditorUtils.BeginColorField(EditorUtils.validColor);
                    } else {
                        label.text = "Bundle " + index;
                    }

                    EditorGUI.PropertyField(rect, sceneBundleList.serializedProperty.GetArrayElementAtIndex(index),label);

                    if(index == 0) {
                        EditorUtils.EndColorField();
                    }
                },

                //
                //Filter if scene bundles are selected to automaticaly add it
                onAddCallback = list => {
                    AddSceneBundle(list.serializedProperty);
                },

                //Check if there are more than one level to enable delete button
                onCanRemoveCallback = list => {
                    return list.count > 1;
                },

                //Delete the entire slot when remove
                onRemoveCallback = list => {
                    if(list.serializedProperty.GetArrayElementAtIndex(list.index) != null) {
                        list.serializedProperty.DeleteArrayElementAtIndex(list.index);
                    }
                    list.serializedProperty.DeleteArrayElementAtIndex(list.index);

                    //Update build scenes
                    if(target == EnhancedSceneManager.GetCurrentSceneList()) {
                        updateBuildSettingsFlag = true;
                    }
                }
            };
        }
        #endregion

        #region Callbacks
        private void OnEnable() {
            FindProperties();
            CheckBundles();
            Undo.undoRedoPerformed += OnUndo;
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            if(updateBuildSettingsFlag) updateBuildSettingsFlag = false;

            EditorGUILayout.PropertyField(persistantSceneBundle);

            GUILayout.Space(8f);
            EditorGUI.BeginChangeCheck();
            sceneBundleList.DoLayoutList();
            if(EditorGUI.EndChangeCheck()) {
                updateBuildSettingsFlag = true;
            }

            EditorGUILayout.Separator();

            if(hasSimilarReferences) {
                EditorGUILayout.HelpBox("Similar references! There will be automatically removed once you stop editing this object", MessageType.Warning);
            }

            if(hasNullReferences) {
                EditorGUILayout.HelpBox("Null references! There will be automatically removed once you stop editing this object", MessageType.Error);
            }

            if(EnhancedSceneManager.GetCurrentSceneList() == target) {
                EditorGUILayout.HelpBox("Current Scene Bundle List", MessageType.None);
            } else {
                if(GUILayout.Button("Set as current")) {
                    EditorEnhancedSceneManager.SetSceneBundleListHasCurrent(target as SceneBundleList);
                }
            }

            serializedObject.ApplyModifiedProperties();
            if(updateBuildSettingsFlag) {
                UpdateBuildSettings();
            }
        }

        private void OnDisable() {
            //Cleans
            if(hasNullReferences || hasSimilarReferences) {
                sceneBundleList.serializedProperty.CleanNullOrSimilarRefs();
                serializedObject.ApplyModifiedProperties();
                UpdateBuildSettings();
            }
            Undo.undoRedoPerformed -= OnUndo;
        }
        #endregion

        #region Scenes

        /// <summary>
        /// Add a scene bundle to the current list
        /// </summary>
        private void AddSceneBundle(SerializedProperty listArray) {
            SceneBundle[] selectedBundles = EditorUtils.GetSelectedObjectsOfType<SceneBundle>();
            if(selectedBundles != null) {
                for(int i = 0; i < selectedBundles.Length; i++) {
                    if(listArray.arraySize > 0) {
                        listArray.InsertArrayElementAtIndex(listArray.arraySize - 1);
                    } else {
                        listArray.InsertArrayElementAtIndex(0);
                    }
                    listArray.GetArrayElementAtIndex(listArray.arraySize - 1).objectReferenceValue = selectedBundles[i];

                }

                //Update build scenes only if new scenes are added
                if(target == EnhancedSceneManager.GetCurrentSceneList()) {
                    updateBuildSettingsFlag = true;
                }

            } else {
                //Just add an element
                if(listArray.arraySize > 0) {
                    listArray.InsertArrayElementAtIndex(listArray.arraySize - 1);
                } else {
                    listArray.InsertArrayElementAtIndex(0);
                    
                }
            }

            CheckBundles();
        }

        /// <summary>
        /// Checks all referenced bundles for similar or missing refs
        /// </summary>
        private void CheckBundles() {
            sceneBundleList.serializedProperty.CheckForNullOrSimilarRefs(ref hasNullReferences, ref hasSimilarReferences);
        }

        /// <summary>
        /// Update levels in build settings
        /// </summary>
        private void UpdateBuildSettings() {
            CheckBundles();

            if(!hasNullReferences) {
                EnhancedSceneBuildManager.UpdateBuildScenes();
            }
        }
        #endregion

        #region Undo
        /// <summary>
        /// On undo callback
        /// </summary>
        private void OnUndo() {
            updateBuildSettingsFlag = true;
        }
        #endregion
    }
}