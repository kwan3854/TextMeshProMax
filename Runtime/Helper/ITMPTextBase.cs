using TMPro;
using UnityEngine;

namespace Runtime.Helper
{
    public interface ITMPTextBase
    {
        TMP_TextInfo TextInfo { get; }
        Transform Transform { get; }
        void ForceMeshUpdate();
        string GetParsedText();
    }
}