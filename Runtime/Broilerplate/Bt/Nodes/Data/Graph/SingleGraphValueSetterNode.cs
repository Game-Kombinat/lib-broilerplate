namespace Broilerplate.Bt.Nodes.Data.Graph {
    /// <summary>
    /// These are action nodes because they actively do something.
    /// And that actively doing needs to be triggered somehow.
    /// And since we work within a behaviour tree, we will do it within the flow of it.
    ///
    /// This exists basically only to get the correct editor for the node display
    /// </summary>
    public abstract class SingleGraphValueSetterNode : ContextDataAccessNode {
        
        protected override void InternalSpawn() {
        }
        
        protected override void InternalTerminate() {
        }
    }
}