using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace TextMeshProMax.Editor
{
    public class TextBakerProMax : EditorWindow
    {
        private Font _targetFont;

        // Language options
        private bool _includeChinese;
        private bool _includeJapanese;
        private bool _includeKorean;

        // CJK kanji options
        private bool _includeCjkUnified; // 4E00~9FBF
        private bool _includeCjkExtensionB; // U+20000~U+2A6DF
        private bool _includeCjkCompatibility; // U+F900~U+FAFF
        private bool _includeCjkCompatibilitySupplement; // U+2F800~U+2FA1F
        private bool _includeCjkRadicals; // U+2E80~U+2EFF

        // Korean options
        private bool _includeKs1001; // KS-1001
        private bool _includeFullHangul; // Full Hangul
        private bool _includeHangulJamo; // Hangul Jamo
        private bool _includeHangulCompatibilityJamo; // Hangul Compatibility Jamo
        private bool _includeHangulJamoExtendedA; // Hangul Jamo Extended-A
        private bool _includeHangulJamoExtendedB; // Hangul Jamo Extended-B

        // Japanese options
        private bool _includeHiragana; // U+3040~U+309F
        private bool _includeKatakana; // U+30A0~U+30FF
        private bool _includeKatakanaPhoneticExt; // U+31F0~U+31FF
        
        // Chinese options
        private bool _includeCommon3500;
        private bool _includeCommon7000;

        // Output settings
        private string _outputFolderPath = "Assets";
        private string _outputFileName = "GeneratedFont.asset";

        // Debug output
        private bool _outputMissingCharactersAsTxtFile;

        // Constants
        private const int AtlasSize = 2048;
        private const int CharacterSize = 100;
        private const float PaddingRatio = 0.1f;
        private const string Korean2350Characters = "korean_2350";
        private const string ChineseCommon3500Characters = "chinese_common_3500";
        private const string ChineseCommon7000Characters = "chinese_common_7000";

        private readonly Dictionary<string, (int start, int end)> _predefinedRanges = new()
        {
            // Korean
            { "KS X 1001 Hangul", (0x0000, 0x0000) }, // Use .txt file.
            { "Full Hangul Syllables (KS1001 included)", (0xAC00, 0xD7A3) },
            { "Hangul Jamo", (0x1100, 0x11FF) },
            { "Hangul Compatibility Jamo", (0x3130, 0x318F) },
            { "Hangul Jamo Extended-A", (0xA960, 0xA97F) },
            { "Hangul Jamo Extended-B", (0xD7B0, 0xD7FF) },

            // Japanese
            { "Hiragana", (0x3040, 0x309F) },
            { "Katakana", (0x30A0, 0x30FF) },
            { "KatakanaPhoneticExt", (0x31F0, 0x31FF) },
            
            // Chinese
            { "Common 3500", (0x0000, 0x0000) }, // Use .txt file.
            { "Common 7000", (0x0000, 0x0000) }, // Use .txt file.

            // CJK
            { "CJK Unified Ideographs", (0x4E00, 0x9FBF) },
            { "CJK Unified Ideographs Extension B", (0x20000, 0x2A6DF) },
            { "CJK Compatibility Ideographs", (0xF900, 0xFAFF) },
            { "CJK Compatibility Ideographs Supplement", (0x2F800, 0x2FA1F) },
            { "CJK Radicals Supplement", (0x2E80, 0x2EFF) }
        };

        [MenuItem("Tools/TextMeshPro Max/CJK Font Atlas Generator")]
        public static void ShowWindow()
        {
            GetWindow<TextBakerProMax>("Font Atlas Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Font Atlas Generator", EditorStyles.boldLabel);

            _targetFont = (Font)EditorGUILayout.ObjectField("Target Font", _targetFont, typeof(Font), false);

            GUILayout.Space(10);
            GUILayout.Label("Languages", EditorStyles.boldLabel);
            _includeChinese = EditorGUILayout.Toggle("Chinese", _includeChinese);
            _includeJapanese = EditorGUILayout.Toggle("Japanese", _includeJapanese);
            _includeKorean = EditorGUILayout.Toggle("Korean", _includeKorean);

            // CJK
            if (_includeChinese || _includeJapanese || _includeKorean)
            {
                DrawCjkOptions();
            }

            // Korean
            if (_includeKorean)
            {
                DrawKoreanOptions();
            }

            // Japanese
            if (_includeJapanese)
            {
                DrawJapaneseOptions();
            }

            if (_includeChinese)
            {
                DrawChineseOptions();
            }

            GUILayout.Space(10);
            GUILayout.Label("Output Settings", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Output Folder");
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Output Folder", Application.dataPath, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    // Absolute path to relative path
                    if (selectedPath.StartsWith(Application.dataPath))
                    {
                        selectedPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    }

                    _outputFolderPath = selectedPath;
                }
            }

            EditorGUILayout.EndHorizontal();

            _outputFolderPath = EditorGUILayout.TextField("Output Folder Path", _outputFolderPath);
            _outputFileName = EditorGUILayout.TextField("Output File Name", _outputFileName);

            _outputMissingCharactersAsTxtFile = EditorGUILayout.Toggle("Output Missing Characters as TXT File",
                _outputMissingCharactersAsTxtFile);

            GUILayout.Space(20);
            if (GUILayout.Button("Generate Font Atlas"))
            {
                GenerateAtlases();
            }
        }

        private void DrawChineseOptions()
        {
            GUILayout.Space(10);
            GUILayout.Label("Chinese Options", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            _includeCommon3500 = GUILayout.Toggle(_includeCommon3500, "Common 3500 Characters", "Radio");
            if (_includeCommon3500) _includeCommon7000 = false;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _includeCommon7000 = GUILayout.Toggle(_includeCommon7000, "Common 7000 Characters", "Radio");
            if (_includeCommon7000) _includeCommon3500 = false;
            EditorGUILayout.EndHorizontal();
        }

        private void DrawJapaneseOptions()
        {
            GUILayout.Space(10);
            GUILayout.Label("Japanese Options", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Hiragana (3040-309F)", GUILayout.Width(300));
            _includeHiragana = EditorGUILayout.Toggle(_includeHiragana);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Katakana (30A0-30FF)", GUILayout.Width(300));
            _includeKatakana = EditorGUILayout.Toggle(_includeKatakana);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Katakana Phonetic Extensions (31F0-31FF)", GUILayout.Width(300));
            _includeKatakanaPhoneticExt = EditorGUILayout.Toggle(_includeKatakanaPhoneticExt);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawKoreanOptions()
        {
            GUILayout.Space(10);
            GUILayout.Label("Korean Options", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("KS-1001 (B0A0-C8FF)", GUILayout.Width(300));
            _includeKs1001 = EditorGUILayout.Toggle(_includeKs1001);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Full Hangul (AC00-D7A3)", GUILayout.Width(300));
            _includeFullHangul = EditorGUILayout.Toggle(_includeFullHangul);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Hangul Jamo (1100-11FF)", GUILayout.Width(300));
            _includeHangulJamo = EditorGUILayout.Toggle(_includeHangulJamo);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Hangul Compatibility Jamo (3130-318F)", GUILayout.Width(300));
            _includeHangulCompatibilityJamo = EditorGUILayout.Toggle(_includeHangulCompatibilityJamo);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Hangul Jamo Extended-A (A960-A97F)", GUILayout.Width(300));
            _includeHangulJamoExtendedA = EditorGUILayout.Toggle(_includeHangulJamoExtendedA);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Hangul Jamo Extended-B (D7B0-D7FF)", GUILayout.Width(300));
            _includeHangulJamoExtendedB = EditorGUILayout.Toggle(_includeHangulJamoExtendedB);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCjkOptions()
        {
            GUILayout.Space(10);
            GUILayout.Label("CJK Options", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("CJK Unified Ideographs", GUILayout.Width(300));
            _includeCjkUnified = EditorGUILayout.Toggle(_includeCjkUnified);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("CJK Unified Ideographs Extension B", GUILayout.Width(300));
            _includeCjkExtensionB = EditorGUILayout.Toggle(_includeCjkExtensionB);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("CJK Compatibility Ideographs", GUILayout.Width(300));
            _includeCjkCompatibility = EditorGUILayout.Toggle(_includeCjkCompatibility);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("CJK Compatibility Ideographs Supplement", GUILayout.Width(300));
            _includeCjkCompatibilitySupplement = EditorGUILayout.Toggle(_includeCjkCompatibilitySupplement);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("CJK Radicals Supplement", GUILayout.Width(300));
            _includeCjkRadicals = EditorGUILayout.Toggle(_includeCjkRadicals);
            EditorGUILayout.EndHorizontal();
        }

        private void GenerateAtlases()
        {
            if (_targetFont == null)
            {
                Debug.LogError("Target Font must be assigned!");
                return;
            }

            if (string.IsNullOrEmpty(_outputFolderPath) || string.IsNullOrEmpty(_outputFileName))
            {
                Debug.LogError("Output folder path and file name must be specified!");
                return;
            }

            var codePoints = new HashSet<int>();

            // Korean
            if (_includeKorean)
            {
                if (_includeKs1001)
                {
                    string korean2350Characters = LoadCharactersFromTxt(Korean2350Characters);
                    foreach (char c in korean2350Characters)
                    {
                        codePoints.Add(c);
                    }
                }

                if (_includeFullHangul)
                    AddCharactersFromRange(_predefinedRanges["Full Hangul Syllables (KS1001 included)"], codePoints);

                if (_includeHangulJamo)
                    AddCharactersFromRange(_predefinedRanges["Hangul Jamo"], codePoints);

                if (_includeHangulCompatibilityJamo)
                    AddCharactersFromRange(_predefinedRanges["Hangul Compatibility Jamo"], codePoints);

                if (_includeHangulJamoExtendedA)
                    AddCharactersFromRange(_predefinedRanges["Hangul Jamo Extended-A"], codePoints);

                if (_includeHangulJamoExtendedB)
                    AddCharactersFromRange(_predefinedRanges["Hangul Jamo Extended-B"], codePoints);
            }

            // Japanese
            if (_includeJapanese)
            {
                if (_includeHiragana)
                    AddCharactersFromRange(_predefinedRanges["Hiragana"], codePoints);

                if (_includeKatakana)
                    AddCharactersFromRange(_predefinedRanges["Katakana"], codePoints);

                if (_includeKatakanaPhoneticExt)
                    AddCharactersFromRange(_predefinedRanges["KatakanaPhoneticExt"], codePoints);
            }

            // Chinese
            if (_includeChinese)
            {
                if (_includeCommon3500)
                {
                    string common3500Characters = LoadCharactersFromTxt(ChineseCommon3500Characters);
                    foreach (char c in common3500Characters)
                    {
                        codePoints.Add(c);
                    }
                }

                if (_includeCommon7000)
                {
                    string common7000Characters = LoadCharactersFromTxt(ChineseCommon7000Characters);
                    foreach (char c in common7000Characters)
                    {
                        codePoints.Add(c);
                    }
                }

                // 여기서 CJK Unified 등도 추가 가능하지만, 이미 아래 CJK 섹션에 있음.
            }

            // CJK
            if (_includeChinese || _includeJapanese || _includeKorean)
            {
                if (_includeCjkUnified)
                    AddCharactersFromRange(_predefinedRanges["CJK Unified Ideographs"], codePoints);

                if (_includeCjkExtensionB)
                    AddCharactersFromRange(_predefinedRanges["CJK Unified Ideographs Extension B"], codePoints);

                if (_includeCjkCompatibility)
                    AddCharactersFromRange(_predefinedRanges["CJK Compatibility Ideographs"], codePoints);

                if (_includeCjkCompatibilitySupplement)
                    AddCharactersFromRange(_predefinedRanges["CJK Compatibility Ideographs Supplement"], codePoints);

                if (_includeCjkRadicals)
                    AddCharactersFromRange(_predefinedRanges["CJK Radicals Supplement"], codePoints);
            }

            // int -> uint
            uint[] unicodeArray = codePoints.Select(cp => (uint)cp).ToArray();

            // Create font asset in dynamic mode with multi atlas support = true
            int padding = Mathf.CeilToInt(CharacterSize * PaddingRatio);
            
            TMP_FontAsset tmpFontAsset = TMP_FontAsset.CreateFontAsset(
                _targetFont,
                CharacterSize,
                padding,
                GlyphRenderMode.SDFAA,
                AtlasSize,
                AtlasSize,
                AtlasPopulationMode.Dynamic, // Dynamic mode
                true // multiAtlasSupport enabled
            );

            Debug.Assert(tmpFontAsset != null, "Failed to create TMP_FontAsset!");

            // In dynamic mode, atlas and material are not automatically created at edit-time.
            // Let's create an initial empty atlas and material to avoid missing references.
            CreateInitialAtlasAndMaterial(tmpFontAsset);

            // Save the font asset
            string assetPath = Path.Combine(_outputFolderPath, _outputFileName);
            if (!assetPath.StartsWith("Assets"))
            {
                Debug.LogError("Output path must be inside the Assets folder.");
                return;
            }

            AssetDatabase.CreateAsset(tmpFontAsset, assetPath);

            // Add the created atlas texture and material as sub assets
            var atlasTexture = tmpFontAsset.atlasTextures != null && tmpFontAsset.atlasTextures.Length > 0
                ? tmpFontAsset.atlasTextures[0]
                : null;
            if (atlasTexture != null)
                AssetDatabase.AddObjectToAsset(atlasTexture, tmpFontAsset);

            if (tmpFontAsset.material != null)
                AssetDatabase.AddObjectToAsset(tmpFontAsset.material, tmpFontAsset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            tmpFontAsset.ReadFontAssetDefinition();
            
            // Call HasCharacters with tryAddCharacter = true to populate as many chars as possible.
            bool success = tmpFontAsset.HasCharacters(UnicodeUintArrayToString(unicodeArray), out uint[] missingCharacters, false, true);

            Debug.Log($"Font atlas generation completed! Saved at {assetPath}");

            if (!success && missingCharacters != null && missingCharacters.Length > 0 && _outputMissingCharactersAsTxtFile)
            {
                string missingCharactersFilePath = Path.Combine(_outputFolderPath, $"{_outputFileName}_MissingCharacters.txt");
                var missingCharactersString = UnicodeUintArrayToString(missingCharacters);
                File.WriteAllText(missingCharactersFilePath, missingCharactersString);
                Debug.Log($"Missing characters saved at {missingCharactersFilePath}");
                AssetDatabase.Refresh();
            }
        }

        private void CreateInitialAtlasAndMaterial(TMP_FontAsset fontAsset)
        {
            // If atlasTextures is null or empty, create a minimal placeholder atlas.
            if (fontAsset.atlasTextures == null || fontAsset.atlasTextures.Length == 0)
            {
                // Create a small empty texture (e.g. 64x64) as initial atlas
                Texture2D placeholderAtlas = new Texture2D(64, 64, TextureFormat.Alpha8, false, true);
                // Fill with transparent pixels
                var fill = new Color32[64 * 64];
                for (int i = 0; i < fill.Length; i++) fill[i] = new Color32(0,0,0,0);
                placeholderAtlas.SetPixels32(fill);
                placeholderAtlas.Apply(false, false);
                placeholderAtlas.name = "Placeholder Atlas";

                fontAsset.atlasTextures = new[] { placeholderAtlas };
            }

            // If material is null, create a default material referencing the atlas
            if (fontAsset.material == null)
            {
                Shader sdfShader = Shader.Find("TextMeshPro/Distance Field");
                Material mat = new Material(sdfShader);
                mat.name = "Font Material";
                mat.SetTexture(ShaderUtilities.ID_MainTex, fontAsset.atlasTextures[0]);
                fontAsset.material = mat;
            }
        }

        private string LoadCharactersFromTxt(string fileName)
        {
            TextAsset txt = Resources.Load<TextAsset>(fileName);
            if (txt == null)
            {
                Debug.LogError($"{fileName}.txt not found in Resources!");
                return string.Empty;
            }

            return txt.text;
        }

        private string UnicodeUintArrayToString(uint[] unicodeArray)
        {
            Debug.Assert(unicodeArray != null, "Unicode array is null!");

            return string.Join("", unicodeArray.Select(cp => char.ConvertFromUtf32((int)cp)));
        }

        private void AddCharactersFromRange((int start, int end) range, HashSet<int> characters)
        {
            for (int i = range.start; i <= range.end; i++)
            {
                characters.Add(i);
            }
        }
    }
}