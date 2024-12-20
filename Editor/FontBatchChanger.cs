using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TextMeshProMax.Editor
{
    public class FontBatchChanger : EditorWindow
    {
        private string _prefabRootFolder = "Assets";
        private List<string> _allScenePaths = new();
        private readonly Dictionary<string, bool> _sceneSelection = new();

        private TMP_FontAsset _newFontAsset;
        private TMP_FontAsset _filterFont;

        private bool _showFontGrouping;
        private Vector2 _windowScrollPosition;
        private Vector2 _sceneScrollPosition;

        private GUIStyle _boxStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _sectionStyle;

        private readonly Dictionary<TMP_FontAsset, Color> _fontColors = new();
        private int _colorIndex;

        private bool _searchFromPrefabs;
        private bool _searchFromScenes;

        private class TMPTextInfo
        {
            public string Path;
            public string SceneName;
            public bool IsCurrentScene => SceneName == SceneManager.GetActiveScene().name;
            public bool IsSceneLoaded => SceneManager.GetSceneByPath(Path).isLoaded;
            public readonly List<FoundTMP> FoundTMPs = new();
        }

        private class FoundTMP
        {
            public TMPTextInfo Mother;
            public TMP_Text TMPText;
            public bool Selected = true;
            public string RelativePath;
        }

        private readonly List<TMPTextInfo> _prefabResults = new();
        private readonly List<TMPTextInfo> _sceneResults = new();
        private readonly Dictionary<TMP_FontAsset, List<FoundTMP>> _fontGroups = new();
        private readonly Dictionary<object, bool> _foldoutStates = new();
        private Scene _initialActiveScene;

        [MenuItem("Tools/TextMeshPro Max/TMP Font Batch Changer")]
        public static void ShowWindow()
        {
            GetWindow<FontBatchChanger>("TMP Font Batch Changer").Show();
        }

        private void OnEnable()
        {
            _initialActiveScene = SceneManager.GetActiveScene();
            var sceneGuids = AssetDatabase.FindAssets("t:Scene");
            _allScenePaths = sceneGuids.Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => !p.StartsWith("Packages/"))
                .ToList();

            foreach (var sp in _allScenePaths)
            {
                if (!_sceneSelection.ContainsKey(sp))
                    _sceneSelection[sp] = false;
            }

            InitializeStyles();
        }

        private void InitializeStyles()
        {
            _boxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(0, 0, 5, 5)
            };

            _headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                margin = new RectOffset(0, 0, 10, 10)
            };

            _sectionStyle = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold
            };
        }

        private void OnGUI()
        {
            if (_boxStyle == null)
            {
                InitializeStyles();
            }
            
            _windowScrollPosition = EditorGUILayout.BeginScrollView(_windowScrollPosition);

            EditorGUILayout.BeginVertical(_boxStyle);
            EditorGUILayout.LabelField("TMP Font Batch Changer", _headerStyle);

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical(_boxStyle);
            EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);

            _newFontAsset =
                (TMP_FontAsset)EditorGUILayout.ObjectField("New Font Asset", _newFontAsset, typeof(TMP_FontAsset),
                    false);

            EditorGUILayout.Space(5);

            // Prefab and Scene Search Configuration
            _searchFromPrefabs = EditorGUILayout.Toggle("Search from Prefabs", _searchFromPrefabs);
            if (_searchFromPrefabs)
            {
                // Box for Search from Prefabs options
                EditorGUILayout.BeginVertical(_boxStyle);
                EditorGUILayout.BeginHorizontal();
                
                EditorGUILayout.LabelField("Prefab Root Folder:", GUILayout.Width(150));
                EditorGUILayout.LabelField(_prefabRootFolder, EditorStyles.textField);
                if (GUILayout.Button("Browse", GUILayout.Width(60)))
                {
                    string selectedPath =
                        EditorUtility.OpenFolderPanel("Select Prefab Root Folder", Application.dataPath, "");
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        if (selectedPath.StartsWith(Application.dataPath))
                        {
                            selectedPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                        }

                        _prefabRootFolder = selectedPath;
                    }
                }
                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            _searchFromScenes = EditorGUILayout.Toggle("Search from Scenes", _searchFromScenes);
            if (_searchFromScenes)
            {
                // Configuration 박스를 감싸는 상위 레이아웃이나 해당 영역을 ExpandHeight(true)로 설정
                // 예: EditorGUILayout.BeginVertical(_boxStyle, GUILayout.ExpandHeight(true));

                EditorGUILayout.BeginVertical(_boxStyle, GUILayout.ExpandHeight(true));
                EditorGUILayout.LabelField("Select Scenes to Search", EditorStyles.boldLabel);
                EditorGUILayout.Space(5);

                // ScrollViewScope에도 ExpandHeight(true)를 적용
                using (var scrollView = new EditorGUILayout.ScrollViewScope(_sceneScrollPosition, GUILayout.ExpandHeight(true)))
                {
                    _sceneScrollPosition = scrollView.scrollPosition;

                    foreach (var sp in _allScenePaths)
                    {
                        EditorGUILayout.BeginVertical(_boxStyle);
                        EditorGUILayout.BeginHorizontal();

                        _sceneSelection[sp] = EditorGUILayout.Toggle(_sceneSelection[sp], GUILayout.Width(20));

                        GUIStyle sceneNameStyle = new GUIStyle(EditorStyles.label)
                        {
                            wordWrap = true
                        };
                        EditorGUILayout.LabelField(Path.GetFileNameWithoutExtension(sp), sceneNameStyle);

                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                    }
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);
            if (GUILayout.Button("Search TMP_Text Components", GUILayout.Height(30)))
            {
                SearchAll();
            }

            if (_prefabResults.Count > 0 || _sceneResults.Count > 0)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical(_boxStyle);
                EditorGUILayout.LabelField("Search Results", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                _showFontGrouping = EditorGUILayout.Toggle("Group by Font", _showFontGrouping);
                if (_showFontGrouping && _fontGroups.Count > 0)
                {
                    _filterFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Filter by Font:", _filterFont,
                        typeof(TMP_FontAsset), false);
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(5);
                if (_showFontGrouping)
                {
                    DrawFontGrouping();
                }
                else
                {
                    DrawSceneAndPrefabResults();
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(10);
                EditorGUI.BeginDisabledGroup(_newFontAsset == null);
                if (GUILayout.Button("Apply New Font Asset", GUILayout.Height(30)))
                {
                    ApplyChanges();
                }

                EditorGUI.EndDisabledGroup();

                if (_newFontAsset == null)
                {
                    EditorGUILayout.HelpBox("Please assign a New Font Asset to apply.", MessageType.Warning);
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }


        private void SearchAll()
        {
            _prefabResults.Clear();
            _sceneResults.Clear();
            _fontGroups.Clear();
            _fontColors.Clear();
            _colorIndex = 0;

            // Prefab search
            if (_searchFromPrefabs)
            {
                string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { _prefabRootFolder });
                foreach (string guid in prefabGuids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab != null)
                    {
                        var texts = prefab.GetComponentsInChildren<TMP_Text>(true);
                        if (texts.Length > 0)
                        {
                            var info = new TMPTextInfo() { Path = path };
                            foreach (var t in texts)
                            {
                                var foundTMP = new FoundTMP
                                {
                                    Mother = info,
                                    TMPText = t,
                                    Selected = true,
                                    RelativePath = GetGameObjectPath(t.gameObject, prefab.transform)
                                };
                                info.FoundTMPs.Add(foundTMP);
                                AddToFontGroup(t, foundTMP);
                            }

                            _prefabResults.Add(info);
                        }
                    }
                }
            }

            // Scene search
            if (_searchFromScenes)
            {
                var selectedScenes = _sceneSelection.Where(s => s.Value).Select(s => s.Key).ToList();
                foreach (var scenePath in selectedScenes)
                {
                    var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                    var texts = Resources.FindObjectsOfTypeAll<TMP_Text>().Where(t => t.gameObject.scene == scene)
                        .ToArray();

                    if (texts.Length > 0)
                    {
                        var info = new TMPTextInfo()
                        {
                            Path = scenePath,
                            SceneName = Path.GetFileNameWithoutExtension(scenePath)
                        };

                        foreach (var t in texts)
                        {
                            var foundTMP = new FoundTMP
                            {
                                Mother = info,
                                TMPText = t,
                                Selected = true,
                                RelativePath = GetSceneObjectPath(t.gameObject)
                            };
                            info.FoundTMPs.Add(foundTMP);
                            AddToFontGroup(t, foundTMP);
                        }

                        _sceneResults.Add(info);
                    }

                    // If it is not only loaded scene, close it
                    if (!IsOnlyLoadedScene(scene))
                    {
                        EditorSceneManager.CloseScene(scene, true);
                    }
                }
            }
        }
        
        private bool IsOnlyLoadedScene(Scene scene)
        {
            var loadedSceneCount = 0;
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var s = SceneManager.GetSceneAt(i);
                if (s.isLoaded)
                {
                    loadedSceneCount++;
                }
            }

            return loadedSceneCount == 1;
        }

        private void DrawSceneAndPrefabResults()
        {
            if (_prefabResults.Count > 0)
            {
                EditorGUILayout.BeginVertical(_boxStyle);
                EditorGUILayout.LabelField("Prefabs:", EditorStyles.boldLabel);
                foreach (var p in _prefabResults)
                {
                    EditorGUILayout.BeginHorizontal(); // Start horizontal layout for foldout + buttons

                    // Draw foldout with the prefab name
                    bool isExpanded = DrawFoldout(p, Path.GetFileNameWithoutExtension(p.Path));

                    // Add "Select All" button within the same row as foldout
                    if (GUILayout.Button("Select All", GUILayout.Width(100)))
                    {
                        foreach (var f in p.FoundTMPs)
                        {
                            f.Selected = true;
                        }
                    }

                    // Add "Deselect All" button within the same row as foldout
                    if (GUILayout.Button("Deselect All", GUILayout.Width(100)))
                    {
                        foreach (var f in p.FoundTMPs)
                        {
                            f.Selected = false;
                        }
                    }

                    EditorGUILayout.EndHorizontal(); // End horizontal layout

                    // Draw the foldout content if expanded
                    if (isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        foreach (var f in p.FoundTMPs)
                        {
                            if (_filterFont != null && f.TMPText.font != _filterFont) continue;
                            DrawTMPTextEntry(f);
                        }

                        EditorGUI.indentLevel--;
                    }
                }

                EditorGUILayout.EndVertical();
            }

            if (_sceneResults.Count > 0)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginVertical(_boxStyle);
                EditorGUILayout.LabelField("Scenes:", EditorStyles.boldLabel);

                foreach (var tmpTextInfo in _sceneResults)
                {
                    EditorGUILayout.BeginHorizontal(); // Start horizontal layout for foldout + button

                    // Draw foldout with the scene name
                    bool isExpanded = DrawFoldout(tmpTextInfo, tmpTextInfo.SceneName);

                    // Add "Open Scene" button within the same row as foldout
                    if (GUILayout.Button("Open Scene", GUILayout.Width(100)))
                    {
                        EditorSceneManager.OpenScene(tmpTextInfo.Path, OpenSceneMode.Additive);
                    }
                    
                    // Add "Select All" button within the same row as foldout
                    if (GUILayout.Button("Select All", GUILayout.Width(100)))
                    {
                        foreach (var f in tmpTextInfo.FoundTMPs)
                        {
                            f.Selected = true;
                        }
                    }
                    
                    // Add "Deselect All" button within the same row as foldout
                    if (GUILayout.Button("Deselect All", GUILayout.Width(100)))
                    {
                        foreach (var f in tmpTextInfo.FoundTMPs)
                        {
                            f.Selected = false;
                        }
                    }

                    EditorGUILayout.EndHorizontal(); // End horizontal layout

                    // Draw the foldout content if expanded
                    if (isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        foreach (var f in tmpTextInfo.FoundTMPs)
                        {
                            if (_filterFont != null && f.TMPText.font != _filterFont) continue;
                            DrawTMPTextEntry(f);
                        }

                        EditorGUI.indentLevel--;
                    }
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawFontGrouping()
        {
            foreach (var kvp in _fontGroups)
            {
                if (_filterFont != null && kvp.Key != _filterFont) continue;

                EditorGUILayout.BeginVertical(_boxStyle);
                string fontName = kvp.Key ? kvp.Key.name : "(No Font)";

                EditorGUILayout.BeginHorizontal(); // Start horizontal layout for foldout + buttons

                // Draw foldout with the font name
                bool isExpanded = DrawFoldout(kvp.Key, fontName);

                // Add "Select All" button within the same row as foldout
                if (GUILayout.Button("Select All", GUILayout.Width(100)))
                {
                    foreach (var f in kvp.Value)
                    {
                        f.Selected = true;
                    }
                }

                // Add "Deselect All" button within the same row as foldout
                if (GUILayout.Button("Deselect All", GUILayout.Width(100)))
                {
                    foreach (var f in kvp.Value)
                    {
                        f.Selected = false;
                    }
                }

                EditorGUILayout.EndHorizontal(); // End horizontal layout

                // Draw the foldout content if expanded
                if (isExpanded)
                {
                    EditorGUI.indentLevel++;
                    foreach (var f in kvp.Value)
                    {
                        DrawTMPTextEntry(f);
                    }
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawTMPTextEntry(FoundTMP f)
        {
            var rect = EditorGUILayout.GetControlRect(GUILayout.Height(20));
            var bgColor = GetFontColor(f.TMPText.font);

            // Alternate row background color (pastel)
            EditorGUI.DrawRect(rect, bgColor);

            // Show checkbox
            Rect toggleRect = new Rect(rect.x, rect.y, rect.width - 60 - 120, rect.height);
            f.Selected = EditorGUI.Toggle(toggleRect, f.Selected);

            // Determine prefix based on Group by Font state
            string prefix = "";
            if (_showFontGrouping)
            {
                if (f.Mother.SceneName != null)
                {
                    prefix = $"[Scene: {f.Mother.SceneName}] ";
                }
                else
                {
                    prefix = "[Prefab] ";
                }
            }

            // Black text for contrast
            GUI.contentColor = Color.black;
            EditorGUI.LabelField(new Rect(rect.x + 25, rect.y, rect.width - 100, rect.height),
                $"{prefix}{f.RelativePath} (Font: {(f.TMPText.font ? f.TMPText.font.name : "None")})");
            GUI.contentColor = Color.white;

            // Select button
            var selectButtonRect = new Rect(rect.x + rect.width - 60, rect.y, 60, rect.height);
            if (GUI.Button(selectButtonRect, "Select"))
            {
                SelectObjectInProjectOrScene(f);
            }

            if (_showFontGrouping)
            {
                // If it's not a scene object, don't show the Open Scene button
                if (f.Mother.SceneName == null) return;
                
                // Add Open Scene button
                if (GUI.Button(new Rect(rect.x + rect.width - 180, rect.y, 120, rect.height), "Open Scene"))
                {
                    var sceneToOpen = f.Mother.Path;
                    EditorSceneManager.OpenScene(sceneToOpen, OpenSceneMode.Additive);
                }
            }
        }

        private void SelectObjectInProjectOrScene(FoundTMP f)
        {
            if (f.Mother.IsSceneLoaded)
            {
                // Highlight prefab in project window

                // Find the gameobject in the scene
                if (f.TMPText == null)
                {
                    var sceneName = f.Mother.SceneName;

                    // find object from the scene
                    f.TMPText = Resources.FindObjectsOfTypeAll<TMP_Text>()
                        .FirstOrDefault(tx => GetSceneObjectPath(tx.gameObject) == f.RelativePath);
                }

                Debug.Assert(f.TMPText != null, "f.TMPText != null");
                Selection.activeObject = f.TMPText.gameObject;
                EditorGUIUtility.PingObject(f.TMPText.gameObject);
            }
            else
            {
                Debug.LogWarning("Selecting objects in unopened scenes is not supported.");
            }
        }

        private Color GetFontColor(TMP_FontAsset font)
        {
            if (!_fontColors.ContainsKey(font))
            {
                // Hue based on font index
                float hue = (_colorIndex * 0.113f) % 1f;
                float saturation = 0.2f;
                float value = 0.95f;
                Color col = Color.HSVToRGB(hue, saturation, value);
                _fontColors[font] = col;
                _colorIndex++;
            }

            return _fontColors[font];
        }

        private bool DrawFoldout(object key, string label)
        {
            if (!_foldoutStates.ContainsKey(key))
                _foldoutStates[key] = true;

            _foldoutStates[key] = EditorGUILayout.Foldout(_foldoutStates[key], label, true, _sectionStyle);
            return _foldoutStates[key];
        }

        private void AddToFontGroup(TMP_Text t, FoundTMP foundTMP)
        {
            TMP_FontAsset font = t.font;
            if (!_fontGroups.ContainsKey(font))
            {
                _fontGroups[font] = new List<FoundTMP>();
            }

            if (!_fontGroups[font].Contains(foundTMP))
            {
                _fontGroups[font].Add(foundTMP);
            }
        }

        private void ApplyChanges()
        {
            if (_newFontAsset == null) return;

            bool anyChanges = false;

            // Prefabs
            foreach (var p in _prefabResults)
            {
                var prefabInstance = PrefabUtility.LoadPrefabContents(p.Path);
                try
                {
                    bool prefabUpdated = false;
                    var selectedTMPs = p.FoundTMPs.Where(x => x.Selected).ToList();

                    var allTexts = prefabInstance.GetComponentsInChildren<TMP_Text>(true);

                    foreach (var f in selectedTMPs)
                    {
                        var match = allTexts.FirstOrDefault(tx =>
                            GetGameObjectPath(tx.gameObject, prefabInstance.transform) == f.RelativePath);

                        if (match != null)
                        {
                            Undo.RecordObject(match, "Change TMP Font");
                            match.font = _newFontAsset;
                            EditorUtility.SetDirty(match);
                            prefabUpdated = true;
                        }
                    }

                    if (prefabUpdated)
                    {
                        PrefabUtility.SaveAsPrefabAsset(prefabInstance, p.Path);
                        anyChanges = true;
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(prefabInstance);
                }
            }

            // Scenes
            var selectedScenes = _sceneSelection.Where(s => s.Value).Select(s => s.Key).ToList();
            foreach (var s in _sceneResults)
            {
                if (!selectedScenes.Contains(s.Path)) continue;

                var scene = EditorSceneManager.OpenScene(s.Path, OpenSceneMode.Additive);
                try
                {
                    bool sceneUpdated = false;
                    var selectedTMPs = s.FoundTMPs.Where(x => x.Selected).ToList();
                    var allTexts = Resources.FindObjectsOfTypeAll<TMP_Text>()
                        .Where(tx => tx.gameObject.scene == scene).ToArray();

                    foreach (var f in selectedTMPs)
                    {
                        var match = allTexts.FirstOrDefault(tx =>
                            GetSceneObjectPath(tx.gameObject) == f.RelativePath);

                        if (match != null)
                        {
                            Undo.RecordObject(match, "Change TMP Font");
                            match.font = _newFontAsset;
                            EditorUtility.SetDirty(match);
                            sceneUpdated = true;
                        }
                    }

                    if (sceneUpdated)
                    {
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                        anyChanges = true;
                    }
                }
                finally
                {
                    if (!IsOnlyLoadedScene(scene))
                    {
                        EditorSceneManager.CloseScene(scene, true);
                    }
                }
            }

            if (anyChanges)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                _fontGroups.Clear();
                _fontColors.Clear();
                _colorIndex = 0;

                foreach (var info in _prefabResults.Concat(_sceneResults))
                {
                    foreach (var tmp in info.FoundTMPs)
                    {
                        if (tmp.Selected)
                        {
                            tmp.TMPText.font = _newFontAsset;
                        }

                        AddToFontGroup(tmp.TMPText, tmp);
                    }
                }

                Repaint();
            }

            if (_initialActiveScene.IsValid())
            {
                EditorSceneManager.OpenScene(_initialActiveScene.path, OpenSceneMode.Single);
            }

            Debug.Log("Font asset changes applied successfully!");
        }

        private string GetGameObjectPath(GameObject obj, Transform root = null)
        {
            if (obj == null) return "";
            string path = obj.name;
            var t = obj.transform;
            while (t.parent != null && t.parent != root)
            {
                path = t.parent.name + "/" + path;
                t = t.parent;
            }

            return path;
        }

        private string GetSceneObjectPath(GameObject obj)
        {
            if (obj == null) return "";
            string path = obj.name;
            Transform t = obj.transform;
            while (t.parent != null)
            {
                path = t.parent.name + "/" + path;
                t = t.parent;
            }

            return path;
        }
    }
}