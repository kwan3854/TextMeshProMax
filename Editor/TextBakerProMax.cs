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

        // CJK kanji options (multiple selection)
        private bool _includeCjkUnified; // U+4E00~U+9FBF
        private bool _includeCjkExtensionB; // U+20000~U+2A6DF
        private bool _includeCjkCompatibility; // U+F900~U+FAFF
        private bool _includeCjkCompatibilitySupplement; // U+2F800~U+2FA1F
        private bool _includeCjkRadicals; // U+2E80~U+2EFF

        // Korean options (multiple selection)
        private bool _includeKs1001; // KS-1001
        private bool _includeFullHangul; // Full Hangul
        private bool _includeHangulJamo; // Hangul Jamo
        private bool _includeHangulCompatibilityJamo; // Hangul Compatibility Jamo
        private bool _includeHangulJamoExtendedA; // Hangul Jamo Extended-A
        private bool _includeHangulJamoExtendedB; // Hangul Jamo Extended-B

        // Japanese options (multiple selection)
        private bool _includeHiragana; // U+3040~U+309F
        private bool _includeKatakana; // U+30A0~U+30FF
        private bool _includeKatakanaPhoneticExt; // U+31F0~U+31FF

        // Output settings
        private string _outputFolderPath = "Assets"; // 기본값: Assets 폴더
        private string _outputFileName = "GeneratedFontAtlas.asset";

        // Debug output
        private bool _outputMissingCharactersAsTxtFile;

        // Constants
        private const int AtlasSize = 2048;
        private const int CharacterSize = 100;
        private const float PaddingRatio = 0.1f;

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

            // CJK
            { "CJK Unified Ideographs", (0x4E00, 0x9FBF) },
            { "CJK Unified Ideographs Extension B", (0x20000, 0x2A6DF) },
            { "CJK Compatibility Ideographs", (0xF900, 0xFAFF) },
            { "CJK Compatibility Ideographs Supplement", (0x2F800, 0x2FA1F) },
            { "CJK Radicals Supplement", (0x2E80, 0x2EFF) }
        };

        [MenuItem("Tools/Font Atlas Generator")]
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
                DrawCJKOptions();
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
                        // eg: /Users/.../ProjectName/Assets -> Assets
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
            GUILayout.Label("Korean Options (Multiple selection)", EditorStyles.boldLabel);
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

        private void DrawCJKOptions()
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
                    string korean2350Characters = LoadKorean2350Characters();
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

            TMP_FontAsset tmpFontAsset = CreateTMPFontAsset(_targetFont, AtlasSize, CharacterSize, PaddingRatio);

            var success = tmpFontAsset.HasCharacters(UnicodeUintArrayToString(unicodeArray), out var missingCharacters,
                false, true);

            // Save the font asset
            string assetPath = Path.Combine(_outputFolderPath, _outputFileName);
            if (!assetPath.StartsWith("Assets"))
            {
                Debug.LogError("Output path must be inside the Assets folder.");
                return;
            }

            AssetDatabase.CreateAsset(tmpFontAsset, assetPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Font atlas generation completed! Saved at {assetPath}");

            if (!success)
            {
                Debug.Assert(missingCharacters != null, "Missing characters array is null!");

                if (_outputMissingCharactersAsTxtFile && missingCharacters.Length > 0)
                {
                    string missingCharactersFilePath =
                        Path.Combine(_outputFolderPath, $"{_outputFileName}_MissingCharacters.txt");
                    var missingCharactersString = UnicodeUintArrayToString(missingCharacters);
                    File.WriteAllText(missingCharactersFilePath, missingCharactersString);
                    Debug.Log($"Missing characters saved at {missingCharactersFilePath}");

                    // Refresh the asset database to make the file visible in the project window
                    AssetDatabase.Refresh();
                }
            }
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

        private TMP_FontAsset CreateTMPFontAsset(Font font, int atlasSize, int charSize, float paddingRatio)
        {
            int padding = Mathf.CeilToInt(charSize * paddingRatio);
            TMP_FontAsset tmpFontAsset = TMP_FontAsset.CreateFontAsset(
                font,
                charSize,
                padding,
                GlyphRenderMode.SDFAA,
                atlasSize,
                atlasSize // multiAtlasSupport
            );

            Debug.Assert(tmpFontAsset != null, "Failed to create TMP_FontAsset!");
            return tmpFontAsset;
        }

        private string LoadKorean2350Characters()
        {
            TextAsset txt = Resources.Load<TextAsset>("korean2350");
            if (txt == null)
            {
                Debug.LogError("korean2350.txt not found in Resources!");
                return string.Empty;
            }

            return txt.text;
        }
    }
}