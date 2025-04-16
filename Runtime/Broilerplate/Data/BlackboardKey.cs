using System;
using Broilerplate.Tools;

namespace Broilerplate.Data {
    [Serializable]
    public readonly struct BlackboardKey : IEquatable<BlackboardKey> {
        private readonly string name;
        private readonly int hashedKey;

        public BlackboardKey(string keyName) {
            name = keyName;
            hashedKey = name.GetFnv1aHash();
        }
        
        public bool Equals(BlackboardKey other) {
            return hashedKey == other.hashedKey;
        }

        public override bool Equals(object obj) {
            return obj is BlackboardKey other && Equals(other);
        }

        public override int GetHashCode() {
            return hashedKey;
        }

        public override string ToString() {
            return name;
        }

        public static bool operator ==(BlackboardKey lhs, BlackboardKey rhs) => lhs.hashedKey == rhs.hashedKey;

        public static bool operator !=(BlackboardKey lhs, BlackboardKey rhs) => !(lhs == rhs);
        
        public static implicit operator BlackboardKey(string val) => new (val);
        public static implicit operator string(BlackboardKey val) => val.name;
        public static implicit operator int(BlackboardKey val) => val.hashedKey;
    }
}