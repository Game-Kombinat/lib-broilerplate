using System;
using System.Collections.Generic;
using System.Linq;
using Broilerplate.Bt.Data;
using Broilerplate.Bt.Nodes;
using Broilerplate.Core;
using Broilerplate.Data;
using UnityEngine;
using XNode;

namespace Broilerplate.Bt {
    [CreateAssetMenu(fileName = "Behaviour Tree", menuName = "Game Kombinat/Ai/Behaviour Tree")]
    [RequireNode(typeof(RootNode))]
    public class BehaviourTree : NodeGraph {
        [SerializeField]
        private BehaviourTree parent;

        public bool HasParent => parent != null;

        public BehaviourTree Parent => parent;

        public bool IsRoot => Root == this;
        public BehaviourTree Root => parent != null ? parent.Root : this;

        public Actor Actor {
            get;
            set;
        }
        
        [SerializeField]
        private DataContext dataContext;
        public DataContext Data {
            get => parent != null ? parent.Data : dataContext;
            private set {
                if (parent != null) {
                    parent.Data = value;
                }
                else {
                    dataContext = value;
                }
            } 
        }

        [NonSerialized]
        private VolatileDataStore volatileData = new VolatileDataStore();
        public VolatileDataStore VolatileData {
            get => parent != null ? parent.VolatileData : volatileData;
            private set {
                if (parent != null) {
                    parent.VolatileData = value;
                }
                else {
                    volatileData = value;
                }
            }
        }

        public TaskStatus Status => root != null ? root.Status : TaskStatus.Uninitialised;
        
        public RootNode RootNode => nodes.FirstOrDefault(x => x is RootNode) as RootNode;
        public bool IsRunning => isRunning;

        private BaseNode root;

        private readonly IList<BaseNode> tickableTasks = new List<BaseNode>();
        private readonly IList<BaseNode> tickableTasksInsertionQueue = new List<BaseNode>();
        private readonly IList<BaseNode> tickableTasksDeletionQueue = new List<BaseNode>();
        
        private bool isInitialised;
        private bool isRunning;
        
        public void RequestTickableInsertion(BaseNode task) {
            tickableTasksInsertionQueue.Add(task);
        }

        public void RequestTickableRemoval(BaseNode task) {
            tickableTasksDeletionQueue.Add(task);
        }

        public void SetParent(BehaviourTree parent) {
            this.parent = parent;
        }

        public void Prepare() {
            if (Data == null) {
                Data = CreateInstance<DataContext>();
            }
        }
        
        public void Begin() {
            Prepare();

            isRunning = true;
        }

        public void Pause() {
            if (!isInitialised) {
                Debug.LogWarning("Attempted to pause a non-initialised behaviour tree. Ignoring");
                return;
            }
            isRunning = false;
        }

        public void Resume() {
            if (!isInitialised) {
                Debug.LogWarning("Attempted to resume a non-initialised behaviour tree. Ignoring");
                return;
            }
            isRunning = true;
        }

        public void Tick() {
            if (!isRunning) {
                return;
            }
            TaskStatus currentStatus = Status;
            if (currentStatus == TaskStatus.Running || currentStatus == TaskStatus.Uninitialised) {
                if (!isInitialised) {
                    root = RootNode;
                    root.Spawn();
                    isInitialised = true;
                }
                ProcessQueues();
                for (int i = 0; i < tickableTasks.Count; ++i) {
                    tickableTasks[i].Tick();
                }
            }
        }

        public void Terminate() {
            if (root != null) {
                root.Terminate();
            }
            Reset();
        }
        
        private void Reset() {
            isRunning = false;
            isInitialised = false;
            // Makes sure GetStatus returns UNINITIALISED next time
            root = null;
            
            tickableTasks.Clear();
            tickableTasksDeletionQueue.Clear();
            tickableTasksInsertionQueue.Clear();
        }
        
        private void ProcessQueues() {
            // Removals first because it can happen that we request a deletion and an addition
            // of the same node in the same frame and then we end up with a running node
            // that will not be ticked because it was removed after it was added.
            
            // Also better to avoid duplicate entries this way.
            for (var i = 0; i < tickableTasksDeletionQueue.Count; i++) {
                tickableTasks.Remove(tickableTasksDeletionQueue[i]);
            }
            
            for (var i = 0; i < tickableTasksInsertionQueue.Count; i++) {
                tickableTasks.Add(tickableTasksInsertionQueue[i]);
            }

            tickableTasksInsertionQueue.Clear();
            tickableTasksDeletionQueue.Clear();
        }
    }
}