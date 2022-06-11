using System;
using System.Collections.Generic;
using System.Linq;

namespace Broilerplate.Tools {
    public static class WeightedRandom {
        private static readonly Random Random = new Random();
        public static T Get<T>(IEnumerable<T> itemsEnumerable, Func<T, int> weightKey) {
            var items = itemsEnumerable.ToList();

            var totalWeight = items.Sum(weightKey);
            var targetWeight = Random.Next(totalWeight);
            var accumulatedWeight = 0;
            for (var i = 0; i < items.Count; i++) {
                var item = items[i];
                accumulatedWeight += weightKey(item);
                if (targetWeight <= accumulatedWeight)
                    return item;
            }

            return default;
        }
    }
}