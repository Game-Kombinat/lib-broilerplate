using Broilerplate.Core;
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
        private PlayerInfo playerInfo;

        public PlayerInfo PlayerInfo => playerInfo;

        public override void TakeControl(Pawn pawn) {
            LeaveControlledPawn();
            if (!playerInput) {
                // Since it's not a broilerplate type we get it via vanilla unity api
                playerInput = gameObject.GetComponent<PlayerInput>();
            }
            inputHandler = new InputHandler(playerInput);

            controlledPawn = pawn;
            pawn.OnControlTaken(this);

        }

        public override void LeaveControlledPawn() {
            if (controlledPawn) {
                controlledPawn.OnControlLeft();
            }

            inputHandler?.ClearInputs();
        }

        public InputHandler GetInputHandler() {
            return inputHandler;
        }

        public void SetPlayerInfo(PlayerInfo inPlayerInfo) {
            playerInfo = inPlayerInfo;
        }
    }
}