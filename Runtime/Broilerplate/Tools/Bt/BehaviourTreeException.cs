using System;

namespace Broilerplate.Tools.Bt {
    public class BehaviourTreeException : Exception {
        public BehaviourTreeException(string msg) : base(msg) { }
    }
}