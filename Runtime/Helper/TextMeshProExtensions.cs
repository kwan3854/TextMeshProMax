using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace TextMeshProMax.Runtime.Helper
{
    public static class TextMeshProExtensions
    {
        /// <summary>
        /// Attempts to retrieve the Rect information for a specific string within a TMP_Text object.
        /// </summary>
        /// <param name="text">The target TMP_Text object.</param>
        /// <param name="targetString">The string to find within the text.</param>
        /// <param name="findMode">
        /// The mode to find the string (first occurrence, all occurrences, etc.):
        /// <list type="bullet">
        /// <item>
        /// <description>TextFindMode.First: Returns a list containing the Rect values for the first occurrence of the specified string.</description>
        /// </item>
        /// <item>
        /// <description>TextFindMode.All: Returns a list containing the Rect values for all occurrences of the specified string.</description>
        /// </item>
        /// </list>
        /// </param>
        /// <param name="results">The output list of TextRectInfo containing the Rect values for the specified string.</param>
        /// <returns>True if the string was found and Rect information was retrieved; otherwise, false.</returns>
        public static bool TryGetStringRects(this TMP_Text text, string targetString, TextFindMode findMode,
            out List<TextRectInfo> results)
        {
            return TryGetStringRectsInternal(new TMPTextWrapper(text), targetString, findMode, out results);
        }

        /// <summary>
        /// Attempts to retrieve the Rect information for a specific string within a TextMeshPro object.
        /// </summary>
        /// <param name="text">The target TextMeshPro object.</param>
        /// <param name="targetString">The string to find within the text.</param>
        /// <param name="findMode">
        /// The mode to find the string (first occurrence, all occurrences, etc.):
        /// <list type="bullet">
        /// <item>
        /// <description>TextFindMode.First: Returns a list containing the Rect values for the first occurrence of the specified string.</description>
        /// </item>
        /// <item>
        /// <description>TextFindMode.All: Returns a list containing the Rect values for all occurrences of the specified string.</description>
        /// </item>
        /// </list>
        /// </param>
        /// <param name="results">The output list of TextRectInfo containing the Rect values for the specified string.</param>
        /// <returns>True if the string was found and Rect information was retrieved; otherwise, false.</returns>
        public static bool TryGetStringRects(this TextMeshPro text, string targetString, TextFindMode findMode,
            out List<TextRectInfo> results)
        {
            return TryGetStringRects((TMP_Text)text, targetString, findMode, out results);
        }

        /// <summary>
        /// Attempts to retrieve the Rect information for a specific string within a TextMeshProUGUI object.
        /// </summary>
        /// <param name="text">The target TextMeshProUGUI object.</param>
        /// <param name="targetString">The string to find within the text.</param>
        /// <param name="findMode">
        /// The mode to find the string (first occurrence, all occurrences, etc.):
        /// <list type="bullet">
        /// <item>
        /// <description>TextFindMode.First: Returns a list containing the Rect values for the first occurrence of the specified string.</description>
        /// </item>
        /// <item>
        /// <description>TextFindMode.All: Returns a list containing the Rect values for all occurrences of the specified string.</description>
        /// </item>
        /// </list>
        /// </param>
        /// <param name="results">The output list of TextRectInfo containing the Rect values for the specified string.</param>
        /// <returns>True if the string was found and Rect information was retrieved; otherwise, false.</returns>
        public static bool TryGetStringRects(this TextMeshProUGUI text, string targetString, TextFindMode findMode,
            out List<TextRectInfo> results)
        {
            return TryGetStringRects((TMP_Text)text, targetString, findMode, out results);
        }

        /// <summary>
        /// Retrieves the Rect information for a specific string within a TMP_Text object.
        /// For TextMeshPro (3D), the Rect values are in the text object's local space.
        /// For TextMeshProUGUI (UI), the Rect values are in the canvas's local space.
        /// </summary>
        /// <param name="text">The target TMP_Text object.</param>
        /// <param name="targetString">The string to find within the text.</param>
        /// <param name="findMode">
        /// The mode to find the string (first occurrence, all occurrences, etc.):
        /// <list type="bullet">
        /// <item>
        /// <description>TextFindMode.First: Returns a list containing the Rect values for the first occurrence of the specified string.</description>
        /// </item>
        /// <item>
        /// <description>TextFindMode.All: Returns a list containing the Rect values for all occurrences of the specified string.</description>
        /// </item>
        /// </list>
        /// </param>
        /// <returns>A list of TextRectInfo containing the Rect values for the specified string.</returns>
        /// <example>
        /// <code>
        /// TMP_Text text = GetComponent&lt;TMP_Text&gt;();
        /// var rects = text.GetStringRects("Hello", TextFindMode.All);
        ///
        /// foreach (var rectInfo in rects)
        /// {
        ///     foreach (var rect in rectInfo.Rects)
        ///     {
        ///         Debug.Log("BottomLeft: " + rect.min + ", TopRight: " + rect.max + ", String: " + rectInfo.TargetString);
        ///     }
        /// }
        /// </code>
        /// </example>
        public static List<TextRectInfo> GetStringRects(
            this TMP_Text text,
            string targetString,
            TextFindMode findMode = TextFindMode.First)
        {
            var wrapper = new TMPTextWrapper(text);
            return TryGetStringRectsInternal(wrapper, targetString, findMode, out var results)
                ? results
                : new List<TextRectInfo>();
        }

        /// <summary>
        /// Retrieves the Rect information for a specific string within a TextMeshPro object.
        /// The returned Rect values are in the text object's local space (relative to the text object's transform).
        /// </summary>
        /// <param name="text">The target TextMeshPro object.</param>
        /// <param name="targetString">The string to find within the text.</param>
        /// <param name="findMode">
        /// The mode to find the string (first occurrence, all occurrences, etc.):
        /// <list type="bullet">
        /// <item>
        /// <description>TextFindMode.First: Returns a list containing the Rect values for the first occurrence of the specified string.</description>
        /// </item>
        /// <item>
        /// <description>TextFindMode.All: Returns a list containing the Rect values for all occurrences of the specified string.</description>
        /// </item>
        /// </list>
        /// </param>
        /// <returns>A list of TextRectInfo containing the Rect values for the specified string.</returns>
        /// <example>
        /// <code>
        /// TextMeshPro text = GetComponent&lt;TextMeshPro&gt;();
        /// var rects = text.GetStringRects("Hello", TextFindMode.All);
        ///
        /// foreach (var rectInfo in rects)
        /// {
        ///     foreach (var rect in rectInfo.Rects)
        ///     {
        ///         Debug.Log("BottomLeft: " + rect.min + ", TopRight: " + rect.max + ", String: " + rectInfo.TargetString);
        ///     }
        /// }
        /// </code>
        /// </example>
        public static List<TextRectInfo> GetStringRects(
            this TextMeshPro text,
            string targetString,
            TextFindMode findMode = TextFindMode.First)
        {
            return GetStringRects((TMP_Text)text, targetString, findMode);
        }

        /// <summary>
        /// Retrieves the Rect information for a specific string within a TextMeshProUGUI object.
        /// The returned Rect values are in the canvas's local space (relative to the canvas transform).
        /// </summary>
        /// <param name="text">The target TextMeshProUGUI object.</param>
        /// <param name="targetString">The string to find within the text.</param>
        /// <param name="findMode">
        /// The mode to find the string (first occurrence, all occurrences, etc.):
        /// <list type="bullet">
        /// <item>
        /// <description>TextFindMode.First: Returns a list containing the Rect values for the first occurrence of the specified string.</description>
        /// </item>
        /// <item>
        /// <description>TextFindMode.All: Returns a list containing the Rect values for all occurrences of the specified string.</description>
        /// </item>
        /// </list>
        /// </param>
        /// <returns>A list of TextRectInfo containing the Rect values for the specified string.</returns>
        /// <example>
        /// <code>
        /// TextMeshProUGUI text = GetComponent&lt;TextMeshProUGUI&gt;();
        /// var rects = text.GetStringRects("Hello", TextFindMode.All);
        ///
        /// foreach (var rectInfo in rects)
        /// {
        ///     foreach (var rect in rectInfo.Rects)
        ///     {
        ///         Debug.Log("BottomLeft: " + rect.min + ", TopRight: " + rect.max + ", String: " + rectInfo.TargetString);
        ///     }
        /// }
        /// </code>
        /// </example>
        public static List<TextRectInfo> GetStringRects(
            this TextMeshProUGUI text,
            string targetString,
            TextFindMode findMode = TextFindMode.First)
        {
            return GetStringRects((TMP_Text)text, targetString, findMode);
        }

#if RUBY_TEXT_SUPPORT
        /// <summary>
        /// Attempts to retrieve the Rect information for a specific RubyString within a RubyTextMeshPro object.
        /// </summary>
        /// <param name="text">The target RubyTextMeshPro object.</param>
        /// <param name="rubyString">The RubyString to find within the text.</param>
        /// <param name="findMode">
        /// The mode to find the string (first occurrence, all occurrences, etc.):
        /// <list type="bullet">
        /// <item>
        /// <description>TextFindMode.First: Returns a list containing the Rect values for the first occurrence of the specified string.</description>
        /// </item>
        /// <item>
        /// <description>TextFindMode.All: Returns a list containing the Rect values for all occurrences of the specified string.</description>
        /// </item>
        /// </list>
        /// </param>
        /// <param name="results">The output list of TextRectInfo containing the Rect values for the specified string.</param>
        /// <returns>True if the string was found and Rect information was retrieved; otherwise, false.</returns>
        public static bool TryGetRubyStringRects(
            this RubyTextMeshPro text,
            RubyString rubyString,
            TextFindMode findMode,
            out List<TextRectInfo> results)
        {
            if (!rubyString.IsValid())
            {
                Debug.LogWarning("[RubyText] Invalid RubyString provided. Ensure all elements are valid.");
                results = new List<TextRectInfo>();
                return false;
            }

            return TryGetStringRectsInternal(new TMPTextWrapper(text), rubyString.ToPlainString(), findMode,
                out results);
        }

        /// <summary>
        /// Attempts to retrieve the Rect information for a specific RubyString within a RubyTextMeshProUGUI object.
        /// </summary>
        /// <param name="text">The target RubyTextMeshProUGUI object.</param>
        /// <param name="rubyString">The RubyString to find within the text.</param>
        /// <param name="findMode">
        /// The mode to find the string (first occurrence, all occurrences, etc.):
        /// <list type="bullet">
        /// <item>
        /// <description>TextFindMode.First: Returns a list containing the Rect values for the first occurrence of the specified string.</description>
        /// </item>
        /// <item>
        /// <description>TextFindMode.All: Returns a list containing the Rect values for all occurrences of the specified string.</description>
        /// </item>
        /// </list>
        /// </param>
        /// <param name="results">The output list of TextRectInfo containing the Rect values for the specified string.</param>
        /// <returns>True if the string was found and Rect information was retrieved; otherwise, false.</returns>
        public static bool TryGetRubyStringRects(
            this RubyTextMeshProUGUI text,
            RubyString rubyString,
            TextFindMode findMode,
            out List<TextRectInfo> results)
        {
            if (!rubyString.IsValid())
            {
                Debug.LogWarning("[RubyText] Invalid RubyString provided. Ensure all elements are valid.");
                results = new List<TextRectInfo>();
                return false;
            }

            return TryGetStringRectsInternal(new TMPTextWrapper(text), rubyString.ToPlainString(), findMode,
                out results);
        }

        /// <summary>
        /// Retrieves the Rect information for a complex RubyString consisting of multiple RubyElement entries.
        /// The returned Rect values are in the canvas's local space (relative to the canvas transform).
        /// </summary>
        /// <param name="rubyText">The target RubyTextMeshProUGUI object.</param>
        /// <param name="rubyString">The RubyString to find within the text.</param>
        /// <param name="findMode">
        /// The mode to find the string (first occurrence, all occurrences, etc.):
        /// <list type="bullet">
        /// <item>
        /// <description>TextFindMode.First: Returns a list containing the Rect values for the first occurrence of the specified string.</description>
        /// </item>
        /// <item>
        /// <description>TextFindMode.All: Returns a list containing the Rect values for all occurrences of the specified string.</description>
        /// </item>
        /// </list>
        /// </param>
        /// <returns>A list of TextRectInfo containing the Rect values for the specified string.</returns>
        /// <example>
        /// <code>
        /// RubyTextMeshProUGUI rubyText = GetComponent&lt;RubyTextMeshProUGUI&gt;();
        /// RubyString rubyString = new RubyString(new List&lt;RubyElement&gt; { ... });
        /// var rects = rubyText.GetRubyStringRects(rubyString, TextFindMode.All);
        ///
        /// foreach (var rectInfo in rects)
        /// {
        ///     foreach (var rect in rectInfo.Rects)
        ///     {
        ///         Debug.Log("BottomLeft: " + rect.min + ", TopRight: " + rect.max + ", String: " + rectInfo.TargetString);
        ///     }
        /// }
        /// </code>
        /// </example>
        public static List<TextRectInfo> GetRubyStringRects(
            this RubyTextMeshProUGUI rubyText,
            RubyString rubyString,
            TextFindMode findMode = TextFindMode.First)
        {
            if (!rubyString.IsValid())
            {
                Debug.LogWarning("[RubyText] Invalid RubyString provided. Ensure all elements are valid.");
                return new List<TextRectInfo>();
            }

            // Convert RubyString into a single plain search string
            var searchString = rubyString.ToPlainString();

            return GetStringRects((TMP_Text)rubyText, searchString, findMode);
        }

        /// <summary>
        /// Retrieves the Rect information for a complex RubyString consisting of multiple RubyElement entries.
        /// The returned Rect values are in the text object's local space (relative to the text object's transform).
        /// </summary>
        /// <param name="rubyText">The target RubyTextMeshPro object.</param>
        /// <param name="rubyString">The RubyString to find within the text.</param>
        /// <param name="findMode">
        /// The mode to find the string (first occurrence, all occurrences, etc.):
        /// <list type="bullet">
        /// <item>
        /// <description>TextFindMode.First: Returns a list containing the Rect values for the first occurrence of the specified string.</description>
        /// </item>
        /// <item>
        /// <description>TextFindMode.All: Returns a list containing the Rect values for all occurrences of the specified string.</description>
        /// </item>
        /// </list>
        /// </param>
        /// <returns>A list of TextRectInfo containing the Rect values for the specified string.</returns>
        /// <example>
        /// <code>
        /// RubyTextMeshPro rubyText = GetComponent&lt;RubyTextMeshPro&gt;();
        /// RubyString rubyString = new RubyString(new List&lt;RubyElement&gt; { ... });
        /// var rects = rubyText.GetRubyStringRects(rubyString, TextFindMode.All);
        ///
        /// foreach (var rectInfo in rects)
        /// {
        ///     foreach (var rect in rectInfo.Rects)
        ///     {
        ///         Debug.Log("BottomLeft: " + rect.min + ", TopRight: " + rect.max + ", String: " + rectInfo.TargetString);
        ///     }
        /// }
        /// </code>
        /// </example>
        public static List<TextRectInfo> GetRubyStringRects(
            this RubyTextMeshPro rubyText,
            RubyString rubyString,
            TextFindMode findMode = TextFindMode.First)
        {
            if (!rubyString.IsValid())
            {
                Debug.LogWarning("[RubyText] Invalid RubyString provided. Ensure all elements are valid.");
                return new List<TextRectInfo>();
            }

            // Convert RubyString into a single plain search string
            var searchString = rubyString.ToPlainString();

            return GetStringRects((TMP_Text)rubyText, searchString, findMode);
        }
#endif

        /// <summary>
        /// Internal method that processes the logic for retrieving Rects for a target string, line by line.
        /// </summary>
        private static bool TryGetStringRectsInternal(
            ITMPTextBase textBase,
            string targetString,
            TextFindMode findMode,
            out List<TextRectInfo> results)
        {
            results = new List<TextRectInfo>();

            if (textBase == null || string.IsNullOrEmpty(targetString))
                return false;

            // Force update to ensure the text mesh is updated
            textBase.ForceMeshUpdate();
            var textInfo = textBase.TextInfo;
            var plainText = textBase.GetParsedText();

            // Find occurrences of the target string in the plain text
            var startIndexes = FindStringOccurrences(plainText, targetString, findMode);
            if (startIndexes.Count == 0)
                return false;

            foreach (var startIndex in startIndexes)
            {
                // Calculate character indices for each occurrence
                var charIndexes = GetCharacterIndexes(textInfo, startIndex, targetString.Length);
                if (charIndexes.Count == 0) continue;

                // Split into lines and calculate Rect for each line
                var lineRects = CalculateLineBoundingRects(charIndexes, textInfo, textBase.Transform);

                if (lineRects.Count > 0)
                {
                    results.Add(new TextRectInfo
                    {
                        Rects = lineRects,
                        TargetString = targetString
                    });
                }
            }

            return results.Count > 0;
        }

        /// <summary>
        /// Calculates the bounding rectangles for the given character indices, grouped by lines.
        /// </summary>
        private static List<Rect> CalculateLineBoundingRects(
            List<int> charIndexes,
            TMP_TextInfo textInfo,
            Transform transform)
        {
            var rects = new List<Rect>();
            var lineGroups = charIndexes
                .GroupBy(index => textInfo.characterInfo[index].lineNumber)
                .OrderBy(group => group.Key);

            foreach (var lineGroup in lineGroups)
            {
                var lineCharIndexes = lineGroup.ToList();
                var rect = CalculateBoundingRect(lineCharIndexes, textInfo, transform);
                if (rect.HasValue)
                    rects.Add(rect.Value);
            }

            return rects;
        }

        /// <summary>
        /// Calculates the bounding rectangle for a list of character indices.
        /// </summary>
        private static Rect? CalculateBoundingRect(
            List<int> charIndexes,
            TMP_TextInfo textInfo,
            Transform transform)
        {
            if (charIndexes == null || charIndexes.Count == 0)
                return null;

            var firstCharInfo = textInfo.characterInfo[charIndexes[0]];
            var bottomLeft = transform.TransformPoint(firstCharInfo.bottomLeft);
            var topRight = transform.TransformPoint(firstCharInfo.topRight);

            foreach (var index in charIndexes.Skip(1))
            {
                var charInfo = textInfo.characterInfo[index];
                var charBottomLeft = transform.TransformPoint(charInfo.bottomLeft);
                var charTopRight = transform.TransformPoint(charInfo.topRight);

                bottomLeft = Vector2.Min(bottomLeft, charBottomLeft);
                topRight = Vector2.Max(topRight, charTopRight);
            }

            return Rect.MinMaxRect(bottomLeft.x, bottomLeft.y, topRight.x, topRight.y);
        }

        private static List<int> FindStringOccurrences(string plainText, string targetString, TextFindMode findMode)
        {
            var startIndexes = new List<int>();
            int currentIndex = 0;

            while (currentIndex < plainText.Length)
            {
                var foundIndex = plainText.IndexOf(targetString, currentIndex, StringComparison.Ordinal);
                if (foundIndex == -1) break;

                startIndexes.Add(foundIndex);
                if (findMode == TextFindMode.First) break;

                currentIndex = foundIndex + targetString.Length;
            }

            return startIndexes;
        }

        private static List<int> GetCharacterIndexes(TMP_TextInfo textInfo, int startIndex, int length)
        {
            var indexes = new List<int>();
            var plainIndex = 0;
            var inRichText = false;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                var charInfo = textInfo.characterInfo[i];
                var character = charInfo.character;

                if (character == '<') inRichText = true;
                if (!inRichText && plainIndex >= startIndex && plainIndex < startIndex + length)
                    indexes.Add(i);
                if (!inRichText) plainIndex++;

                if (character == '>' && inRichText) inRichText = false;
            }

            return indexes;
        }
    }
}