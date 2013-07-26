namespace Oclc.Exceptions
{
    using System;

    [Serializable()]
    public class AEException : Exception
    {
        public AEException() : base() { }
        public AEException(string message) : base(message) { }
        public AEException(string message, System.Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an 
        // exception propagates from a remoting server to the client.
        protected AEException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
    }
}
