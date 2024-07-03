using System;
using Broilerplate.Core;
using Broilerplate.Ticking;
using GameKombinat.Fnbt;
using UnityEngine;

namespace Broilerplate.Tools {
    public class Timer : ITickable {
        private DateTime start;
        private TimeSpan duration = TimeSpan.Zero;
        
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
        
        public float Progress => duration == TimeSpan.Zero ? 1 : Mathf.InverseLerp(0, (float)duration.TotalSeconds, (float)TimeElapsed.TotalSeconds);
        
        public DateTime Finish => start + duration;
        
        public TimeSpan TimeLeft => Finish - DateTime.Now;
        
        public TimeSpan TimeElapsed => DateTime.Now - start;
        public bool IsDone => Progress >= 1;
        
        public void StartTimer(TimeSpan time, float timerInterval = 1f) {
            start = DateTime.Now;
            duration = time;
            SetupTickFunc(timerInterval);
            OnStartTimer?.Invoke();
        }
        
        public void SetTimer(DateTime from, TimeSpan time, float timerInterval = 1f) {
            start = from;
            duration = time;
            SetupTickFunc(timerInterval);
            OnStartTimer?.Invoke();
        }
        
        public void Clear() {
            start = DateTime.Now;
            duration = TimeSpan.FromMinutes(0);
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
            OnTick?.Invoke();
            
            TryFinish();
        }
        
        public void SetEnableTick(bool shouldTick) {
            tickFunc.SetEnableTick(shouldTick);
        }
        
        public void UnregisterTickFunc() {
            if (world) {
                world.UnregisterTickFunc(tickFunc);
            }
        }
        public void OnEnableTick() { }
        public void OnDisableTick() { }
        
        public NbtCompound GetSaveData() {
            return new NbtCompound() {
                new NbtDouble("time", duration.TotalMinutes),
                new NbtLong("from", start.ToBinary()),
                new NbtFloat("interval", tickFunc.TickInterval),
                new NbtInt("wasTicking", tickFunc.TickEnabled ? 1 : 0)
            };
        }
        
        public void SetSaveData(NbtCompound data) {
            if (data == null) {
                return;
            }
            
            var time = TimeSpan.FromMinutes(data["time"]?.DoubleValue ?? 0d);
            var from = DateTime.FromBinary(data["from"]?.LongValue ?? 0);
            var interval = data["interval"]?.FloatValue ?? 1;
            
            bool wasTicking = (data["wasTicking"]?.IntValue ?? 0) == 1;
            
            if (wasTicking) {
                SetTimer(from, time, interval);
            }
        }
        
        public void ForceDone() {
            start = DateTime.Now - duration;
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