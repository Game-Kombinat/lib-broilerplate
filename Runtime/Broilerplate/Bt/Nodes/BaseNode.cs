using Broilerplate.Bt.Data;
using Broilerplate.Bt.Exceptions;
using Broilerplate.Bt.Nodes.Ports;
using UnityEngine;
using XNode;

namespace Broilerplate.Bt.Nodes {
    public abstract class BaseNode : Node {
        [Input]
        public Port parent;
        /// <summary>
        /// If connected, the Tick method will evaluate this interruptor before
        /// ticking the actual node. if the interruptor returns SUCCESS,
        /// this node will be forced to return FAILURE and be terminated
        /// </summary>
        [Input(connectionType = ConnectionType.Override)]
        public Port interruptor;
        
        public BehaviourTree Tree => graph as BehaviourTree;
        protected DataContext GlobalContext => Tree.Data;
        public bool IsTerminated => Status == TaskStatus.Terminated;

        private BaseNode interruptorNode;
        private bool hasInterruptor;

        public TaskStatus Status {
            get;
            protected set;
        }

        public virtual BaseNode GetInput(string portName = "parent")
        {
            var port = GetInputPort(portName);
            BaseNode input = null;
            if (port != null && port.IsConnected) {
                input = port.Connection.node as BaseNode;
            }
            return input;
        }
        
        public virtual BaseNode GetNext(string portName = "output")
        {
            var port = GetOutputPort(portName);
            BaseNode next = null;
            if (port != null && port.IsConnected) {
                next = port.Connection.node as BaseNode;
            }
            return next;
        }

        protected BaseNode GetInterruptor() {
            return GetInput(nameof(interruptor));
        }

        public override object GetValue(NodePort port) {
            return Port.portValue;
        }

        public void Tick() {
            if (IsTerminated) {
                return; // terminated tasks do nothing
            }

            if (hasInterruptor) {
                var interruptorStatus = interruptorNode.InternalTick();
                if (interruptorStatus == TaskStatus.Success) {
                    Debug.Log($"Node {name} was interrupted by {interruptorNode.name}.");
                    // if we got an interruptor and it returned success, kill this node
                    Tree.RequestTickableRemoval(this);
                    interruptorNode.InternalTerminate();
                    Status = TaskStatus.Failure;
                    return;
                }
            }

            var newStatus = InternalTick();
            if (!ValidateInternalTickStatus(newStatus)) {
                throw new IllegalReturnStatusException(newStatus + " cannot be returned by BaseNode.InternalTick()");
            }

            if (newStatus != TaskStatus.Running) {
                Tree.RequestTickableRemoval(this);
                // Note: This was Largely unused by the "original" implementation of the BT. Basically an event you can subscribe to for each node.
                // So this would basically invoke an event and that's it.
                // BroadcastTaskStatusChange(newStatus);
            }
            Status = newStatus;
        }

        /// <summary>
        /// Called before the first Tick() (and InternalTick) is called.
        /// Use for initialisation
        /// </summary>
        public void Spawn() {
            Status = TaskStatus.Running;
            interruptorNode = GetInterruptor();
            hasInterruptor = interruptorNode != null;
            if (hasInterruptor) {
                // do not register this in the tickable loop.
                // This is a special case
                interruptorNode.Status = TaskStatus.Running;
                interruptorNode.InternalSpawn();
            }
            // Add to the insertion queue. The InternalSpawn process may remove it again
            // If the task does not need ticking.
            Tree.RequestTickableInsertion(this);
            // Do the specific spawning thing
            InternalSpawn();
        }

        /// <summary>
        /// Handle the nodes internal spawning logic.
        /// </summary>
        protected abstract void InternalSpawn();

        /// <summary>
        /// Called every tick on the currently active node of the behaviour tree
        /// </summary>
        protected abstract TaskStatus InternalTick();

        public void Terminate() {
            if (!IsTerminated) {
                Status = TaskStatus.Terminated;
                Tree.RequestTickableRemoval(this);
                InternalTerminate();
            }
        }

        protected abstract void InternalTerminate();
        
        private static bool ValidateInternalTickStatus(TaskStatus status) {
            return !(status == TaskStatus.Terminated || status == TaskStatus.Uninitialised);
        }
    }
}