namespace Broilerplate.Tools.Bt {
    /// <summary>
    /// Runs all child nodes until the end or a failure was received.
    /// </summary>
    public class Sequence : Node {
        public Sequence(string name) : base(name) {
        }

        protected override TaskStatus Process() {
            TaskStatus childStatus = ActiveChild.Status;

            if (childStatus == TaskStatus.Running) {
                return TaskStatus.Running;
            }
            if (childStatus is TaskStatus.Failure or TaskStatus.Terminated) {
                return TaskStatus.Failure;
            }
            if (currentChild == Children.Count - 1) {
                return TaskStatus.Success;
            }
            
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