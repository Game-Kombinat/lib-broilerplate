using System.Collections.Generic;
using Broilerplate.Core.Components;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Broilerplate.Tools {
    /// <summary>
    /// Conceptually similar to a pool.
    /// However, objects retrieved from the dispenser will never be returned.
    /// Thus it can be depleted. Which is the whole point.
    /// </summary>
    public class ObjectDispenser<T> : ActorComponent where T : Component {
        /// <summary>
        /// The objects that will be spawned when the dispenser is loaded.
        /// They are then put into an inventory list which will de depleted by using the dispenser.
        /// </summary>
        [SerializeField]
        private List<T> loadout;

        [SerializeField]
        private bool loadOnBeginPlay;

        private Queue<T> instances = new();

        private List<T> dispensedObjects = new();
        
        public bool IsDepleted => instances.Count == 0;

        public override void BeginPlay() {
            base.BeginPlay();
            if (loadOnBeginPlay) {
                LoadPool();
            }
        }

        public void SetLoadout(List<T> newLoadout) {
            if (!IsDepleted) {
                Clear();
            }
            
            loadout = newLoadout;
        }

        public void Clear() {
            // take out all objects from queue
            while (!IsDepleted) {
                Get();
            }

            for (int i = 0; i < dispensedObjects.Count; i++) {
                Destroy(dispensedObjects[i].gameObject);
            }
        }

        public void LoadPool() {
            for (int i = 0; i < loadout.Count; i++) {
                var go = GetWorld().SpawnActor(loadout[i].gameObject);
                go.SetActive(false);
                instances.Enqueue(go.GetComponent<T>());
            }
        }

        public T Get() {
            if (IsDepleted) {
                return default;
            }

            var i = instances.Dequeue();
            dispensedObjects.Add(i);
            return i;
        }

        protected override void OnDestroy() {
            Clear();
            base.OnDestroy();
        }
    }
}