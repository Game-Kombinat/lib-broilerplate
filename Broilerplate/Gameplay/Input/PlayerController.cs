using UnityEngine;
using UnityEngine.InputSystem;

namespace Broilerplate.Gameplay.Input {
    /// <summary>
    /// This is the entry point for sampling user input
    /// and piping it through to a pawn which will then pipe it through
    /// to wherever it needs to go.
    /// Consequently this will possess a pawn and take control of it.
    /// One possessable pawn per player controller.
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : ControllerBase {
        // todo: get the camera manager in here.
        // probably going to go with Cinemachine for camera handling.
        [SerializeField]
        private PlayerInput playerInput;

        private InputHandler inputHandler;

        public override void Possess(Pawn pawn) {
            EjectPawn();
            
            inputHandler.ClearInputs();
            inputHandler = new InputHandler(playerInput);

            possessedPawn = pawn;
            pawn.OnPossess(this);

        }

        public override void EjectPawn() {
            if (possessedPawn) {
                possessedPawn.OnEjectPawn();
            }
            inputHandler.ClearInputs();
        }

        public InputHandler GetInputHandler() {
            return inputHandler;
        }
    }
}