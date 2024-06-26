﻿using System;
using Broilerplate.Ticking;
using UnityEngine;

namespace Broilerplate.Gameplay.Input {
    public interface IInputHandler : ITickable {

        void Setup(PlayerController pc);
        /// <summary>
        /// Clears the list of bound button and value mappings.
        /// </summary>
        void ClearInputs();

        void EnableInputs();

        void DisableInputs();

        void SetInputMap(string mapId);

        void BindAction(string action, ButtonActivatorType type, ButtonPress callback);
        void BindAxis(string action, AxisInputData<float>.AxisInput callback);
        void BindAxis(string action, AxisInputData<Vector2>.AxisInput callback);

        void BindPointerPositionUpdate(Action<Vector2> callback);
    }
}