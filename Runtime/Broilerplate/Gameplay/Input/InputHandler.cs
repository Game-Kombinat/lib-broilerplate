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

        private readonly Dictionary<string, ButtonPress> pressEvents = new();
        private readonly Dictionary<string, ButtonPress> holdEvents = new();
        private readonly Dictionary<string, ButtonPress> releaseEvents = new();
        private readonly Dictionary<string, SingleAxisInput> singleAxisEvents = new();
        private readonly Dictionary<string, DoubleAxisInput> doubleAxisEvents = new();
        
        public InputHandler(PlayerInput playerInput) {
            inputs = playerInput;
            inputs.onActionTriggered += InputActionReceived;
            inputs.currentActionMap.actionTriggered += InputActionReceived;
        }

        /// <summary>
        /// Clears the list of bound button and value mappings.
        /// </summary>
        public void ClearInputs() {
            inputs.onActionTriggered -= InputActionReceived;
            inputs.currentActionMap.actionTriggered -= InputActionReceived;
            
            pressEvents.Clear();
            holdEvents.Clear();
            releaseEvents.Clear();
            singleAxisEvents.Clear();
            doubleAxisEvents.Clear();
        }
        
        public void BindAction(ButtonActivatorType type, string action, ButtonPress callback) {
            switch (type) {
                case ButtonActivatorType.Press:
                    if (pressEvents.ContainsKey(action)) {
                        pressEvents[action] += callback;
                    }
                    else {
                        pressEvents[action] = callback;
                    }
                    
                    break;
                case ButtonActivatorType.Hold:
                    if (holdEvents.ContainsKey(action)) {
                        holdEvents[action] += callback;
                    }
                    else {
                        holdEvents[action] = callback;
                    }
                    break;
                case ButtonActivatorType.Release:
                    if (releaseEvents.ContainsKey(action)) {
                        releaseEvents[action] += callback;
                    }
                    else {
                        releaseEvents[action] = callback;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public void BindAxis(string action, SingleAxisInput callback) {
            // inputs.currentActionMap[action].performed += ;
            if (singleAxisEvents.ContainsKey(action)) {
                singleAxisEvents[action] += callback;
            }
            else {
                singleAxisEvents[action] = callback;
            }
            
        }
        
        public void BindAxis(string action, DoubleAxisInput callback) {
            if (doubleAxisEvents.ContainsKey(action)) {
                doubleAxisEvents[action] += callback;
            }
            else {
                doubleAxisEvents[action] = callback;
            }
        }

        private void InputActionReceived(InputAction.CallbackContext ctx) {
            Debug.Log(ctx.action + " " + ctx.phase);
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