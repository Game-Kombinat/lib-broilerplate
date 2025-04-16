using System;

namespace Broilerplate.Tools.Bt {
    /// <summary>
    /// When ticking nodes, if present, an interruptor will be checked.
    /// If it evaluates true, the nodes execution is interrupted and Failed is returned as status.
    /// </summary>
    public class Interruptor {
        public string Name { get; }

        private Func<Node, bool> interruptCondition;

        private Node observedNode;

        public Interruptor(string name, Func<Node, bool> condition) {
            Name = name;
            interruptCondition = condition;
        }

        public void SetNode(Node node) {
            observedNode = node;
        }
        
        public bool TestInterrupt() {
            return interruptCondition(observedNode);
        }
    }
}