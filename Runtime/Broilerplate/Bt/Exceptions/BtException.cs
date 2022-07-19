using System;

namespace Broilerplate.Bt.Exceptions {
    public class BtException : Exception {

        public BtException(string msg) : base(msg) {
        }

        public BtException(string msg, Exception cause) : base(msg, cause) {
        }
    }
}