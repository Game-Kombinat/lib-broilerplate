using System;
using System.Collections.Generic;
using UnityEngine;

namespace Broilerplate.Tools.Bt {
    /// <summary>
    /// It's a node in a behaviour tree.
    /// Could be anything at this point.
    /// </summary>
    public abstract class Node {
        public string Name { get; }
        public Node Parent { get; private set; }
        
        public TaskStatus Status { get; protected set; }

        public bool IsTerminated => Status == TaskStatus.Terminated;
        
        public IInterruptor Interruptor { get; protected set; }

        public bool HasInterruptor => Interruptor != null;

        private List<Node> children = new();

        public IReadOnlyList<Node> Children => children;
        
        // technically, root of every BT is the BT node. So there is that
        private BehaviourTree root;
        public virtual BehaviourTree Root {
            get {
                if (root != null) {
                    return root;
                }
                
                Node current = this;
                while (current != null) {
                    if (current is BehaviourTree t) {
                        root = t;
                        break;
                    }

                    current = Parent;
                }
                return root;
            }
        }

        protected Node(string name) {
            Name = name;
        }

        public Node AddChild(Node node) {
            node.Parent = this;
            children.Add(node);
            return this;
        }

        public Node WithInterruptor(IInterruptor interruptor) {
            Interruptor = interruptor;

            return this;
        }

        public void Tick() {
            if (IsTerminated) {
                return; // terminated tasks do nothing
            }

            if (HasInterruptor) {
                if (Interruptor.TestInterrupt()) {
                    Debug.Log($"Node {Name} was interrupted by {Interruptor.Name}.");
                    // if we got an interruptor and it returned success, kill this node
                    Root.RequestDeletion(this);
                    Status = TaskStatus.Failure;
                    return;
                }
            }

            var newStatus = Process();
            if (!ValidateInternalTickStatus(newStatus)) {
                throw new Exception($"{newStatus} cannot be returned by BaseNode.InternalTick()");
            }

            if (newStatus != TaskStatus.Running) {
                Root.RequestDeletion(this);
            }
            Status = newStatus;
        }

        protected abstract TaskStatus Process();

        public virtual void Reset() {
        }

        public virtual void Spawn() {
            Status = TaskStatus.Running;
            // Add to the insertion queue. The InternalSpawn process may remove it again
            // If the task does not need ticking.
            Root.RequestInsertion(this);
        }
        
        private static bool ValidateInternalTickStatus(TaskStatus status) {
            return !(status == TaskStatus.Terminated || status == TaskStatus.Uninitialised);
        }
    }
}