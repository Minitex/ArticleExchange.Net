namespace Oclc.Exceptions
{
    using System;

    [Serializable()]
    public class WSKeyClientException : Exception
    {
        public WSKeyClientException() : base() { }
        public WSKeyClientException(string message) : base(message) { }
        public WSKeyClientException(string message, System.Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an 
        // exception propagates from a remoting server to the client.
        protected WSKeyClientException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
    }
}
