using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Runtime.Helper
{
    public static class TextMeshProExtensions
    {
        /// <summary>
        /// Returns the Rect information of a specific string.
        /// </summary>
        /// <param name="text">The target TMP_Text object</param>
        /// <param name="targetString">The string to find</param>
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
            if (TryGetStringRects(text, targetString, findMode, out var results))
            {
                return results;
            }

            return new List<TextRectInfo>();
        }

        /// <summary>
        /// Tries to get the list of TextRectInfo for the specified string in the text.
        /// </summary>
        /// <param name="text">The target TMP_Text object</param>
        /// <param name="targetString">The string to find</param>
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
        /// <returns>True if the string is found; otherwise, false.</returns>
        /// <example>
        /// <code>
        /// TMP_Text text = GetComponent&lt;TMP_Text&gt;();
        /// if (text.TryGetStringRects("Hello", TextFindMode.All, out var rects))
        /// {
        ///     foreach (var rectInfo in rects)
        ///     {
        ///         foreach (var rect in rectInfo.Rects)
        ///         {
        ///             Debug.Log("BottomLeft: " + rect.min + ", TopRight: " + rect.max + ", String: " + rectInfo.TargetString);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public static bool TryGetStringRects(
            this TMP_Text text,
            string targetString,
            TextFindMode findMode,
            out List<TextRectInfo> results)
        {
            results = new List<TextRectInfo>();

            if (text == null || string.IsNullOrEmpty(targetString))
                return false;

            text.ForceMeshUpdate();

            var textInfo = text.textInfo;
            var characterCount = textInfo.characterCount;

            // 1. Find the positions of the target string
            var plainText = text.GetParsedText();
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

            // Return false if the target string is not found
            if (startIndexes.Count == 0)
                return false;

            // 2. Calculate the Rect for each found string
            foreach (var startIndex in startIndexes)
            {
                var rects = new List<Rect>();
                var charIndexes = GetCharacterIndexes(textInfo, startIndex, targetString.Length);

                foreach (var line in textInfo.lineInfo)
                {
                    var lineRects = GetLineRects(charIndexes, textInfo, line, text.transform);
                    if (lineRects != null)
                        rects.Add(lineRects.Value);
                }

                if (rects.Count > 0)
                {
                    results.Add(new TextRectInfo
                    {
                        Rects = rects,
                        TargetString = targetString
                    });
                }
            }

            return true;
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

        private static Rect? GetLineRects(List<int> targetCharIndexes, TMP_TextInfo textInfo, TMP_LineInfo lineInfo,
            Transform transform)
        {
            var lineCharIndexes = targetCharIndexes
                .Where(index => index >= lineInfo.firstCharacterIndex && index <= lineInfo.lastCharacterIndex)
                .ToList();

            if (lineCharIndexes.Count == 0) return null;

            var firstCharInfo = textInfo.characterInfo[lineCharIndexes[0]];
            var bottomLeft = transform.TransformPoint(firstCharInfo.bottomLeft);
            var topRight = transform.TransformPoint(firstCharInfo.topRight);

            foreach (var index in lineCharIndexes.Skip(1))
            {
                var charInfo = textInfo.characterInfo[index];
                var charBottomLeft = transform.TransformPoint(charInfo.bottomLeft);
                var charTopRight = transform.TransformPoint(charInfo.topRight);

                if (charBottomLeft.x < bottomLeft.x)
                    bottomLeft.x = charBottomLeft.x;
                if (charBottomLeft.y < bottomLeft.y)
                    bottomLeft.y = charBottomLeft.y;
                if (charTopRight.x > topRight.x)
                    topRight.x = charTopRight.x;
                if (charTopRight.y > topRight.y)
                    topRight.y = charTopRight.y;
            }

            return Rect.MinMaxRect(bottomLeft.x, bottomLeft.y, topRight.x, topRight.y);
        }
    }
}