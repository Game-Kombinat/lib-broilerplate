using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace Broilerplate.Tools {
    /// <summary>
    /// Generic typed weighted random function.
    /// </summary>
    public static class WeightedRandom {
        private static readonly Random Random = new Random();
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