using UnityEngine;

namespace Broilerplate.Tools {
    /// <summary>
    /// Just some helpie helpers to deal with directionality in vectors.
    /// </summary>
    public static class VectorTools {
        public static float Dot(Vector3 direction, Vector3 targetDirection) {
            return Vector3.Dot(direction, targetDirection);
        }

        public static bool IsFacing(Vector3 direction, Vector3 fromHere, Vector3 facingThis, float precision = .55f) {
            return Dot(direction, (facingThis - fromHere).normalized) >= precision;
        }
        
        public static bool IsFacingDirection(Vector3 direction, Vector3 otherDirection, float precision = .55f) {
            return Dot(direction, otherDirection) >= precision;
        }

        public static bool IsFacingWithinAngle(Vector3 direction, Vector3 fromHere, Vector3 facingThis, float angle = 15f) {
            float dot = Dot(direction, (facingThis - fromHere).normalized);
            return Mathf.Acos(dot) * Mathf.Rad2Deg <= angle;
        }
        
        public static bool IsFacingWithinAngle(Vector3 direction, Vector3 targetDirection, float angle = 15f) {
            float dot = Dot(direction, targetDirection);
            return Mathf.Acos(dot) * Mathf.Rad2Deg <= angle;
        }
    }
}