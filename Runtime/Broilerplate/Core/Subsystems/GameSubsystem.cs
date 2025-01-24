namespace Broilerplate.Core.Subsystems {
    /// <summary>
    /// Subsystem that is initialised with the GameInstance and is destroyed with the GameInstance.
    ///
    /// In other words: This is live for the entire lifecycle of the game.
    /// </summary>
    public class GameSubsystem : SubsystemBase, IThing {
        protected World loadedWorld;
        
        public virtual void OnGameEnds() {
            
        }

        public virtual void OnWorldSpawned(World world) {
            loadedWorld = world;
        }

        public virtual void OnWorldDespawning(World world, string nextLevel) {
            if (loadedWorld == world) {
                loadedWorld = null;
            }
        }
        
        public World GetWorld() {
            return loadedWorld;
        }

        public int GetRuntimeId() {
            return GetInstanceID();
        }

        public static T GetSubsystem<T>() where T : GameSubsystem {
            return GameInstance.GetInstance().GetSubsystem<T>();
        }
    }
}