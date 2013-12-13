using System;

namespace LaCore.Hyperion.Adapters.QuickBloxIntegration
{
    [Serializable]
    public class QuickBloxException : Exception
    {
        public QuickBloxException() { }
        public QuickBloxException(string message) : base(message) { }
        public QuickBloxException(string message, Exception inner) : base(message, inner) { }
        protected QuickBloxException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
