using System.Collections.Generic;
using Broilerplate.Core;
using Broilerplate.Core.Components;
using UnityEngine;

namespace Broilerplate.Tools {
    public class RandomMultiActorPool<T> : ActorComponent where T : Actor {
        public struct WeightedEntry {
            public T prefab;
            public int weight;
        }
        
        [SerializeField]
        protected WeightedEntry[] poolingObject;
        
        [SerializeField]
        protected Transform targetParent;
        
        [SerializeReference, SubclassSelector]
        private IActorPoolPostProcessor postProcessor;
        
        [SerializeField]
        protected int poolSize;
        
        [SerializeField]
        protected bool loadPoolInBeginPlay = false;
        
        /// <summary>
        /// List of objects that are not live and can be retrieved from the pool.
        /// </summary>
        protected List<T> pooledObjects;

        /// <summary>
        /// List of objects we know were live the last time we checked
        /// </summary>
        protected readonly List<T> liveObjects = new List<T>();

        public override void BeginPlay() {
            base.BeginPlay();
            if (loadPoolInBeginPlay) {
                LoadPool();
            }
        }

        public void LoadPool() {
            // in a better implementation we could have x amount of pooled objects and pick one pool at random.
            // but this is okay too.
            pooledObjects = new List<T>(poolSize);
            for (int i = 0; i < poolSize; ++i) {
                T instantiatedActor;
                var prefab = WeightedRandom.Get(poolingObject, (x) => x.weight);
                if (targetParent) {
                    instantiatedActor = GetWorld().SpawnActor(prefab.prefab, targetParent);
                }
                else {
                    instantiatedActor = GetWorld().SpawnActor(prefab.prefab);
                }

                postProcessor?.PostProcessOnSpawn(instantiatedActor);

                instantiatedActor.SetGameObjectActive(false);
                pooledObjects.Add(instantiatedActor);
            }
        }
        
        public T Get() {
            return Get(out T geddit) ? geddit : null;
        }

        public bool Get(out T geddit) {
            // this handles 2 lists basically.
            // The reason is that if we were to just use the first inactive T in the pool
            // we run a chance that those will be the same and not random.
            // so we drain the pool first completely and then attempt to fill it up with
            // now inactive Ts from the overflow pool.
            // That will give us some sort of randomness created during the pool loading process.
            geddit = FindNext();
            if (!geddit) {
                UpdateLiveObjectsStatus();
                geddit = FindNext();
            }

            if (geddit) {
                pooledObjects.Remove(geddit);
                liveObjects.Add(geddit);
                return true;
            }
            Debug.Log("No inactive actors in pool.");
            return false;
        }

        private T FindNext() {
            for (int i = 0; i < pooledObjects.Count; ++i) {
                var actor = pooledObjects[i];
                if (!actor.gameObject.activeInHierarchy) {
                    if (postProcessor != null) {
                        postProcessor.PostProcessOnGet(actor);
                    }
                    return actor;
                }
            }

            return null;
        }

        private void UpdateLiveObjectsStatus() {
            for (int i = 0; i < liveObjects.Count; i++) {
                var actor = liveObjects[i];
                if (!actor.gameObject.activeInHierarchy) {
                    pooledObjects.Add(actor);
                    liveObjects.RemoveAt(i--);
                }
            }
        }
        
        public void ClearPool() {
            if (pooledObjects != null) {
                for (int i = 0; i < pooledObjects.Count; ++i) {
                    if (pooledObjects[i] != null) {
                        pooledObjects[i].Kill();
                    }
                }

                pooledObjects.Clear();
            }

            if (liveObjects != null) {
                for (int i = 0; i < liveObjects.Count; i++) {
                    liveObjects[i].Kill();
                }

                liveObjects.Clear();
            }
        }

        protected override void OnDestroy() {
            ClearPool();
            base.OnDestroy();
        }

        
    }
}