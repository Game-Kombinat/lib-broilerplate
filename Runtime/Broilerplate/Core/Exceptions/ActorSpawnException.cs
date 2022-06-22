using System;

namespace Broilerplate.Core.Exceptions
{
    public class ActorSpawnException : Exception
    {
        public ActorSpawnException(string msg) : base(msg) { }
    }
}