using System;

namespace Broilerplate.Data {
    public class BlackboardException : Exception {
        public BlackboardException(string msg) : base(msg) { }
    }
}