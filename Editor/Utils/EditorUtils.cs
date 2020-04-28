//Created by Julien Delaunay, see more on https://github.com/Sorangon/Enhanced-Scene-Manager

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SorangonToolset.EnhancedSceneManager.CoreEditor {
    /// <summary>
    /// A utility class for editor that contains utils functions and constant variables
    /// </summary>
    public static class EditorUtils {
        #region Constants
        #region Colors
        public static readonly Color validColor = new Color(0.6f, 0.8f, 0.65f); 
        public static readonly Color deleteColor = new Color(0.8f, 0.5f, 0.5f);
        #endregion
        #endregion

        #region Color Field
        private static Color backupColor;
        private static bool inColorField = false;

        /// <summary>
        /// Begin a color area in Editor GUI
        /// </summary>
        /// <param name="col"></param>
        public static void BeginColorField(Color col) {
            if(!inColorField) { 
                backupColor = GUI.color;
            }

            inColorField = true;
            GUI.color = col;
        }

        /// <summary>
        /// End a Editor GUI color field, reapply the base editor color
        /// </summary>
        public static void EndColorField() {
            inColorField = false;
            GUI.color = backupColor;
        }
        #endregion

        #region Selection
        /// <summary>
        /// Return all selected object of a type. Return null if any are selected
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static T[] GetSelectedObjectsOfType<T>() where T : Object {
            if(Selection.objects.Length <= 0) return null;

            List<T> selectedT = new List<T>();
            foreach(Object selected in Selection.objects) {
                if(selected is T) {
                    selectedT.Add(selected as T);
                }
            }

            if(selectedT.Count > 0) {
                return selectedT.ToArray();
            } else {
                return null;
            }
        }
        #endregion

        #region Scene 
        /// <summary>
        /// Return true if the scene belongs to the persistant scene bundle
        /// </summary>
        /// <param name="scene"></param>
        /// <returns></returns>
        public static bool IsPersistantScene(this SceneAsset scene) {
            if(scene == null) return false;
            SceneBundleList currentList = EnhancedSceneManager.GetCurrentSceneList();
            if(currentList == null) return false;
            if(currentList.PersistantScenesBundle == null) return false;

            for(int i = 0; i < currentList.PersistantScenesBundle.ScenesCount; i++) {
                if(currentList.PersistantScenesBundle.ContainsScene(scene.name)) return true;
            }

            return false;
        }
        #endregion

        #region Enumerables
        /// <summary>
        /// Check for similar or null references in the Seiralized property
        /// </summary>
        /// <param name="property"></param>
        /// <param name="hasNullRef"></param>
        /// <param name="hasSimialRefs"></param>
        public static void CheckForNullOrSimilarRefs(this SerializedProperty property, ref bool hasNullRef, ref bool hasSimialRefs) {
            hasNullRef = false;
            hasSimialRefs = false;

            if(!property.isArray) {
                hasNullRef = property.objectReferenceValue != null;
                return; 
            }

            for(int i = 0; i < property.arraySize; i++) {
                Object refToCheck = property.GetArrayElementAtIndex(i).objectReferenceValue;
                if(refToCheck != null) {
                    if(!hasSimialRefs) {
                        if(property.ContainsObjects(refToCheck, 2)) {
                            hasSimialRefs = true;
                        }
                    }
                } else {
                    if(!hasNullRef) hasNullRef= true;
                }
            }
        }

        /// <summary>
        /// Clean every null or simial reference in the serialized property array
        /// </summary>
        /// <param name="property"></param>
        public static void CleanNullOrSimilarRefs(this SerializedProperty property) {
            if(!property.isArray) return;
            List<Object> objectHistory = new List<Object>();
            List<int> elementsToDeleteId = new List<int>();

            //Check for null references or similar id
            for(int i = 0; i < property.arraySize; i++) {
                Object current = property.GetArrayElementAtIndex(i).objectReferenceValue;
                if(current != null && !objectHistory.Contains(current)) {
                    objectHistory.Add(current);
                } else {
                    elementsToDeleteId.Add(i);
                }
            }

            //Remove unwanted elements
            for(int i = elementsToDeleteId.Count - 1; i >= 0; i--) {
                if(property.GetArrayElementAtIndex(elementsToDeleteId[i]).objectReferenceValue != null) {
                    property.DeleteArrayElementAtIndex(elementsToDeleteId[i]);
                }
                property.DeleteArrayElementAtIndex(elementsToDeleteId[i]);
            }
        }

        /// <summary>
        /// Return true if this serialized property array contain the object reference
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool ContainsObjects(this SerializedProperty property, Object obj, int objectInstances = 1) {
            if(!property.isArray) return obj.Equals(property.objectReferenceValue);

            for(int i = 0; i < property.arraySize; i++) {
                if(property.GetArrayElementAtIndex(i).objectReferenceValue == obj) {
                    objectInstances--;
                    if(objectInstances <= 0) {
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion
    }
}
