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
        private InputHandler inputs;
        private MovementComponent movementComponent;

        public override void BeginPlay() {
            base.BeginPlay();
            movementComponent = GetGameComponent<MovementComponent>();
            if (!movementComponent) {
                throw new UnassignedReferenceException("Pawn requires movement input component. None found");
            }
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

        public virtual InputHandler GetInputHandler() {
            return inputs;
        }

        public ControllerBase GetController() {
            return controller;
        }

        public MovementComponent GetMovementComponent() {
            return movementComponent;
        }

        protected virtual void SetupInputs(InputHandler inputHandler) {
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
            transform.rotation = controlRotation;
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