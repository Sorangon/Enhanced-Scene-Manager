//Created by Julien Delaunay, see more on https://github.com/Sorangon/Enhanced-Scene-Manager

using UnityEngine;
using UnityEditor;

namespace SorangonToolset.EnhancedSceneManager.CoreEditor {
	public class EnhancedSceneManagerWindow : EditorWindow , IHasCustomMenu{

		#region Current
		private Vector2 scrollPos = Vector2.zero;
		private DisplayMode displayMode = DisplayMode.TwoColumns;
		#endregion

		#region Constants
		internal const string DISPLAY_MODE_PREF_ID = "Enhanced Scene Manager Display Mode";
		#endregion

		#region Callbacks
		[MenuItem("Window/Enhanced Scene Manager")]
		static void Init() {
			EnhancedSceneManagerWindow window = GetWindow<EnhancedSceneManagerWindow>("Enhanced Scene Manager");
			window.minSize = new Vector2(350f, 100f);
		}
		#endregion

		#region Enums
		internal enum DisplayMode {
			OneColumn = 1,
			TwoColumns = 2,
			ThreeColumns = 3
		}
		#endregion

		#region Menu
		public void AddItemsToMenu(GenericMenu menu) {
			//TODO : Fix window ratio before enable this
			//menu.AddItem(new GUIContent("One Column"), displayMode == DisplayMode.OneColumn, () => ChangeDisplayMode(DisplayMode.OneColumn));
			//menu.AddItem(new GUIContent("Two Column"), displayMode == DisplayMode.TwoColumns, () => ChangeDisplayMode(DisplayMode.TwoColumns));
			//menu.AddItem(new GUIContent("Three Column"), displayMode == DisplayMode.ThreeColumns, () => ChangeDisplayMode(DisplayMode.ThreeColumns));
		}
		#endregion

		#region GUI Callbacks
		private void OnEnable() {
			displayMode = (DisplayMode)EditorPrefs.GetInt(DISPLAY_MODE_PREF_ID);
		}

		private void OnGUI() {
			EditorGUILayout.Space();
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
			var sceneList = EnhancedSceneManager.GetCurrentSceneList();
			if(sceneList) {
				DisplaySceneBundles(sceneList);
			} else {
				EditorGUILayout.HelpBox("Any current scene list is currently existing. Create a new one",MessageType.Warning);
				if(GUILayout.Button("Create Scene Bundle List")) {
					CreateSceneBundleList();
				}
			}

			EditorGUILayout.Separator();

			DisplayCurrentSceneList();
			EditorGUILayout.EndScrollView();
		}
        #endregion

        #region Scene Bundle & List
		/// <summary>
		/// Creates a new Scene Bundle List in the target bundle
		/// </summary>
		private void CreateSceneBundleList() {
			string path = EditorUtility.SaveFilePanelInProject("Create new Scene Bundle List", "NewSceneBundleList.asset", "asset", "Please enter a name to the Scene Bundle list");

			if(path.Length <= 0) return;

			SceneBundleList newSceneBundleList = SceneBundleList.CreateInstance<SceneBundleList>();
			AssetDatabase.CreateAsset(newSceneBundleList, path);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			EditorEnhancedSceneManager.SetSceneBundleListHasCurrent(newSceneBundleList);
		}
        #endregion

        #region Display Scene Bundles
        /// <summary>
        /// Display the full list of scene bundles
        /// </summary>
        private void DisplaySceneBundles(SceneBundleList targetList) {
			EditorGUILayout.LabelField("Load Scene Bundle");

			int columnsPerRow = (int)displayMode;
			int columnIndex = 0;

			EditorGUILayout.BeginHorizontal();

			for(int i = 0; i < targetList.ScenesBundles.Length; i++) {
				if(targetList.ScenesBundles[i] == null) continue;
				DisplaySceneBundleButton(targetList.ScenesBundles[i]);
				
				columnIndex++;

				if(columnIndex >= columnsPerRow) {
					columnIndex = 0;
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();
				} else {
					GUILayout.Space(10f);
				}
			}
			EditorGUILayout.EndHorizontal();

		}

		/// <summary>
		/// Display the button of a Scene Bundle
		/// </summary>
		private void DisplaySceneBundleButton(SceneBundle bundle) {
			GUIContent content = new GUIContent(bundle.name, bundle.Description);
			if(GUILayout.Button(content, GUILayout.Height(30f))) {
				EditorEnhancedSceneManager.OpenSceneBundle(bundle);
			}

			if(GUILayout.Button("A", GUILayout.Width(20f), GUILayout.Height(30f))) {
				Selection.activeObject = bundle;
				EditorGUIUtility.PingObject(bundle);
			}
		}

		/// <summary>
		/// Change the display mode and save it in the editor prefs
		/// </summary>
		/// <param name="mode"></param>
		private void ChangeDisplayMode(DisplayMode mode) {
			EditorPrefs.SetInt(DISPLAY_MODE_PREF_ID, (int)mode);
			displayMode = mode;
		}
        #endregion

        #region Scene List Setting
		/// <summary>
		/// Display the current scene list field
		/// </summary>
		private void DisplayCurrentSceneList() {
			EditorGUI.BeginChangeCheck();
			SceneBundleList list = EditorGUILayout.ObjectField("Current Bundle List", EnhancedSceneManager.GetCurrentSceneList(), typeof(SceneBundleList), false) as SceneBundleList;

			if(EditorGUI.EndChangeCheck() && list != null) {
				EditorEnhancedSceneManager.SetSceneBundleListHasCurrent(list);
			}
		}
        #endregion
    }
}
