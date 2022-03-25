﻿using Broilerplate.Core;
using Broilerplate.Gameplay.Input;
using NaughtyAttributes;

namespace Broilerplate.Gameplay {
    /// <summary>
    /// Represents a thing that can be possessed by a player controller.
    /// </summary>
    public class Pawn : Actor {

        private ControllerBase controller;
        private InputHandler inputs;
        public virtual void OnControlTaken(ControllerBase inController) {
            controller = inController;
            if (inController is PlayerController p) {
                inputs = p.GetInputHandler();
                SetupInputs(GetInputHandler());
            }
        }
        
        public virtual void OnControlLeft() {}

        public virtual InputHandler GetInputHandler() {
            return inputs;
        }

        public ControllerBase GetController() {
            return controller;
        }

        protected virtual void SetupInputs(InputHandler inputHandler) {
            
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
    }
}