using System;

namespace GameKombinat.ControlFlow.Bt.Exceptions {

    internal class IllegalReturnStatusException : Exception {

        public IllegalReturnStatusException(string msg) : base(msg) {
        }

        public IllegalReturnStatusException(string msg, Exception cause) : base(msg, cause) {
        }
    }
}