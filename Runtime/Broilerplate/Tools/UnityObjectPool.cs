using System.Collections.Generic;
using UnityEngine;

namespace Broilerplate.Tools {
    public abstract class UnityObjectPool<T> : MonoBehaviour where T : UnityEngine.Component {
        public T poolingObject;
        public PoolingPostProcessor postProcessor;
        public int poolSize;
        private List<T> pooledObjects;
        [SerializeField]
        private bool loadPoolInAwake = false;

        protected virtual void Awake() {
            if (loadPoolInAwake) {
                LoadPool();
            }
        }

        public void LoadPool() {
            pooledObjects = new List<T>(poolSize);
            for (int i = 0; i < poolSize; ++i) {
                var instantiatedObject = Instantiate(poolingObject, Vector3.zero, Quaternion.identity);
                if (postProcessor != null) {
                    postProcessor.PostProcessOnSpawn(instantiatedObject.gameObject);
                }

                instantiatedObject.gameObject.SetActive(false);
                pooledObjects.Add(instantiatedObject);
            }
        }

        public T Get() {
            for (int i = 0; i < pooledObjects.Count; ++i) {
                var component = pooledObjects[i];
                if (!component.gameObject.activeInHierarchy) {
                    if (postProcessor != null) {
                        postProcessor.PostProcessOnGet(component.gameObject);
                    }
                    return component;
                }
            }
            Debug.Log("No inactive objects in pool.");

            return null;
        }

        public bool Get(out T geddit) {
            geddit = null;
            for (int i = 0; i < pooledObjects.Count; ++i) {
                var component = pooledObjects[i];
                if (!component.gameObject.activeInHierarchy) {
                    if (postProcessor != null) {
                        postProcessor.PostProcessOnGet(component.gameObject);
                    }
                    geddit = component;
                    return true;
                }
            }
            Debug.Log("No inactive objects in pool.");

            return false;
        }

        private void OnDestroy() {
            for (int i = 0; i < pooledObjects.Count; ++i) {
                if (pooledObjects[i] != null) {
                    Destroy(pooledObjects[i].gameObject);
                }
            }
            pooledObjects.Clear();
        }
    }
}