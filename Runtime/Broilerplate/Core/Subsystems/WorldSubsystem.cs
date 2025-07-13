using System;
using Broilerplate.Ticking;
using UnityEngine;

namespace Broilerplate.Core.Subsystems {
    /// <summary>
    /// Subsystem whose life-cycle is tied to the life cycle of a world.
    /// That is: It gets loaded with a scene and destroyed with the unloading of a scene.
    /// </summary>
    public class WorldSubsystem : SubsystemBase, ITickable, IThing {
        [SerializeField]
        protected TickFunc worldTick;
        
        protected World world;

        public override void LateBeginPlay() {
            base.LateBeginPlay();
            if (worldTick.CanEverTick) {
                worldTick.SetTickTarget(this);
                world.RegisterTickFunc(worldTick);
            }
        }

        public virtual void OnDestroy() {

        }

        public virtual void ProcessTick(float deltaTime, TickGroup tickGroup) {
            
        }

        public void SetEnableTick(bool shouldTick) {
            if (!worldTick.CanEverTick) {
                Debug.LogWarning($"Attempted to change tick on WorldSubsystem that never ticks: {name}");
                return;
            }

            worldTick.SetEnableTick(shouldTick);
        }

        public void UnregisterTickFunc() {
            if (world) {
                world.UnregisterTickFunc(worldTick);
            }
        }

        public virtual void OnEnableTick() {
            
        }

        public virtual void OnDisableTick() {
            
        }

        public World GetWorld() => world;
        public int GetRuntimeId() {
            return GetInstanceID();
        }

        public void SetWorld(World inWorld) {
            world = inWorld;
        }

        public static T GetSubsystem<T>() where T : WorldSubsystem {
            var world = GameInstance.GetInstance().GetWorld();
            return world.GetSubsystem<T>();
        }

    }
}
