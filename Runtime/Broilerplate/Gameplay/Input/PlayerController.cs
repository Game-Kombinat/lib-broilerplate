﻿using Broilerplate.Core;
using Broilerplate.Gameplay.View;
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
        [SerializeField]
        private CameraManager cameraManagerType;

        private CameraManager cameraManagerInstance;
        
        [SerializeField]
        private PlayerInput playerInput;

        private InputHandler inputHandler;
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
        }

        public override void ControlPawn(Pawn pawn) {
            LeaveControlledPawn();
            if (!playerInput) {
                // Since it's not a broilerplate type we get it via vanilla unity api
                playerInput = gameObject.GetComponent<PlayerInput>();
            }
            inputHandler = new InputHandler(playerInput);

            controlledPawn = pawn;
            if (cameraManagerInstance.AutoHandleViewTargets) {
                cameraManagerInstance.SetViewTarget(pawn);
            }
            
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