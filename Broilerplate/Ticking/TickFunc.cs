using System;
using Broilerplate.Core;
using UnityEngine;

namespace Broilerplate.Ticking {
    /// <summary>
    /// Every actor can be ticked.
    /// This TickFunc class calls tick functions on tickable types
    /// according to its configuration.
    /// </summary>
    [Serializable]
    public class TickFunc {
        /// <summary>
        /// If false, this tick function will never tick at all.
        /// It will not be registered with the Tick Loop
        /// </summary>
        [SerializeField]
        private bool canEverTick = false;

        /// <summary>
        /// Determines if we start with ticking enabled by default.
        /// </summary>
        [SerializeField]
        private bool startTickEnabled = false;

        /// <summary>
        /// Determines if we're currently ticking.
        /// </summary>
        [SerializeField]
        private bool tickEnabled = false;

        /// <summary>
        /// When this tick is executed.
        /// </summary>
        [SerializeField]
        private TickGroup tickGroup;

        /// <summary>
        /// Interval in seconds in which the tick is executed.
        /// A value of less or equal to 0 means tick at framerate.
        /// </summary>
        [SerializeField]
        private float tickInterval = 0;

        /// <summary>
        /// Use this to determine the order in which this tick is ticking compared to others
        /// in the same tick group. Higher numbers are executed later. Smaller numbers are executed earlier.
        /// </summary>
        [SerializeField]
        private int priority = 0;

        /// <summary>
        /// The thing on which we will execute the tick function
        /// according to these here configurations.
        /// </summary>
        private ITickable tickTarget = null;

        private float lastTick;

        /// <summary>
        /// If false, this tick function will never tick at all.
        /// It will not be registered with the Tick Loop
        /// </summary>
        public bool CanEverTick => canEverTick;

        /// <summary>
        /// Determines if we start with ticking enabled by default.
        /// </summary>
        public bool StartTickEnabled => startTickEnabled;

        /// <summary>
        /// Determines if we're currently ticking.
        /// </summary>
        public bool TickEnabled => tickEnabled;

        /// <summary>
        /// When this tick is executed.
        /// </summary>
        public TickGroup TickGroup => tickGroup;

        /// <summary>
        /// Interval in seconds in which the tick is executed.
        /// A value of less or equal to 0 means tick at framerate.
        /// </summary>
        public float TickInterval => tickInterval;

        /// <summary>
        /// Use this to determine the order in which this tick is ticking compared to others
        /// in the same tick group. Higher numbers are executed later. Smaller numbers are executed earlier.
        /// </summary>
        public int Priority => priority;

        /// <summary>
        /// The thing on which we will execute the tick function
        /// according to these here configurations.
        /// </summary>
        public ITickable TickTarget => tickTarget;

        public void SetCanEverTick(bool allowTick) {
            canEverTick = allowTick;
        }

        public override bool Equals(object obj) {
            if (!(obj is TickFunc otherFunc)) {
                return false;
            }
            return otherFunc.tickTarget == tickTarget;
        }

        public override int GetHashCode() {
            return tickTarget.GetHashCode();
        }

        public bool CanTickNow(float currentTime) {
            return tickEnabled && lastTick + tickInterval <= currentTime;
        }

        public void Tick(float deltaTime, float currentTime) {
            lastTick = currentTime;
            tickTarget.ProcessTick(deltaTime);
        }

        public void OnReset() {
            tickEnabled = startTickEnabled;
            lastTick = 0;
        }

        public void SetEnableTick(bool shouldTick) {
            tickEnabled = shouldTick;
        }

        public void SetTickTarget(ITickable tickable) {
            if (tickTarget != null) {
                Debug.LogError("Cannot override already initialised ticktarget.");
                return;
            }
            tickTarget = tickable;
        }
    }
}