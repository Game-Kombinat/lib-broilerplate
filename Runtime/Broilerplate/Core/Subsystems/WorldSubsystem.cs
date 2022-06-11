using System;
using Broilerplate.Ticking;
using UnityEngine;

namespace Broilerplate.Core.Subsystems {
    public class WorldSubsystem : SubsystemBase, ITickable, IThing {
        [SerializeField]
        protected TickFunc worldTick;
        
        protected World world;
        
        public override void BeginPlay() {
            base.BeginPlay();
            if (worldTick.CanEverTick) {
                worldTick.SetTickTarget(this);
                world.RegisterTickFunc(worldTick);
            }
        }

        public virtual void ProcessTick(float deltaTime) {
            
        }

        public World GetWorld() => world;

        public void SetWorld(World inWorld) {
            world = inWorld;
        }

    }
}
