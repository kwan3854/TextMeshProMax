using TMPro;
using UnityEngine;

namespace TextMeshProMax.Runtime.Helper
{
    internal class TMPTextWrapper : ITMPTextBase
    {
        public TMPTextWrapper(TMP_Text text) => _text = text;
        public TMP_TextInfo TextInfo => _text.textInfo;
        public Transform Transform => _text.transform;
        public void ForceMeshUpdate() => _text.ForceMeshUpdate();
        public string GetParsedText() => _text.GetParsedText();
        
        private readonly TMP_Text _text;
    }
}