namespace Broilerplate.Tools.Bt {
    /// <summary>
    /// Runs child nodes until the first returns success.
    /// </summary>
    public class Selector : Node {
        public Selector(string name) : base(name) {
        }

        protected override TaskStatus Process() {
            TaskStatus childStatus = ActiveChild.Status;

            // Check for termination states first
            if (childStatus == TaskStatus.Running) {
                return TaskStatus.Running;
            }
            if (childStatus == TaskStatus.Success) {
                return TaskStatus.Success;
            }
            
            // Check if we reached the selectors end
            if (currentChild == Children.Count - 1) {
                // No child returned success. This was a fail.
                return TaskStatus.Failure;
            }
            
            // Still here? Lets keep on going with the next child
            currentChild++;
            ActiveChild.Spawn();
            return TaskStatus.Running;
        }

        public override void Spawn() {
            base.Spawn();
            ActiveChild.Spawn();
        }
    }
}