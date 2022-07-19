using System;
using GameKombinat.Fnbt;
using XNode;

namespace Broilerplate.Bt.Nodes.Data.Graph {
    /// <summary>
    /// Value nodes do nothing but provide access to values
    /// and additional calculations within the ecosystem of the behaviour tree.
    /// They are, in themselves, not behaviours. Just complementary
    /// </summary>
    public abstract class SingleGraphValueGetterNode : ContextDataAccessNode {
        [NonSerialized]
        private NbtTag valueTag;

        public override object GetValue(NodePort port) {
            // That's a thing in Editor when hovering over node ports
            if (valueTag == null) {
                RefreshValueTag();
            }
            return port.fieldName == GetValuePortName() ? valueTag["value"] : null;
        }

        /// <summary>
        /// Mostly used in editor only.
        /// Called when, on the node, the value changed
        /// so that node ports will display the correct values.
        /// Otherwise, this would have reason for existence because
        /// at the beginning of each Behaviour Tree execution, the nodes are initialised.
        /// At that points value tags are fetched and during normal operation (not editing stuff)
        /// they will remain the same. Only the boxed values change but ofc that is accounted for.
        /// </summary>
        public void RefreshValueTag() {
            valueTag = GetDataContextFromScope().Find(varName);
        }
        protected override void Init() {
            RefreshValueTag();
        }

        protected abstract string GetValuePortName();
        
        // This node will not be used as BT action.
        // We have to implement this though because the value setter nodes need it
        // and in order to avoid much code duplication, I chose to go this route.
        protected override void InternalSpawn() {
            throw new InvalidOperationException("Value Getter Nodes are not supposed to be used as BT Tasks!");
        }
        
        protected override TaskStatus InternalTick() {
            throw new InvalidOperationException("Value Getter Nodes are not supposed to be used as BT Tasks!");
        }
        
        protected override void InternalTerminate() {
            throw new InvalidOperationException("Value Getter Nodes are not supposed to be used as BT Tasks!");
        }
    }
}