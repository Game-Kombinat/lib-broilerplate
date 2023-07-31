using System;

namespace Broilerplate.Core.Exceptions {
    public class GameException : Exception {
        public GameException() : base() { }

        public GameException(string msg) : base(msg) { }
    }
}