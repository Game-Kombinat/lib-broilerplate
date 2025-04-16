namespace Broilerplate.Tools.Bt {
    /// <summary>
    /// Inverts the Success and Failure status to the opposite.
    /// </summary>
    public class Inverter : SingleChildNode {
        
        public Inverter(string name) : base(name) {
            
        }
        protected override TaskStatus Process() {
            switch (ActiveChild.Status) {
                case TaskStatus.Success:
                    return TaskStatus.Failure;
                case TaskStatus.Failure:
                    return TaskStatus.Success;
                default:
                    return TaskStatus.Running;
            }
        }
    }
}