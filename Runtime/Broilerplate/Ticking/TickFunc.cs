using System;
using Broilerplate.Core;
using UnityEngine;

namespace Broilerplate.Ticking {
    /// <summary>
    /// Every actor and actor component can be ticked.
    /// This TickFunc class calls tick functions on tickable types
    /// according to its configuration.
    /// </summary>
    [Serializable]
    public class TickFunc {

        /// <summary>
        /// Determines if we're currently ticking.
        /// </summary>
        [SerializeField]
        private bool tickEnabled = false;

        /// <summary>
        /// When this tick is executed.
        /// </summary>
        [SerializeField]
        private TickGroup tickGroup = TickGroup.Tick;

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

        private int tickTargetHash;

        private bool isRegistered;

        /// <summary>
        /// If false, this tick function will never tick at all.
        /// It will not be registered with the Tick Loop
        /// </summary>
        public bool CanEverTick => tickGroup != TickGroup.None;

        /// <summary>
        /// Determines if we're currently ticking.
        /// </summary>
        public bool TickEnabled => tickEnabled;

        /// <summary>
        /// A mask of all tick groups this TickFunc is part of.
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

        public bool HasTickTarget => tickTarget != null;
        public bool IsRegistered => isRegistered;

        public override bool Equals(object obj) {
            if (!(obj is TickFunc otherFunc)) {
                return false;
            }
            return otherFunc.tickTargetHash == tickTargetHash;
        }

        public override int GetHashCode() {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return tickTargetHash;
        }

        public bool IsInTickGroup(TickGroup other) {
            return (tickGroup & other) != TickGroup.None;
        }

        public bool CanTickNow(float currentTime) {
            return tickEnabled && lastTick + tickInterval <= currentTime;
        }

        public void Tick(float deltaTime, float currentTime, TickGroup currentGroup) {
            lastTick = currentTime;
            tickTarget.ProcessTick(deltaTime, currentGroup);
        }

        public void OnReset() {
            lastTick = 0;
            isRegistered = false;
        }

        public void SetEnableTick(bool shouldTick) {
            tickEnabled = shouldTick;
            if (shouldTick) {
                tickTarget?.OnEnableTick();
            }
            else {
                tickTarget?.OnDisableTick();
            }
        }

        public void SetTickTarget(ITickable tickable) {
            if (tickTarget != null) {
                Debug.LogError("Cannot override already initialised ticktarget.");
                return;
            }
            tickTarget = tickable;
            tickTargetHash = tickable.GetHashCode() + priority.GetHashCode() + tickGroup.GetHashCode();
        }

        public void ClearTickTarget() {
            tickTarget = null;
            // info: we need the tick target hash to remain because if this tickfunc is scheduled for removal
            // from the tick loop then the hash is used to find it in the tick list
        }

        public void SetTickGroup(TickGroup inTickGroup) {
            tickGroup = inTickGroup;
        }

        public void SetTickInterval(float interval) {
            tickInterval = interval;
        }

        public void MarkRegistered() {
            isRegistered = true;
        }

        public void MarkUnregistered() {
            isRegistered = false;
        }
    }
}