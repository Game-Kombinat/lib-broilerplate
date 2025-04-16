namespace Broilerplate.Tools.Bt {
    public abstract class SingleChildNode : Node {
        protected SingleChildNode(string name) : base(name) {
        }

        public override Node AddChild(Node node) {
            if (Children.Count >= 1) {
                throw new BehaviourTreeException("Single Child node supports only single child!");
            } 
            
            return base.AddChild(node);
        }
    }
}