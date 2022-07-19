using System;

namespace Broilerplate.Bt.Exceptions {

    public class IllegalReturnStatusException : BtException {

        public IllegalReturnStatusException(string msg) : base(msg) {
        }

        public IllegalReturnStatusException(string msg, Exception cause) : base(msg, cause) {
        }
    }
}