using Broilerplate.Core;
using Broilerplate.Gameplay.Input;
using NaughtyAttributes;
using UnityEngine;

namespace Broilerplate.Gameplay {
    /// <summary>
    /// Represents a thing that can be possessed by a player controller.
    /// </summary>
    public class Pawn : Actor {

        private ControllerBase controller;
        private IInputHandler inputs;
        private MovementComponent movementComponent;

        public override void BeginPlay() {
            base.BeginPlay();
            movementComponent = GetGameComponent<MovementComponent>();
        }

        public virtual void OnControlTaken(ControllerBase inController) {
            controller = inController;
            if (inController is PlayerController p) {
                inputs = p.GetInputHandler();
                SetupInputs(GetInputHandler());
            }
        }

        public virtual void OnControlLeft() {
            controller = null;
        }

        public bool IsPlayerControlled() => controller is PlayerController;

        public virtual IInputHandler GetInputHandler() {
            return inputs;
        }

        public ControllerBase GetController() {
            return controller;
        }

        public MovementComponent GetMovementComponent() {
            return movementComponent;
        }

        protected virtual void SetupInputs(IInputHandler inputHandler) {
            // base function. override this to bind your inputs
        }

        protected virtual void ClearInputs() {
            GetInputHandler().ClearInputs();
        }
#if UNITY_EDITOR
        [Button("Control This")]
        private void ControlThis() {
            GetWorld().GetGameMode().GetMainPlayerController().ControlPawn(this);
        }
#endif
        public virtual void SetControlRotation(Quaternion controlRotation) {
            GetControlTransform().rotation = controlRotation;
        }

        /// <summary>
        /// Returns the Transform that is being affected by the control rotation.
        /// </summary>
        /// <returns></returns>
        public virtual Transform GetControlTransform() {
            return transform;
        }
    }
}