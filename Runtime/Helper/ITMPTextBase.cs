using TMPro;
using UnityEngine;

namespace TextMeshProMax.Runtime.Helper
{
    internal interface ITMPTextBase
    {
        TMP_TextInfo TextInfo { get; }
        Transform Transform { get; }
        void ForceMeshUpdate();
        string GetParsedText();
    }
}