﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Broilerplate.Ticking {
    /// <summary>
    /// This is a list that contains <see cref="TickFunc"/> references.
    /// There is one TickList for every relevant update loop (Pre Physics, Physics, Late Update)
    /// </summary>
    public class TickList {
        public readonly List<TickFunc> subjects = new List<TickFunc>();

        private TickManager tickManager;

        public TickList(TickManager manager) {
            tickManager = manager;
        }

        public void Add(TickFunc tickFunc) {
            subjects.Add(tickFunc);
        }

        public void Sort() {
            subjects.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        }

        public void Tick(float deltaTime, float timeSinceWorldBoot, TickGroup currentGroup) {
            for (int i = 0; i < subjects.Count; ++i) {
                var tick = subjects[i];
                // This happens when actors get destroyed that have not been processed yet.
                // It's rare but especially prone to happen on low-end devices.
                if (tick == null || !tick.HasTickTarget) {
                    Debug.LogWarning("Found null reference in ticking subject pool or TickFunc without target. Removing it.");
                    subjects.RemoveAt(i--);
                    continue;
                }
                
                if (tick.CanTickNow(timeSinceWorldBoot, tickManager.IsPaused)) {
                    try {
                        tick.Tick(deltaTime, timeSinceWorldBoot, currentGroup);
                    }
                    catch (Exception e) {
                        Debug.LogException(e);
                    }
                }
            }
        }

        public void Remove(TickFunc tickFunc) {
            subjects.Remove(tickFunc);
        }
    }
}