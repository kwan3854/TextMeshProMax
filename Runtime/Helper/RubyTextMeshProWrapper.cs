#if RUBY_TEXT_SUPPORT
using TMPro;
using UnityEngine;

namespace Runtime.Helper
{
    public class RubyTextMeshProWrapper : ITMPTextBase
    {
        private TMP_Text _rubyText;

        public RubyTextMeshProWrapper(TMP_Text rubyText) => _rubyText = rubyText;
        public TMP_TextInfo TextInfo => _rubyText.textInfo;
        public Transform Transform => _rubyText.transform;
        public void ForceMeshUpdate() => _rubyText.ForceMeshUpdate();
        public string GetParsedText() => _rubyText.GetParsedText();
    }
}

#endif