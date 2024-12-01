using TMPro;
using UnityEngine;

namespace Runtime.Helper
{
    public class TMPTextWrapper : ITMPTextBase
    {
        private TMP_Text _text;

        public TMPTextWrapper(TMP_Text text) => _text = text;
        public TMP_TextInfo TextInfo => _text.textInfo;
        public Transform Transform => _text.transform;
        public void ForceMeshUpdate() => _text.ForceMeshUpdate();
        public string GetParsedText() => _text.GetParsedText();
    }
}