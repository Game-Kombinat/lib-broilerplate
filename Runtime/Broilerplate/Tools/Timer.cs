using System;
using Broilerplate.Core;
using Broilerplate.Ticking;
using DataKombinat.Binary.Tags;
using UnityEngine;

namespace Broilerplate.Tools {
    public class Timer : ITickable {
        private TimeSpan duration = TimeSpan.Zero;
        private TimeSpan currentTimer;
        
        private TickFunc tickFunc = new();
        private World world;
        
        /// <summary>
        /// Raised when this timer is started.
        /// Ideally subscribe to this very early in BeginPlay.
        /// That way you get the notifications when restoring timer data
        /// </summary>
        public event Action OnStartTimer;
        /// <summary>
        /// Raised when this timer is finished.
        /// Ideally subscribe to this very early in BeginPlay.
        /// That way you get the notifications when restoring timer data
        /// </summary>
        public event Action OnFinish;
        
        /// <summary>
        /// Raised when this timer is being ticked at its configured interval.
        /// Ideally subscribe to this very early in BeginPlay.
        /// That way you get the notifications when restoring timer data
        /// </summary>
        public event Action OnTick;
        
        public float Progress => duration == TimeSpan.Zero ? 1 : Mathf.InverseLerp(0, (float)duration.TotalSeconds, (float)currentTimer.TotalSeconds);
        
        public TimeSpan TimeLeft => duration - currentTimer;
        
        public TimeSpan TimeElapsed => currentTimer;
        
        public bool IsDone => Progress >= 1;
        
        public void StartTimer(TimeSpan timerDuration, float timerInterval = 1f) {
            duration = timerDuration;
            currentTimer = TimeSpan.Zero;
            SetupTickFunc(timerInterval);
            OnStartTimer?.Invoke();
        }
        
        private void SetTimer(TimeSpan current, TimeSpan timerDuration, float timerInterval = 1f) {
            duration = timerDuration;
            currentTimer = current;
            SetupTickFunc(timerInterval);
            OnStartTimer?.Invoke();
        }
        
        public void Clear() {
            duration = TimeSpan.Zero;
            currentTimer = TimeSpan.Zero;
            SetEnableTick(false);
        }
        
        public void Destroy() {
            SetEnableTick(false);
            UnregisterTickFunc();
            OnStartTimer = null;
            OnFinish = null;
            OnTick = null;
        }
        
        private void SetupTickFunc(float timerInterval) {
            world = GameInstance.GetInstance().GetWorld();
            UnregisterTickFunc();
            tickFunc = new TickFunc();
            tickFunc.SetTickInterval(timerInterval);
            tickFunc.SetTickGroup(TickGroup.Tick);
            tickFunc.SetTickTarget(this);
            SetEnableTick(true);
            world.RegisterTickFunc(tickFunc);
        }
        
        public void ProcessTick(float deltaTime, TickGroup tickGroup) {
            // deltaTime is accumulative with tick intervals. so this will actually work like that.
            Debug.Log($"TIMER DELTA: {deltaTime}");
            currentTimer = currentTimer.Add(TimeSpan.FromSeconds(deltaTime));
            OnTick?.Invoke();
            
            TryFinish();
        }
        
        public void SetEnableTick(bool shouldTick) {
            tickFunc.SetEnableTick(shouldTick);
        }
        
        public void UnregisterTickFunc() {
            if (world && tickFunc != null) {
                world.UnregisterTickFunc(tickFunc);
            }
        }
        public void OnEnableTick() { }
        public void OnDisableTick() { }
        
        public NbtCompound GetSaveData() {
            return new NbtCompound() {
                new NbtLong("duration", duration.Ticks),
                new NbtLong("current", currentTimer.Ticks),
                new NbtFloat("interval", tickFunc.TickInterval),
                new NbtInt("wasTicking", tickFunc.TickEnabled ? 1 : 0)
            };
        }
        
        public void SetSaveData(NbtCompound data) {
            if (data == null) {
                return;
            }
            
            var duration = TimeSpan.FromMinutes(data["duration"]?.DoubleValue ?? 0d);
            var current = TimeSpan.FromTicks(data["current"]?.LongValue ?? 0);
            var interval = data["interval"]?.FloatValue ?? 1;
            
            bool wasTicking = (data["wasTicking"]?.IntValue ?? 0) == 1;
            
            if (wasTicking) {
                SetTimer(current, duration, interval);
            }
        }
        
        public void ForceDone() {
            currentTimer = duration;
            TryFinish();
        }

        private void TryFinish() {
            if (IsDone) {
                OnFinish?.Invoke();
                Clear();
            }
        }

        public void ShortenDuration(TimeSpan withTime) {
            var durationToSet = TimeLeft - withTime;
            if (durationToSet > TimeSpan.Zero) {
                StartTimer(durationToSet);
            }
            else {
                ForceDone();
            }
        }
    }
}