using System;
using System.Collections.Generic;

namespace Broilerplate.Data {
    /// <summary>
    /// It's a blackboard.
    /// It can be used to do funny things with behaviour trees.
    /// But you could use it in any which way.
    ///
    /// It's better than a data context and will replace it eventually.
    /// </summary>
    [Serializable]
    public class Blackboard {
        private Dictionary<BlackboardKey, object> entries = new();

        public bool TryGet<T>(BlackboardKey key, out T val) {
            if (entries.TryGetValue(key, out var entry) && entry is BlackboardEntry<T> cast) {
                val = cast.Value;
                return true;
            }

            val = default;
            return false;
        }

        public void Set<T>(BlackboardKey key, T value) {
            if (entries.TryGetValue(key, out var entry)) {
                if (entry is BlackboardEntry<T> bbt) {
                    bbt.Value = value;
                }
                else {
                    throw new BlackboardException($"Cannot set value of type {typeof(T)} to key {key} - it's bound to a different type.");
                }
            }
            else {
                entries[key] = new BlackboardEntry<T>(key, value);
            }
        }

        public bool ContainsKey(BlackboardKey key) => entries.ContainsKey(key);

        public void Remove(BlackboardKey key) => entries.Remove(key);
    }
}