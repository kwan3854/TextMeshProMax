using System;
using System.Collections.Generic;
using UnityEngine;

namespace TextMeshProMax.Runtime.Helper
{
    /// <summary>
    /// Contains information about the Rect values of a specific string.
    /// </summary>
    [Serializable]
    public class TextRectInfo
    {
        /// <summary>
        /// Gets or sets the list of Rect values for the specified string.
        /// </summary>
        public List<Rect> Rects { get; set; }

        /// <summary>
        /// Gets or sets the target string for which the Rect values are calculated.
        /// </summary>
        public string TargetString { get; set; }
    }
}