namespace Broilerplate.Tools.Bt {
    /// <summary>
    /// Runs all children as a sequence in parallel.
    /// Otherwise acts like a sequence.
    /// </summary>
    public class ParallelSequence : Node {
        public ParallelSequence(string name) : base(name) {
        }

        protected override TaskStatus Process() {
            int succeeded = 0;
            for (int i = 0; i < Children.Count; i++) {
                var child = Children[i];
                var childStatus = child.Status;
                if (childStatus is TaskStatus.Failure or TaskStatus.Terminated) {
                    return TaskStatus.Failure;
                }

                if (childStatus == TaskStatus.Success) {
                    succeeded++;
                }
            }

            if (succeeded == Children.Count) {
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }

        public override void Spawn() {
            base.Spawn();
            for (int i = 0; i < Children.Count; i++) {
                Children[i].Spawn();
            }
        }
    }
}