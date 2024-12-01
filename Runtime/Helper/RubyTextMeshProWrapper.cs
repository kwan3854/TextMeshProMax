#if RUBY_TEXT_SUPPORT
using TMPro;
using UnityEngine;

namespace Runtime.Helper
{
    public class RubyTextMeshProWrapper : ITMPTextBase
    {
        private RubyTextMeshProUGUI _rubyText;

        public RubyTextMeshProWrapper(RubyTextMeshProUGUI rubyText) => _rubyText = rubyText;
        public TMP_TextInfo TextInfo => _rubyText.textInfo;
        public Transform Transform => _rubyText.transform;
        public void ForceMeshUpdate() => _rubyText.ForceMeshUpdate();
        public string GetParsedText() => _rubyText.GetParsedText();
    }
}

#endif