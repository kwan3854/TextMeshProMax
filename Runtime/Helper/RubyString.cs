#if RUBY_TEXT_SUPPORT
using System;
using System.Collections.Generic;
using System.Linq;

namespace Runtime.Helper
{
    public struct RubyString
    {
        public List<RubyElement> Elements { get; private set; }

        public RubyString(List<RubyElement> elements)
        {
            Elements = elements ?? throw new ArgumentNullException(nameof(elements));
        }

        public string ToPlainString()
        {
            return string.Join("", Elements.Select(e => e.ToPlainString()));
        }

        public bool IsValid()
        {
            return Elements != null && Elements.All(e => e.IsValid());
        }

        public override string ToString()
        {
            return string.Join(" | ", Elements.Select(e => e.ToString()));
        }
    }
}
#endif