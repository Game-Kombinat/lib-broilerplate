namespace Broilerplate.Tools.Bt {
    /// <summary>
    /// Repeats until interrupted.
    /// </summary>
    public class Repeater : SingleChildNode {
        public Repeater(string name) : base(name) {
        }

        protected override TaskStatus Process() {
            if (ActiveChild.Status != TaskStatus.Running) {
                ActiveChild.Despawn();
                ActiveChild.Spawn();
            }
            return ActiveChild.Status;
        }

        public override void Spawn() {
            if (!HasInterruptor) {
                throw new BehaviourTreeException("Repeater is required to run with interruptor to prevent infinite loops!");
            }
            base.Spawn();
            ActiveChild.Spawn();
        }
    }
}