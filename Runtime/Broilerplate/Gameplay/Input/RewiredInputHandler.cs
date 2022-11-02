#if ENABLE_REWIRED
using System;
using System.Collections.Generic;
using System.Linq;
using Broilerplate.Ticking;
using Rewired;
using UnityEngine;

namespace Broilerplate.Gameplay.Input {
    /*
        // This event will be called every frame any input is updated
        player.AddInputEventDelegate(OnInputUpdate, UpdateLoopType.Update);

        // This event will be called every frame the "Fire" action is updated
        player.AddInputEventDelegate(OnFireUpdate, UpdateLoopType.Update, "Fire");

        // This event will be called when the "Fire" button is first pressed
        player.AddInputEventDelegate(OnFireButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Fire");

        // This event will be called when the "Fire" button is first released
        player.AddInputEventDelegate(OnFireButtonUp, UpdateLoopType.Update, InputActionEventType.ButtonJustReleased, "Fire");

        // This event will be called every frame the "Move Horizontal" axis is non-zero and once more when it returns to zero.
        player.AddInputEventDelegate(OnMoveHorizontal, UpdateLoopType.Update, InputActionEventType.AxisActiveOrJustInactive, "Move Horizontal");
     */
    public class RewiredInputHandler : IInputHandler {

        private Player reInput;
        protected PlayerController playerController;
        
        private readonly Dictionary<string, ButtonPress> pressEvents = new Dictionary<string, ButtonPress>();
        private readonly Dictionary<string, ButtonPress> doublePressEvents = new Dictionary<string, ButtonPress>();
        private readonly Dictionary<string, ButtonPress> holdEvents = new Dictionary<string, ButtonPress>();
        private readonly Dictionary<string, ButtonPress> releaseEvents = new Dictionary<string, ButtonPress>();
        private readonly Dictionary<string, AxisInputData<float>> singleAxisEvents = new Dictionary<string, AxisInputData<float>>();
        private event Action<Vector2> PointerPositionUpdates;
        public RewiredInputHandler(PlayerController pc) {
            playerController = pc;
            reInput = ReInput.players.GetPlayer("System");
            
            // note: because rewired works more predictable than unitys stuff, we don't need any ticking in here but 
            // the IInputHandler makes us implement all the tickable stuff.
            reInput.AddInputEventDelegate(OnReceiveAxisInput, UpdateLoopType.Update, InputActionEventType.AxisActive);
            reInput.AddInputEventDelegate(OnReceiveAxisInput, UpdateLoopType.Update, InputActionEventType.AxisInactive);
            
            reInput.AddInputEventDelegate(OnReceiveButtonInput, UpdateLoopType.Update, InputActionEventType.ButtonPressed);
            reInput.AddInputEventDelegate(OnReceiveButtonInput, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed);
            reInput.AddInputEventDelegate(OnReceiveButtonInput, UpdateLoopType.Update, InputActionEventType.ButtonJustReleased);
            reInput.AddInputEventDelegate(OnReceiveAnyUpdate, UpdateLoopType.Update, InputActionEventType.Update);
            
            tf = new TickFunc();
            tf.SetTickTarget(this);
            tf.SetCanEverTick(true);
            tf.SetTickGroup(TickGroup.Tick);
            tf.SetStartWithTickEnabled(true);
            
            pc.GetWorld().RegisterTickFunc(tf);
            
        }

        #region Tick

        private readonly TickFunc tf;
        public void ProcessTick(float deltaTime) {
            // note: due to a bug (?) in Rewired, we can only poll for double presses. so we do that
            foreach (var kvp in doublePressEvents) {
                if (reInput.GetButtonDoublePressDown(kvp.Key)) {
                    kvp.Value.Invoke();
                }
            }
        }

        public void SetEnableTick(bool shouldTick) {
            
        }

        public void UnregisterTickFunc() {
            playerController.GetWorld().UnregisterTickFunc(tf);
        }

        public void OnEnableTick() {
            tf.SetEnableTick(true);
        }

        public void OnDisableTick() {
            tf.SetEnableTick(false);
        }
        #endregion

        public void ClearInputs() {
            reInput.ClearInputEventDelegates();
            PointerPositionUpdates = null;
            UnregisterTickFunc();
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
                case ButtonActivatorType.Holding:
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
                case ButtonActivatorType.DoublePress:
                    if (doublePressEvents.ContainsKey(action)) {
                        doublePressEvents[action] += callback;
                    }
                    else {
                        doublePressEvents[action] = callback;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, $"{type} is not a supported activator type for Rewired input module");
            }
        }

        public void BindAxis(string action, AxisInputData<float>.AxisInput callback) {
            if (!singleAxisEvents.ContainsKey(action)) {
                singleAxisEvents.Add(action, new AxisInputData<float>());
            }
            singleAxisEvents[action].InputCallbacks += callback;
        }

        public void BindAxis(string action, AxisInputData<Vector2>.AxisInput callback) {
            throw new NotImplementedException("Rewired backend does not provide composite axis binding. Register for 2 separate axis instead");
        }

        public void BindPointerPositionUpdate(Action<Vector2> callback) {
            PointerPositionUpdates += callback;
        }

        private void OnReceiveAxisInput(InputActionEventData data) {
            HandleAxisInput(data.actionName, data.GetAxis());
        }
        
        private void OnReceiveButtonInput(InputActionEventData data) {
            HandleButtonInput(data.actionName, data);
        }
        
        private void OnReceiveAnyUpdate(InputActionEventData data) {
            if (reInput.controllers.hasMouse) {
                PointerPositionUpdates?.Invoke(reInput.controllers.Mouse.screenPosition);
            }
        }

        private void HandleButtonInput(string actionName, InputActionEventData data) {
            switch (data.eventType) {
                case InputActionEventType.ButtonPressed: // hold
                    HandleButtonCallbacks(actionName, holdEvents);
                    break;
                case InputActionEventType.ButtonJustReleased: // release
                    HandleButtonCallbacks(actionName, releaseEvents);
                    break;
                case InputActionEventType.ButtonJustPressed: // down
                    HandleButtonCallbacks(actionName, pressEvents);
                    break;
                // note: due to a bug (?) double presses need to be polled in ProcessTick
                // case InputActionEventType.ButtonJustDoublePressed:
                //     HandleButtonCallbacks(actionName, doublePressEvents);
                //     break;
            }
        }

        private void HandleAxisInput(string binding, float axisValue) {
            if (!singleAxisEvents.ContainsKey(binding)) {
                return;
            }

            var control = singleAxisEvents[binding];
            control.UpdateInput(axisValue);
            control.Invoke();
        }
        
        private void HandleButtonCallbacks(string bindingName, Dictionary<string,ButtonPress> bindings) {
            if (!bindings.ContainsKey(bindingName)) {
                return;
            }
            bindings[bindingName]?.Invoke();
        }
    }
}
#endif