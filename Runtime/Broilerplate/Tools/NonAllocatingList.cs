using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Broilerplate.Tools {
    /// <summary>
    /// It's a list with a fixed set of elements in it.
    /// This list will, past its initialisation, not cause any extra allocations.
    /// It has a fixed size and that size cannot be changed during the lifetime of the object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NonAllocatingList<T> : IList<T> {
        /// <summary>
        /// The maximum length of this list.
        /// </summary>
        public int MaxSize { get; }

        /// <summary>
        /// Returns the fixed sizes current length.
        /// </summary>
        public int Count { get; private set; }
        
        public bool IsReadOnly => false;

        private readonly List<T> internalList; // Because I'm not THAT insane
        
        public NonAllocatingList(int size) {
            MaxSize = size;
            internalList = new List<T>(size);
            // Pre-fill with defaults because capacity doesn't cut it
            for (int i = 0; i < MaxSize; i++) {
                internalList.Add(default);
            }

            Clear();
        }

        /// <summary>
        /// Call this when re-populating this list.
        /// Otherwise you may run into trouble
        /// </summary>
        public void ResetIndex() {
            Count = 0;
        }
        
        public void Add(T item) {
            internalList[Count] = item; // this will throw out of range if Count is wrong
            Count++; // only increment after add because if exception is thrown we don't overshoot max limit
        }

        public void Clear() {
            ResetIndex();
        }
        public bool Contains(T item) {
            var index = internalList.IndexOf(item); // see if we have this item
            return RangeCheckNoThrow(index); // then see if we're in currently valid range
        }

        public void CopyTo(T[] array, int arrayIndex) {
            internalList.CopyTo(array, arrayIndex);
        }
        public bool Remove(T item) {
            throw new NotSupportedException("Fixed list cannot be removed from! Use ResetIndex() to clear the list!");
        }

        public IEnumerator<T> GetEnumerator() {
            return internalList.Take(Count).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public int IndexOf(T item) {
            return internalList.IndexOf(item);
        }

        public void Insert(int index, T item) {
            throw new NotSupportedException("Fixed list cannot be inserted into!");
        }

        public void RemoveAt(int index) {
            throw new NotSupportedException("Fixed list cannot be removed at!");
        }

        public T this[int index] {
            get {
                RangeCheck(index);
                return internalList[index];
            }
            set {
                RangeCheck(index);
                internalList[index] = value;
            }
        }

        private void RangeCheck(int index) {
            if (index < Count && index >= 0) {
                return;
            }

            throw new IndexOutOfRangeException($"{index} is out of range on list from 0 to {Count} (internal list size {internalList.Count})");
        }
        
        private bool RangeCheckNoThrow(int index) {
            if (index < Count && index >= 0) {
                return true;
            }

            return false;
        }

        public bool HasCapacity() {
            return Count < MaxSize;
        }

        // at least as non-allocating as possible
        #region non allocating linq-like comfort methods
        public bool Any() {
            return Count > 0;
        }

        public bool Any([NotNull]Predicate<T> predicate) {
            for (int i = 0; i < Count; i++) {
                if (predicate(internalList[i])) {
                    return true;
                }
            }

            return false;
        }
        
        #endregion
    }
}