using System.Collections.Generic;
using UnityEngine;

namespace GameKombinat.ControlFlow.Bt.Data {
    /// <summary>
    /// A storage for GameObjects and components.
    /// This is volatile data and does not persist anywhere in any way.
    ///
    /// Used in Behaviour Trees to store runtime information that is otherwise not serializable.
    /// </summary>
    public class VolatileDataStore {
        private Dictionary<string, GameObject> dataStore = new Dictionary<string, GameObject>(5);

        public T Get<T>(string name) {
            if (dataStore.TryGetValue(name, out GameObject obj)) {
                return obj.GetComponent<T>();
            }
            Debug.LogWarning($"Trying to get component ({typeof(T)}) from object {name} but that does not exist.");
            return default;
        }

        public GameObject Get(string name) {
            if (dataStore.TryGetValue(name, out GameObject obj)) {
                return obj;
            }
            Debug.LogWarning($"Trying to get object {name} but that does not exist.");
            return default;
        }

        public void Set(string name, Component comp) {
            Set(name, comp.gameObject);
        }

        public void Set(string name, GameObject go) {
            dataStore[name] = go;
        }
    }
}