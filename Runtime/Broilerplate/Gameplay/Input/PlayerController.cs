using Broilerplate.Core;
using Broilerplate.Gameplay.View;
using Broilerplate.Ticking;
using UnityEngine;

namespace Broilerplate.Gameplay.Input {
    /// <summary>
    /// This is the entry point for sampling user input
    /// and piping it through to a pawn which will then pipe it through
    /// to wherever it needs to go.
    /// Consequently this will possess a pawn and take control of it.
    /// One possessable pawn per player controller.
    /// </summary>
    public class PlayerController : ControllerBase {
        
        [Header("Control Behaviour")]
        [SerializeField]
        private bool startWithMouseCursor;
        
        [Header("References")]
        [SerializeField]
        private CameraManager cameraManagerType;
        
        [SerializeReference, SubclassSelector]
        private IInputHandler inputHandler;
        
        private CameraManager cameraManagerInstance;

        
        private PlayerInfo playerInfo;

        public PlayerInfo PlayerInfo => playerInfo;

        public CameraManager CameraManager => cameraManagerInstance;

        public override void BeginPlay() {
            base.BeginPlay();
            if (cameraManagerType) {
                cameraManagerInstance = GetWorld().SpawnActor(cameraManagerType, Vector3.zero, Quaternion.identity);
            }
            else {
                var go = new GameObject("Default Camera Manager");
                cameraManagerInstance = GetWorld().SpawnActorOn<CameraManager>(go);
            }

            if (startWithMouseCursor) {
                ShowMouseCursor();
            }
            else {
                HideMouseCursor();
            }
        }

        protected override void Reset() {
            base.Reset();
            actorTick.SetTickGroup(TickGroup.LateTick);
        }

        public void ShowMouseCursor() {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        
        public void HideMouseCursor() {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        public override void ControlPawn(Pawn pawn) {
            LeaveControlledPawn();

            controlledPawn = pawn;
            if (cameraManagerInstance.AutoViewTargeting) {
                cameraManagerInstance.SetViewTarget(pawn);
            }
            inputHandler.Setup(this);
            pawn.OnControlTaken(this);
        }

        public override void LeaveControlledPawn() {
            if (controlledPawn) {
                controlledPawn.OnControlLeft();
                inputHandler?.ClearInputs();
            }
        }

        public IInputHandler GetInputHandler() {
            return inputHandler;
        }

        public void SetPlayerInfo(PlayerInfo inPlayerInfo) {
            playerInfo = inPlayerInfo;
        }

        
        public void AddRotationInput(float x, float y, float z) {
            ControlledPawn.GetMovementComponent()?.AddRotationInput(x, y, z);
        }

        public void AddMovementInput(float x, float y, float z) {
            ControlledPawn.GetMovementComponent()?.AddMovementInput(x, y, z);
        }

        public CameraManager GetCameraManager() {
            return cameraManagerInstance;
        }
    }
}