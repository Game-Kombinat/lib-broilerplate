namespace Broilerplate.Core {
    /// <summary>
    /// Represents a Thing that lives in a World.
    /// Therefore offers the GetWorld() method.
    /// </summary>
    public interface IThing {

        public World GetWorld();
    }
}