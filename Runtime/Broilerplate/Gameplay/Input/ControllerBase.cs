using Broilerplate.Core;

namespace Broilerplate.Gameplay.Input {
    public abstract class ControllerBase : Actor {
        protected Pawn controlledPawn;
        
        public Pawn ControlledPawn => controlledPawn;

        public abstract void ControlPawn(Pawn pawn);

        public abstract void LeaveControlledPawn();
    }
}