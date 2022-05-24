using System;
using System.Collections.Generic;
using Broilerplate.Ticking;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Broilerplate.Gameplay.Input {
    public enum ButtonActivatorType {
        Press,
        Hold,
        Release
    }

    public class AxisInputData<T> {
        public delegate void AxisInput(T value);
        public event AxisInput Inputs;
        public T lastInput;
        
        public void Invoke() {
            Inputs?.Invoke(lastInput);
        }

        public void UpdateInput(T newInput) {
            lastInput = newInput;
        }
    }

    public delegate void ButtonPress();
    
    public class InputHandler : ITickable {

        protected PlayerInput inputs;
        protected PlayerController playerController;

        private readonly Dictionary<string, ButtonPress> pressEvents = new();
        private readonly Dictionary<string, ButtonPress> holdEvents = new();
        private readonly Dictionary<string, ButtonPress> releaseEvents = new();
        private readonly Dictionary<string, AxisInputData<float>> singleAxisEvents = new();
        private readonly Dictionary<string, AxisInputData<Vector2>> doubleAxisEvents = new();
        
        private readonly TickFunc inputTick;
        
        public InputHandler(PlayerInput playerInput, PlayerController controller) {
            inputs = playerInput;
            inputs.onActionTriggered += InputActionReceived;
            playerController = controller;
            
            inputTick = new TickFunc();
            inputTick.SetTickGroup(TickGroup.LateTick);
            inputTick.SetTickTarget(this);
            inputTick.SetEnableTick(false);
            controller.GetWorld().RegisterTickFunc(inputTick);
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
            
            playerController.GetWorld().UnregisterTickFunc(inputTick);
        }
        
        public void BindAction(string action, ButtonActivatorType type, ButtonPress callback) {
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

        public void BindAxis(string action, AxisInputData<float>.AxisInput callback) {
            if (!singleAxisEvents.ContainsKey(action)) {
                singleAxisEvents.Add(action, new AxisInputData<float>());
            }
            singleAxisEvents[action].Inputs += callback;
            
        }
        
        public void BindAxis(string action, AxisInputData<Vector2>.AxisInput callback) {
            if (!doubleAxisEvents.ContainsKey(action)) {
                doubleAxisEvents.Add(action, new AxisInputData<Vector2>());
            }
            doubleAxisEvents[action].Inputs += callback;
        }

        public bool ProcessAxisInput() {
            // just calls the registered callbacks with the currently known axis values.
            // Because Unitys input system doesn't actually do repeated invocations
            // on axis that have non-0 values (because that is obviously way too difficult and nobody ever could possibly want that)
            // we have to repeat the axis inputs manually for all inputs until they are all 0.
            bool repeatNextFrame = false;
            foreach (var kvp in singleAxisEvents) {
                kvp.Value.Invoke();
                if (kvp.Value.lastInput != 0) {
                    repeatNextFrame = true;
                }
            }
            
            foreach (var kvp in doubleAxisEvents) {
                kvp.Value.Invoke();
                if (kvp.Value.lastInput != Vector2.zero) {
                    repeatNextFrame = true;
                }
            }

            return repeatNextFrame;
        }
        
        public void SetEnableTick(bool shouldTick) {
            // we ignore the "can ever tick" flag here because
            // we know that we must tick this
            inputTick.SetEnableTick(shouldTick);
        }

        private void InputActionReceived(InputAction.CallbackContext ctx) {
            // Debug.Log(ctx.action + " " + ctx.phase);
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

                    singleAxisEvents[ctx.action.name].UpdateInput(ctx.ReadValue<float>());
                    SetEnableTick(true);
                }
                else if (ctx.valueType == typeof(Vector2)) {
                    if (!doubleAxisEvents.ContainsKey(ctx.action.name)) {
                        return;
                    }
                    
                    doubleAxisEvents[ctx.action.name].UpdateInput(ctx.ReadValue<Vector2>());
                    SetEnableTick(true);
                }
            }
        }

        private void HandleButtonCallbacks(string bindingName, Dictionary<string, ButtonPress> callbacks) {
            if (!callbacks.ContainsKey(bindingName)) {
                return;
            }
            callbacks[bindingName]?.Invoke();
        }

        public void ProcessTick(float deltaTime) {
            if (!ProcessAxisInput()) {
                SetEnableTick(false);
            }
        }
    }
}