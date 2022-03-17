using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Broilerplate.Gameplay.Input {
    public enum ButtonActivatorType {
        Press,
        Hold,
        Release
    }

    public delegate void ButtonPress();
    public delegate void SingleAxisInput(float value);
    public delegate void DoubleAxisInput(Vector2 value);
    
    public class InputHandler {

        protected PlayerInput inputs;

        private Dictionary<string, ButtonPress> pressEvents;
        private Dictionary<string, ButtonPress> holdEvents;
        private Dictionary<string, ButtonPress> releaseEvents;
        private Dictionary<string, SingleAxisInput> singleAxisEvents;
        private Dictionary<string, DoubleAxisInput> doubleAxisEvents;
        
        public InputHandler(PlayerInput playerInput) {
            inputs = playerInput;
            inputs.onActionTriggered += InputActionReceived;
        }

        /// <summary>
        /// Clears the list of bound button and value mappings.
        /// </summary>
        public void ClearInputs() {
            inputs.onActionTriggered -= InputActionReceived;
            pressEvents.Clear();
            holdEvents.Clear();
            releaseEvents.Clear();
            singleAxisEvents.Clear();
            doubleAxisEvents.Clear();
        }
        
        public void BindAction(ButtonActivatorType type, string action, ButtonPress callback) {
            switch (type) {
                case ButtonActivatorType.Press:
                    pressEvents[action] += callback;
                    break;
                case ButtonActivatorType.Hold:
                    holdEvents[action] += callback;
                    break;
                case ButtonActivatorType.Release:
                    releaseEvents[action] += callback;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public void BindAxis(string action, SingleAxisInput callback) {
            singleAxisEvents[action] = callback;
        }
        
        public void BindAxis(string action, DoubleAxisInput callback) {
            doubleAxisEvents[action] = callback;
        }

        private void InputActionReceived(InputAction.CallbackContext ctx) {
            if (ctx.action.type == InputActionType.Button) {
                switch (ctx.phase) {
                    case InputActionPhase.Started:
                        HandleButtonCallbacks(ctx.action.name, pressEvents);
                        break;
                    case InputActionPhase.Performed:
                        HandleButtonCallbacks(ctx.action.name, holdEvents);
                        break;
                    case InputActionPhase.Canceled:
                        HandleButtonCallbacks(ctx.action.name, releaseEvents);
                        break;
                    case InputActionPhase.Disabled:
                    case InputActionPhase.Waiting:
                    default:
                        // this is not illegal, we just don't go handling it
                        break;
                }
            }
            else {
                // we treat the other options as axis for now
                
                // bit icky. need another data structure here to easily support all the things
                // that the input system has on offer in terms of composite axis inputs.
                if (ctx.valueType == typeof(float)) {
                    if (!singleAxisEvents.ContainsKey(ctx.action.name)) {
                        return;
                    }
                    singleAxisEvents[ctx.action.name]?.Invoke(ctx.ReadValue<float>());
                }
                else if (ctx.valueType == typeof(Vector2)) {
                    if (!doubleAxisEvents.ContainsKey(ctx.action.name)) {
                        return;
                    }
                    doubleAxisEvents[ctx.action.name]?.Invoke(ctx.ReadValue<Vector2>());
                }
            }
        }

        private void HandleButtonCallbacks(string bindingName, Dictionary<string, ButtonPress> callbacks) {
            if (!callbacks.ContainsKey(bindingName)) {
                return;
            }
            callbacks[bindingName]?.Invoke();
            
        }
    }
}