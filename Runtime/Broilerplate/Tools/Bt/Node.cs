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

                    current = current.Parent;
                }
                return root;
            }
        }

        protected Node(string name) {
            Name = name;
        }

        public virtual Node  AddChild(Node node) {
            SetParentOn(node);
            children.Add(node);
            return this;
        }

        protected void SetParentOn(Node node) {
            node.Parent = this;
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
                    Interrupt(Interruptor.InterruptParent);
                    return;
                }
            }

            var newStatus = Process();
            if (!ValidateInternalTickStatus(newStatus)) {
                throw new BehaviourTreeException($"{newStatus} cannot be returned by {Name}.Process()");
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

        public void Interrupt(bool includingParent) {
            if (includingParent && Parent != null) {
                // this will call down back here, so bail already
                Parent.Interrupt(false);
                return;
            }
            
            Despawn();
            for (int i = 0; i < Children.Count; i++) {
                // even if we did interrupt including parent, we are the parent at this point.
                // therefore, false it is.
                Children[i].Interrupt(false);
            }
        }
        
        private static bool ValidateInternalTickStatus(TaskStatus status) {
            return status is not (TaskStatus.Terminated or TaskStatus.Uninitialised);
        }
    }
}