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
        
        public Interruptor Interruptor { get; protected set; }

        public bool HasInterruptor => Interruptor != null;

        private readonly List<Node> children = new();

        public IReadOnlyList<Node> Children => children;

        protected int currentChild;

        public Node ActiveChild => Children[currentChild];
        
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

        public virtual Node AddChild(Node node) {
            node.Parent = this;
            children.Add(node);
            return this;
        }

        public Node WithInterruptor(Interruptor interruptor) {
            Interruptor = interruptor;
            interruptor.SetNode(this);
            return this;
        }

        public void Tick() {
            if (IsTerminated) {
                return; // terminated tasks do nothing
            }

            if (HasInterruptor) {
                if (Interruptor.TestInterrupt()) {
                    Debug.Log($"Node {Name} was interrupted by {Interruptor.Name}.");
                    Interrupt();
                    return;
                }
            }

            var newStatus = Process();
            if (!ValidateInternalTickStatus(newStatus)) {
                throw new BehaviourTreeException($"{newStatus} cannot be returned by Node.Process()");
            }

            if (newStatus != TaskStatus.Running) {
                Despawn();
            }
            Status = newStatus;
        }

        protected abstract TaskStatus Process();

        public virtual void Reset() {
            currentChild = 0;
            Status = TaskStatus.Uninitialised;
        }

        public virtual void Spawn() {
            Reset();
            Status = TaskStatus.Running;
            // Add to the insertion queue. The InternalSpawn process may remove it again
            // If the task does not need ticking.
            Root.RequestInsertion(this);
        }

        public virtual void Despawn() {
            Status = TaskStatus.Terminated;
            Root.RequestDeletion(this);
        }

        public void Interrupt() {
            Despawn();

            for (int i = 0; i < Children.Count; i++) {
                Children[i].Interrupt();
            }
        }
        
        private static bool ValidateInternalTickStatus(TaskStatus status) {
            return status is not (TaskStatus.Terminated or TaskStatus.Uninitialised);
        }
    }
}