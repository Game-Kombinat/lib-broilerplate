using System;

namespace Broilerplate.Tools.Fsm {
    public class StateMachineException : Exception {
        public StateMachineException(string msg) : base(msg) {
        }
    }
}