using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Broilerplate.Tools {
    public static class ListTools {
        public static bool IsEmpty(this IList list) {
            return list.Count == 0;
        }

        public static bool IsNullEmpty(this IList list) {
            return list == null || list.Count == 0;
        }

        public static bool Approx(this float val, float target) {
            return Mathf.Approximately(val, target);
        }
        
        /// <summary>
        /// Return a random number between min and this value (inclusive)
        /// </summary>
        /// <param name="val"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        public static int Random(this int val, int min = 0) {
            return UnityEngine.Random.Range(min, val + 1);
        }

        public static bool InRange<T>(this IEnumerable<T> source, int index) {
            return source != null && index >= 0 && index < source.Count();
        }

        public static T Random<T>(this IEnumerable<T> source) {
            var ts = source.ToList(); // apparently, if this already is a list, this isn't doing another allocation.
            if (ts.Count == 0) {
                return default;
            }

            return ts[UnityEngine.Random.Range(0, ts.Count)];
        }

        public static T Random<T>(this IEnumerable<T> source, System.Random rnd) {
            var ts = source.ToList(); // apparently, if this already is a list, this isn't doing another allocation.
            if (ts.Count == 0) {
                return default;
            }

            return ts[rnd.Next(0, ts.Count)];
        }
    }
}