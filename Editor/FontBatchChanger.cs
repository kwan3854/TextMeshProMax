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
        private Vector2 _scrollPosition;

        private GUIStyle _boxStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _sectionStyle;

        private readonly Dictionary<TMP_FontAsset, Color> _fontColors = new();
        private int _colorIndex;

        private class TMPTextInfo
        {
            public string Path;
            public string SceneName;
            public readonly List<FoundTMP> FoundTMPs = new();
        }

        private class FoundTMP
        {
            public GameObject GameObject;
            public TMP_Text TMPText;
            public bool Selected = true;
            public string RelativePath;

            private bool IsSceneObject => GameObject.scene.IsValid();
            public bool IsPrefab => !IsSceneObject;
        }

        private readonly List<TMPTextInfo> _prefabResults = new();
        private readonly List<TMPTextInfo> _sceneResults = new();
        private readonly Dictionary<TMP_FontAsset, List<FoundTMP>> _fontGroups = new();
        private readonly Dictionary<object, bool> _foldoutStates = new();
        private Scene _initialActiveScene;

        [MenuItem("Tools/TMP Font Batch Changer")]
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
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.BeginVertical(_boxStyle);
            EditorGUILayout.LabelField("TMP Font Batch Changer", _headerStyle);

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical(_boxStyle);
            EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
            _newFontAsset =
                (TMP_FontAsset)EditorGUILayout.ObjectField("New Font Asset", _newFontAsset, typeof(TMP_FontAsset),
                    false);

            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Prefab Root Folder:", GUILayout.Width(120));
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

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical(_boxStyle);
            EditorGUILayout.LabelField("Scene Selection", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            foreach (var sp in _allScenePaths)
            {
                _sceneSelection[sp] = EditorGUILayout.Toggle(Path.GetFileNameWithoutExtension(sp), _sceneSelection[sp]);
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
                        var info = new TMPTextInfo() { Path = path};
                        foreach (var t in texts)
                        {
                            var foundTMP = new FoundTMP
                            {
                                GameObject = t.gameObject,
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

            // Scene search
            var selectedScenes = _sceneSelection.Where(s => s.Value).Select(s => s.Key).ToList();
            foreach (var scenePath in selectedScenes)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                var texts = Resources.FindObjectsOfTypeAll<TMP_Text>()
                    .Where(t => t.gameObject.scene == scene).ToArray();

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
                            GameObject = t.gameObject,
                            TMPText = t,
                            Selected = true,
                            RelativePath = GetSceneObjectPath(t.gameObject)
                        };
                        info.FoundTMPs.Add(foundTMP);
                        AddToFontGroup(t, foundTMP);
                    }

                    _sceneResults.Add(info);
                }

                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private void DrawSceneAndPrefabResults()
        {
            if (_prefabResults.Count > 0)
            {
                EditorGUILayout.BeginVertical(_boxStyle);
                EditorGUILayout.LabelField("Prefabs:", EditorStyles.boldLabel);
                foreach (var p in _prefabResults)
                {
                    if (DrawFoldout(p, Path.GetFileNameWithoutExtension(p.Path)))
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
                foreach (var s in _sceneResults)
                {
                    if (DrawFoldout(s, s.SceneName))
                    {
                        EditorGUI.indentLevel++;
                        foreach (var f in s.FoundTMPs)
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

                if (DrawFoldout(kvp.Key, fontName))
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

            // 전체 라인에 배경색 적용
            EditorGUI.DrawRect(rect, bgColor);

            // 체크박스 표시
            Rect toggleRect = new Rect(rect.x, rect.y, 20, rect.height);
            f.Selected = EditorGUI.Toggle(toggleRect, f.Selected);

            // 텍스트(검은색)
            GUI.contentColor = Color.black;
            EditorGUI.LabelField(new Rect(rect.x + 25, rect.y, rect.width - 100, rect.height),
                $"{f.RelativePath} (Font: {(f.TMPText.font ? f.TMPText.font.name : "None")})");
            GUI.contentColor = Color.white;

            // Select 버튼
            var selectButtonRect = new Rect(rect.x + rect.width - 60, rect.y, 60, rect.height);
            if (GUI.Button(selectButtonRect, "Select"))
            {
                SelectObjectInProjectOrScene(f);
            }
        }

        private void SelectObjectInProjectOrScene(FoundTMP f)
        {
            if (f.IsPrefab)
            {
                // Prefab 오브젝트를 프로젝트에서 하이라이트
                Selection.activeObject = f.GameObject;
                EditorGUIUtility.PingObject(f.GameObject);
            }
            else
            {
                // 씬 오브젝트를 씬에서 하이라이트
                Selection.activeGameObject = f.GameObject;
                EditorGUIUtility.PingObject(f.GameObject);
            }
        }

        private Color GetFontColor(TMP_FontAsset font)
        {
            if (!_fontColors.ContainsKey(font))
            {
                // Hue 기반 파스텔 톤 색상 생성
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
                    EditorSceneManager.CloseScene(scene, true);
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