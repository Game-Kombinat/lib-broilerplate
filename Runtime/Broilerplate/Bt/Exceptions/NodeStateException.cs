using System;

namespace Broilerplate.Bt.Exceptions {
    public class NodeStateException : BtException {
        
        public NodeStateException(string msg) : base(msg) {
        }

        public NodeStateException(string msg, Exception cause) : base(msg, cause) {
        }
    }
}