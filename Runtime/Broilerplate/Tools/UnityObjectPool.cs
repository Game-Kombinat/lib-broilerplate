using System.Collections.Generic;
using UnityEngine;

namespace Broilerplate.Tools {
    
    /// <summary>
    /// Generic object pool specific for Unity component types.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class UnityObjectPool<T> : MonoBehaviour where T : UnityEngine.Component {
        public T poolingObject;
        [SerializeReference, SubclassSelector]
        public IComponentPoolPostProcessor postProcessor;
        public int poolSize;
        
        [SerializeField]
        protected bool loadPoolInAwake = false;
        [SerializeField]
        protected bool instantiateInParent;
        
        protected List<T> pooledObjects;

        protected virtual void Awake() {
            if (loadPoolInAwake) {
                LoadPool();
            }
        }

        public virtual void LoadPool() {
            pooledObjects = new List<T>(poolSize);
            for (int i = 0; i < poolSize; ++i) {
                T instantiatedObject;
                if (instantiateInParent) {
                    instantiatedObject = Instantiate(poolingObject, poolingObject.transform.parent);
                }
                else {
                    instantiatedObject = Instantiate(poolingObject);
                }

                postProcessor?.PostProcessOnSpawn(instantiatedObject);

                instantiatedObject.gameObject.SetActive(false);
                pooledObjects.Add(instantiatedObject);
            }
        }

        public T Get() {
            for (int i = 0; i < pooledObjects.Count; ++i) {
                var component = pooledObjects[i];
                if (!component.gameObject.activeInHierarchy) {
                    postProcessor?.PostProcessOnGet(component);
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
                        postProcessor.PostProcessOnGet(component);
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