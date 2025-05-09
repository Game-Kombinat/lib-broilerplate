using System;

namespace Broilerplate.Data {
    [Serializable] //  todo: how can this be serializable? lol
    public class BlackboardEntry<T> {
        public BlackboardKey Key { get; }
        
        public T Value { get; }
        
        public Type ValueType { get; }

        public BlackboardEntry(BlackboardKey key, T value) {
            Key = key;
            Value = value;
            ValueType = typeof(T);
        }

        public override bool Equals(object obj) {
            return obj is BlackboardEntry<T> other && other.Key == Key;
        }

        public override int GetHashCode() {
            return Key.GetHashCode();
        }
    }
}