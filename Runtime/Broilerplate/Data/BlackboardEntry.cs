using System;

namespace Broilerplate.Data {
    /// <summary>
    /// This is a class because the dictionary in which it is used
    /// is storing object types so if this was a struct, it would get boxed anyway.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BlackboardEntry<T> {
        public BlackboardKey Key { get; }

        private T actualValue;
        
        public T Value { get; set; }
        
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