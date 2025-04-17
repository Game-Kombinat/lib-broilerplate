using System;

namespace Broilerplate.Tools.Bt {
    /// <summary>
    /// Select either or a node based on a configured condition.
    /// </summary>
    public class ConditionalSelector : Node {
        private Func<bool> condition;

        private Node trueNode;
        private Node falseNode;

        private Node selectedNode;
        public ConditionalSelector(string name) : base(name) {
            
        }

        public ConditionalSelector WithCondition(Func<bool> condi) {
            condition = condi;
            return this;
        }

        public ConditionalSelector WithTrueNode(Node node) {
            SetParentOn(node);
            trueNode = node;
            return this;
        }

        public ConditionalSelector WithFalseNode(Node node) {
            SetParentOn(node);
            falseNode = node;

            return this;
        }

        protected override TaskStatus Process() {
            if (selectedNode != null) {
                if (selectedNode.Status != TaskStatus.Running) {
                    return TaskStatus.Success;
                }
                return TaskStatus.Running;
            }
            if (condition()) {
                trueNode.Spawn();
                selectedNode = trueNode;
            }
            else {
                falseNode.Spawn();
                selectedNode = falseNode;
            }
            
            return selectedNode.Status;
        }

        public override void Reset() {
            base.Reset();
            selectedNode = null;
        }
    }
}