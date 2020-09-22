using System;

namespace TMRI.Primitives
{
    public class TMRIException : Exception
    {
        public TMRIException(string message) : base(message) { }

        public TMRIException(string message, Exception innerException) : base(message, innerException) { }
    }
}
