using Broilerplate.Core;

namespace Broilerplate.Gameplay.Input {
    public abstract class ControllerBase : Actor {
        protected Pawn possessedPawn;

        public abstract void Possess(Pawn pawn);

        public abstract void EjectPawn();
    }
}