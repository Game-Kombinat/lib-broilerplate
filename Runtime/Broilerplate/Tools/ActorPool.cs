using System.Collections.Generic;
using Broilerplate.Core;
using Broilerplate.Core.Components;
using UnityEngine;

namespace Broilerplate.Tools {
    
    /// <summary>
    /// Actor Component that spawns other actors.
    /// </summary>
    public class ActorPool<T> : ActorComponent where T : Actor {
        [SerializeField]
        private T poolingObject;
        
        [SerializeReference, SubclassSelector]
        private IActorPoolPostProcessor postProcessor;
        
        [SerializeField]
        private int poolSize;
        
        [SerializeField]
        protected bool loadPoolInBeginPlay = false;
        [SerializeField]
        protected bool instantiateInParent;
        
        private List<T> pooledObjects;

        public override void BeginPlay() {
            base.BeginPlay();
            if (loadPoolInBeginPlay) {
                LoadPool();
            }
        }

        public void LoadPool() {
            pooledObjects = new List<T>(poolSize);
            for (int i = 0; i < poolSize; ++i) {
                T instantiatedActor;
                if (instantiateInParent) {
                    instantiatedActor = GetWorld().SpawnActor(poolingObject, poolingObject.transform.parent);
                }
                else {
                    instantiatedActor = GetWorld().SpawnActor(poolingObject);
                }

                postProcessor?.PostProcessOnSpawn(instantiatedActor);

                instantiatedActor.SetGameObjectActive(false);
                pooledObjects.Add(instantiatedActor);
            }
        }
        
        public T Get() {
            for (int i = 0; i < pooledObjects.Count; ++i) {
                var actor = pooledObjects[i];
                if (!actor.gameObject.activeInHierarchy) {
                    postProcessor?.PostProcessOnGet(actor);
                    return actor;
                }
            }
            Debug.Log("No inactive actors in pool.");

            return null;
        }

        public bool Get(out T geddit) {
            geddit = null;
            for (int i = 0; i < pooledObjects.Count; ++i) {
                var actor = pooledObjects[i];
                if (!actor.gameObject.activeInHierarchy) {
                    if (postProcessor != null) {
                        postProcessor.PostProcessOnGet(actor);
                    }
                    geddit = actor;
                    return true;
                }
            }
            Debug.Log("No inactive actors in pool.");

            return false;
        }

        protected override void OnDestroy() {
            for (int i = 0; i < pooledObjects.Count; ++i) {
                if (pooledObjects[i] != null) {
                    pooledObjects[i].Kill();
                }
            }
            pooledObjects.Clear();
            base.OnDestroy();
        }
    }
}