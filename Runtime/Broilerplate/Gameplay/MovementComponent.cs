using Broilerplate.Core.Components;
using Broilerplate.Ticking;
using UnityEngine;

namespace Broilerplate.Gameplay {
    public abstract class MovementComponent : ActorComponent {
        
        /// <summary>
        /// If true, the movement input will be discarded resulting in no pawn movement.
        /// </summary>
        [Header("Basic Movement Setup")]
        [SerializeField] 
        private bool ignoreMovementInput;
        
        [SerializeField] 
        private bool ignoreRollRotation;

        protected Vector3 frameMovement;

        protected Vector3 frameRotation;

        protected Quaternion controlRotation;
        
        protected bool IgnoreMovementInput => ignoreMovementInput;
        
        public Pawn Pawn => Owner as Pawn;

        public void EnableInput() {
            ignoreMovementInput = false;
            SetEnableTick(true);
        }

        public void DisableInput() {
            ignoreMovementInput = true;
            SetEnableTick(false);
        }
        
        public virtual void AddMovementInput(float x, float y, float z) {
            if (!IgnoreMovementInput) {
                frameMovement += new Vector3(x, y, z);
            }
        }
        
        public virtual void AddRotationInput(float x, float y, float z) {
            if (!IgnoreMovementInput) {
                frameRotation += new Vector3(x, y, z);
            }
        }
        
        protected void UpdateControlRotation(Vector2 rotationLimitMax, Vector2 rotationLimitMin) {
            
            frameRotation.x = ClampAngle(frameRotation.x, rotationLimitMin.x, rotationLimitMax.x);
            frameRotation.y = ClampAngle(frameRotation.y, rotationLimitMin.y, rotationLimitMax.y);
            
            var x = Quaternion.AngleAxis(frameRotation.x, Vector3.up);
            var y = Quaternion.AngleAxis(frameRotation.y, Vector3.left);
            var z = ignoreRollRotation ? Quaternion.AngleAxis(controlRotation.eulerAngles.z, Vector3.forward) : Quaternion.AngleAxis(frameRotation.z, Vector3.forward);
            controlRotation = x * y * z;
        }
        
        protected static float ClampAngle(float angle, float min, float max) {
            return Mathf.Clamp(Mathf.DeltaAngle(0, angle), min, max);
        }

        /// <summary>
        /// Process the input data here.
        /// Ie. pipe through to a CharacterMovement component or whatever else drives this pawn movement.
        /// </summary>
        protected abstract void InternalApplyInput(float deltaTime, TickGroup tickGroup);

        protected override void Reset() {
            base.Reset();
            componentTick.SetTickGroup(TickGroup.LateTick);
        }

        public override void ProcessTick(float deltaTime, TickGroup tickGroup) {
            if (!IgnoreMovementInput) {
                InternalApplyInput(deltaTime, tickGroup);
            }
        }
    }
}