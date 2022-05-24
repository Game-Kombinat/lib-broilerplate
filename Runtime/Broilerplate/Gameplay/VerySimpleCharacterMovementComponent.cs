using UnityEngine;

namespace Broilerplate.Gameplay {
    [RequireComponent(typeof(CharacterController))]
    public class VerySimpleCharacterMovementComponent : MovementComponent {

        [Header("Movement Behaviour")]
        [SerializeField]
        private float movementSpeed = 5;
        
        [SerializeField]
        [Range(0, 1)]
        private float walkingModifier = .55f;
        
        [Header("Look Behavior")]
        [SerializeField]
        private float lookSensitivity = 1;

        [SerializeField]
        private Vector2 rotationLimitMin;
        
        [SerializeField]
        private Vector2 rotationLimitMax;
        
        [Header("Jumping")]
        [SerializeField]
        private float gravity = 9.81f;

        [SerializeField]
        private float jumpHeight = 2f;

        private CharacterController controller;

        private float activeMovementSpeed;
        
        private float lastDeltaTime;
        
        public override void BeginPlay() {
            base.BeginPlay();
            controller = GetComponent<CharacterController>();
            activeMovementSpeed = movementSpeed;
        }

        public void Jump() {
            if (controller.isGrounded) {
                frameMovement.y = Mathf.Sqrt(jumpHeight * -2 * (-gravity));
            }
        }

        public void BeginCrouch() {
            activeMovementSpeed = movementSpeed * walkingModifier;
        }

        public void EndCrouch() {
            activeMovementSpeed = movementSpeed;
        }

        public override void AddRotationInput(float x, float y, float z) {
            if (!IgnoreMovementInput) {
                frameRotation += new Vector3(x, y, z) * (lookSensitivity * lastDeltaTime);
            }
        }

        protected override void InternalApplyInput(float deltaTime) {
            frameMovement.y = Mathf.Max(frameMovement.y - gravity * (deltaTime * 2), -gravity);
            var pawnTransform = Pawn.GetControlTransform();
            var pawnForward = pawnTransform.forward;
            var pawnRight = pawnTransform.right;

            var finalMovement = pawnForward * frameMovement.z + pawnRight * frameMovement.x;
            // finalMovement.y = frameMovement.y;
            finalMovement = Vector3.ClampMagnitude(finalMovement * activeMovementSpeed, activeMovementSpeed);
            controller.Move(finalMovement * deltaTime);
            controller.Move(Vector3.up * (frameMovement.y * deltaTime));
            
            float frameGravity = frameMovement.y;
            frameMovement = Vector3.zero;
            frameMovement.y = frameGravity;

            UpdateControlRotation(rotationLimitMax, rotationLimitMin);
            Pawn.SetControlRotation(controlRotation);
            lastDeltaTime = deltaTime;
        }
    }
}