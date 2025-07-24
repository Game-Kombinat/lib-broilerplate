namespace Broilerplate.Tools.Bt {
    /// <summary>
    /// Runs all children as a selector in parallel.
    /// Otherwise acts like a selector.
    /// </summary>
    public class ParallelSelector : Node {
        public ParallelSelector(string name) : base(name) {
        }
        
        protected override TaskStatus Process() {
            int failed = 0;
            for (int i = 0; i < Children.Count; i++) {
                var child = Children[i];
                var childStatus = child.Status;
                if (childStatus is TaskStatus.Success) {
                    return TaskStatus.Success;
                }
                if (childStatus is TaskStatus.Failure or TaskStatus.Terminated) {
                    failed++;
                }

            }

            if (failed == Children.Count) {
                return TaskStatus.Failure;
            }
            // none was selected
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