namespace Broilerplate.Tools.Bt {
    /// <summary>
    /// When ticking nodes, if present, an interruptor will be checked.
    /// If it evaluates true, the nodes execution is interrupted and Failed is returned as status.
    /// </summary>
    public interface IInterruptor {
        public string Name { get; }
        // todo: this probably needs to be given some context to check if interruption is necessary.
        public bool TestInterrupt();
    }
}