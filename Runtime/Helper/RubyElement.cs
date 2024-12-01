#if RUBY_TEXT_SUPPORT
namespace Runtime.Helper
{
    public struct RubyElement
    {
        public string Ruby { get; private set; }
        public string Body { get; private set; }
        public bool IsPlainText { get; private set; }

        public RubyElement(string body, string ruby = null)
        {
            Body = body;
            Ruby = ruby;
            IsPlainText = ruby == null;
        }

        public string ToPlainString()
        {
            return IsPlainText ? Body : $"{Body}{Ruby}";
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Body) || IsPlainText;
        }

        public override string ToString()
        {
            return IsPlainText ? $"PlainText: {Body}" : $"Ruby: {Ruby}, Body: {Body}";
        }
    }
}
#endif