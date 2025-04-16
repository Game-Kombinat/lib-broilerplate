using System.Collections.Generic;
using Broilerplate.Data;

namespace Broilerplate.Tools.Bt {
    /// <summary>
    /// Root node of a behaviour tree.
    /// This needs to be ticked somewhere outside to facilitate the ticking of everything else inside it.
    /// </summary>
    public class BehaviourTree : Node {
        private readonly List<Node> tickableTasks = new List<Node>();
        private readonly List<Node> insertionRequests = new List<Node>();
        private readonly List<Node> deletionRequests = new List<Node>();
        
        public Blackboard Blackboard { get; private set; }

        public override BehaviourTree Root => this;

        private RunMode mode;
        
        public BehaviourTree(string name) : base(name) {
        }
        
        protected override TaskStatus Process() {
            if (Status is TaskStatus.Running) {
                ProcessNodeRequests();
                // run nodes backwards because the order in which they were added is 
                // parent, child, child, grandchild, grandgrandchild, you get the idea.
                // We want to tick the children first.
                for (int i = tickableTasks.Count - 1; i >= 0 ; --i) {
                    tickableTasks[i].Tick();
                }
            }

            var status = tickableTasks.Count > 0 ? TaskStatus.Running : TaskStatus.Success;

            if (status == TaskStatus.Success && mode == RunMode.Repeat) {
                Despawn();
                Spawn();
                status = TaskStatus.Running;
            }
            
            return status;
        }

        public override void Spawn() {
            Reset();
            Status = TaskStatus.Running;
            ActiveChild.Spawn();
        }

        public override void Despawn() {
            Status = TaskStatus.Terminated;
            ActiveChild.Despawn();
        }

        public BehaviourTree UsingBlackboard(Blackboard bb) {
            Blackboard = bb;
            return this;
        }

        // just for semantics
        public BehaviourTree WithStartNode(Node node) {
            AddChild(node);
            return this;
        }

        public BehaviourTree InRunMode(RunMode runMode) {
            mode = runMode;
            return this;
        }

        /// <summary>
        /// Request to be ticked by the behaviour tree
        /// </summary>
        /// <param name="task"></param>
        public void RequestInsertion(Node task) {
            insertionRequests.Add(task);
        }

        /// <summary>
        /// Request that the ticking shall stop from the behaviour tree.
        /// </summary>
        /// <param name="task"></param>
        public void RequestDeletion(Node task) {
            deletionRequests.Add(task);
        }
        
        private void ProcessNodeRequests() {
            // Removals first because it can happen that we request a deletion and an addition
            // of the same node in the same frame and then we end up with a running node
            // that will not be ticked because it was removed after it was added.
            
            // Also better to avoid duplicate entries this way.
            for (var i = 0; i < deletionRequests.Count; i++) {
                tickableTasks.Remove(deletionRequests[i]);
            }
            
            for (var i = 0; i < insertionRequests.Count; i++) {
                tickableTasks.Add(insertionRequests[i]);
            }

            insertionRequests.Clear();
            deletionRequests.Clear();
        }
    }
}