using System;

namespace OriGame.Core
{
    public class ServiceLocatorException : Exception
    {
        public ServiceLocatorException(string message) : base(message)
        {}
    }
}

