using System;
using System.Collections.Generic;
using Broilerplate.Ticking;
using UnityEngine;

namespace Broilerplate.Gameplay.Input {
    internal class TapPressHandler {
        public event ButtonPress Callbacks;
        private float timeButtonDown = float.PositiveInfinity;

        public void ExecuteTap() {
            ResetTiming();
            Callbacks?.Invoke();
        }

        public bool HasTapped() {
            var delay = Mathf.Abs(Time.time - timeButtonDown);

            if (delay > .175f && !float.IsInfinity(delay)) {
                ResetTiming();
                RegisterClick();
                return false;
            }

            return delay <= .15f;
        }

        public void RegisterClick() {
            timeButtonDown = Time.time;
        }

        public void ResetTiming() {
            timeButtonDown = float.PositiveInfinity;
        }
    }

    internal class DoublePressHandler {
        public event ButtonPress Callbacks;
        private float firstPress, secondPress;

        public void ExecuteDoublePress() {
            ResetTiming();
            Callbacks?.Invoke();
        }

        public bool HasDoublePress() {
            var delay = secondPress - firstPress;
            if (delay > .35f && !float.IsInfinity(delay)) {
                ResetTiming();
                RegisterClick();
                return false;
            }

            return delay <= .3f;
        }

        public void RegisterClick() {
            if (firstPress < 0) {
                firstPress = Time.time;
            }
            else {
                secondPress = Time.time;
            }
        }

        public void ResetTiming() {
            firstPress = -1;
            secondPress = float.PositiveInfinity;
        }
    }

    public class DefaultUnityInputHandler : IInputHandler {
        private readonly PlayerController playerController;

        private readonly Dictionary<string, ButtonPress> pressEvents = new();
        private readonly Dictionary<string, DoublePressHandler> doublePressEvents = new();
        private readonly Dictionary<string, TapPressHandler> tapEvents = new();
        private readonly Dictionary<string, ButtonPress> holdEvents = new();
        private readonly Dictionary<string, ButtonPress> releaseEvents = new();
        private readonly Dictionary<string, AxisInputData<float>> singleAxisEvents = new();
        private event Action<Vector2> PointerPositionUpdates;

        private readonly TickFunc tickFunc;

        public DefaultUnityInputHandler(PlayerController controller) {
            playerController = controller;
            tickFunc = new TickFunc();
            tickFunc.SetStartWithTickEnabled(true);
            tickFunc.SetTickTarget(this);
            tickFunc.SetTickGroup(TickGroup.Tick);
            controller.GetWorld().RegisterTickFunc(tickFunc);
        }

        public void ProcessTick(float deltaTime, TickGroup tickGroup) {
            // unity default input doesn't do events so we have to poll everything each frame manually
            // Update pointer position first, then run through the bindings.
            PointerPositionUpdates?.Invoke(UnityEngine.Input.mousePosition);

            foreach (var kvp in singleAxisEvents) {
                kvp.Value.UpdateInput(UnityEngine.Input.GetAxis(kvp.Key));
                kvp.Value.Invoke();
            }

            foreach (var kvp in pressEvents) {
                if (UnityEngine.Input.GetButtonDown(kvp.Key)) {
                    kvp.Value?.Invoke();
                }
            }

            foreach (var kvp in holdEvents) {
                if (UnityEngine.Input.GetButton(kvp.Key)) {
                    kvp.Value?.Invoke();
                }
            }

            foreach (var kvp in releaseEvents) {
                if (UnityEngine.Input.GetButtonUp(kvp.Key)) {
                    kvp.Value?.Invoke();
                }
            }

            foreach (var kvp in doublePressEvents) {
                if (UnityEngine.Input.GetButtonDown(kvp.Key)) {
                    kvp.Value.RegisterClick();
                    if (kvp.Value.HasDoublePress()) {
                        kvp.Value.ExecuteDoublePress();
                    }
                }
            }

            foreach (var kvp in tapEvents) {
                if (UnityEngine.Input.GetButtonUp(kvp.Key)) {
                    if (kvp.Value.HasTapped()) {
                        kvp.Value.ExecuteTap();
                    }
                }

                if (UnityEngine.Input.GetButtonDown(kvp.Key)) {
                    kvp.Value.RegisterClick();
                }
            }
        }

        public void SetEnableTick(bool shouldTick) {
            tickFunc.SetEnableTick(shouldTick);
        }

        public void UnregisterTickFunc() {
            playerController.GetWorld().UnregisterTickFunc(tickFunc);
        }

        public void OnEnableTick() {
        }

        public void OnDisableTick() {
        }

        public void ClearInputs() {
            pressEvents.Clear();
            doublePressEvents.Clear();
            tapEvents.Clear();
            holdEvents.Clear();
            releaseEvents.Clear();
            singleAxisEvents.Clear();

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
                    DoublePressHandler handler;
                    if (doublePressEvents.ContainsKey(action)) {
                        handler = doublePressEvents[action];
                    }
                    else {
                        handler = new DoublePressHandler();
                        handler.ResetTiming();
                        doublePressEvents[action] = handler;
                    }

                    handler.Callbacks += callback;
                    break;
                case ButtonActivatorType.Tap:
                    TapPressHandler tapHandler;
                    if (tapEvents.ContainsKey(action)) {
                        tapHandler = tapEvents[action];
                    }
                    else {
                        tapHandler = new TapPressHandler();
                        tapHandler.ResetTiming();
                        tapEvents[action] = tapHandler;
                    }

                    tapHandler.Callbacks += callback;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, $"{type} is not a supported activator type for default input module");
            }
        }

        public void BindAxis(string action, AxisInputData<float>.AxisInput callback) {
            if (!singleAxisEvents.ContainsKey(action)) {
                singleAxisEvents.Add(action, new AxisInputData<float>());
            }

            singleAxisEvents[action].InputCallbacks += callback;
        }

        public void BindAxis(string action, AxisInputData<Vector2>.AxisInput callback) {
            throw new NotImplementedException("Composite axis binding not supported with default Input module. Register for 2 separate axis instead");
        }

        public void BindPointerPositionUpdate(Action<Vector2> callback) {
            PointerPositionUpdates += callback;
        }
    }
}