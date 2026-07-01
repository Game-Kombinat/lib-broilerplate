using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZLinq;
using Random = System.Random;

namespace Broilerplate.Tools {
    /// <summary>
    /// Generic typed weighted random function.
    /// </summary>
    public static class WeightedRandom {
        private static readonly Random Random = new();
        public static T Get<T>(IEnumerable<T> itemsEnumerable, Func<T, int> weightKey) {
            return Get(itemsEnumerable, weightKey, Random);
        }
        
        /// <summary>
        /// IEnumerable version for random weight.
        /// </summary>
        /// <param name="itemsEnumerable"></param>
        /// <param name="weightKey"></param>
        /// <param name="rng"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static T Get<T>(IEnumerable<T> itemsEnumerable, Func<T, int> weightKey, Random rng) {
            if (itemsEnumerable is T[] array) {
                return Get(array, weightKey, rng);
            }
            
            // save on new list allocation as, very likely, we're already passing a list at this point
            var items = itemsEnumerable as IList<T> ?? itemsEnumerable.ToList();

            if (items.Count == 0) {
                return default;
            }
            
            var totalWeight = items.Sum(weightKey);
            var targetWeight = rng.Next(totalWeight);
            var accumulatedWeight = 0;
            for (var i = 0; i < items.Count; i++) {
                var item = items[i];
                accumulatedWeight += weightKey(item);
                if (targetWeight <= accumulatedWeight) {
                    return item;
                }
            }
            return default;
        }
        
        /// <summary>
        /// Array version for random weight
        /// </summary>
        /// <param name="itemsEnumerable"></param>
        /// <param name="weightKey"></param>
        /// <param name="rng"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static T Get<T>(T[] itemsEnumerable, Func<T, int> weightKey, Random rng) {
            var items = itemsEnumerable;
            if (items.Length == 0) {
                return default;
            }
            var totalWeight = items.Sum(weightKey);
            var targetWeight = rng.Next(totalWeight);
            var accumulatedWeight = 0;
            for (var i = 0; i < items.Length; i++) {
                var item = items[i];
                accumulatedWeight += weightKey(item);
                if (targetWeight <= accumulatedWeight) {
                    return item;
                }
            }
            return default;
        }

        public static T RandomWithWeight<T>(this IEnumerable<T> itemsEnumerable, Func<T, int> weightKey, Random rng) {
            return Get(itemsEnumerable, weightKey, rng);
        }
        
        public static T RandomWithWeight<T>(this IEnumerable<T> itemsEnumerable, Func<T, int> weightKey) {
            return Get(itemsEnumerable, weightKey);
        }
        
        /// <summary>
        /// ZLinq based weighted random pick. 
        /// </summary>
        public static T RandomWithWeight<TEnumerator, T>(this ValueEnumerable<TEnumerator, T> source, Func<T, int> weightKey, Random rng)
            where TEnumerator : struct, IValueEnumerator<T> {

            var enumerator = source.Enumerator;
            try {
                // array or list - sources expose a span, we can go with the double-phase approach that we have for vanilla enumerables.
                // This is faster-ish because it doesn't need to enumerate.
                if (enumerator.TryGetSpan(out var span)) {
                    return GetFromSpan(span, weightKey, rng);
                }

                // Otherwise use the "Algorithm A-Chao" which I stole from this here wikipedia page and butchered some:
                // https://en.wikipedia.org/wiki/Reservoir_sampling
                T selected = default;
                var totalWeight = 0;
                while (enumerator.TryGetNext(out var item)) {
                    var weight = weightKey(item);
                    if (weight <= 0) {
                        continue;
                    }
                    totalWeight += weight;
                    if (rng.Next(totalWeight) < weight) { // fires with probability weight / totalWeight
                        selected = item;
                    }
                }
                return selected;
            }
            finally {
                enumerator.Dispose(); // this is all struct stuff so this was a copy and needs to be disposed as it is a disposable
            }
        }

        public static T RandomWithWeight<TEnumerator, T>(this ValueEnumerable<TEnumerator, T> source, Func<T, int> weightKey)
            where TEnumerator : struct, IValueEnumerator<T> {
            return source.RandomWithWeight(weightKey, Random);
        }

        private static T GetFromSpan<T>(ReadOnlySpan<T> items, Func<T, int> weightKey, Random rng) {
            if (items.Length == 0) {
                return default;
            }
            var totalWeight = 0;
            for (var i = 0; i < items.Length; i++) {
                totalWeight += weightKey(items[i]);
            }
            if (totalWeight <= 0) {
                return default;
            }
            var targetWeight = rng.Next(totalWeight);
            var accumulatedWeight = 0;
            for (var i = 0; i < items.Length; i++) {
                accumulatedWeight += weightKey(items[i]);
                if (targetWeight < accumulatedWeight) {
                    return items[i];
                }
            }
            return default;
        }

        public static IEnumerable<T> OrderWeightedRandomSequence<T>(this IEnumerable<T> itemsEnumerable, Func<T, int> weightKey) {
            return OrderWeightedRandomSequence(itemsEnumerable, weightKey, Random);
        }
        
        public static IEnumerable<T> OrderWeightedRandomSequence<T>(this IEnumerable<T> itemsEnumerable, Func<T, int> weightKey, Random rng) {
            return itemsEnumerable
                .Select(x => {
                    float weight = weightKey(x);
                    float noise = (float)rng.NextDouble();
                    float key = Mathf.Pow(noise, 1f / weight);
                    return (item: x, key);
                })
                .OrderByDescending(x => x.key)
                .Select(x => x.item);
        }
    }
}