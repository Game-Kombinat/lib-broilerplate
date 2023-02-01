using Broilerplate.Core;
using Broilerplate.Ticking;
using UnityEditor;
using UnityEngine;

namespace Broilerplate.Gameplay {
    public class PlayerStart : Actor {
        public PlayerStart() {
            actorTick.SetTickGroup(TickGroup.None);
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos() {
            Handles.color = Color.green;
            Matrix4x4 localSpace = Matrix4x4.TRS(transform.position, transform.rotation, Handles.matrix.lossyScale);
            using (new Handles.DrawingScope(localSpace)) {
                DrawWireCapsule(.5f, 2f);
                
            }
            // this clear color creates a selectable box that is invisible but clickable,
            // because the handles are not selectable
            Gizmos.color = Color.clear;
            Gizmos.DrawCube(transform.position, new Vector3(1f, 2f, 1f));
        }

        public static void DrawWireCapsule(float radius, float height) {
            var pointOffset = (height - (radius * 2)) / 2;

            // draw sideways
            Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.left, Vector3.back, -180, radius, 2);
            Handles.DrawLine(new Vector3(0, pointOffset, -radius), new Vector3(0, -pointOffset, -radius), 2);
            Handles.DrawLine(new Vector3(0, pointOffset, radius), new Vector3(0, -pointOffset, radius), 2);
            Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.left, Vector3.back, 180, radius, 2);
            // draw frontways
            Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.back, Vector3.left, 180, radius, 2);
            Handles.DrawLine(new Vector3(-radius, pointOffset, 0), new Vector3(-radius, -pointOffset, 0), 2);
            Handles.DrawLine(new Vector3(radius, pointOffset, 0), new Vector3(radius, -pointOffset, 0), 2);
            Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.back, Vector3.left, -180, radius, 2);
            // draw center
            Handles.DrawWireDisc(Vector3.up * pointOffset, Vector3.up, radius, 2);
            Handles.DrawWireDisc(Vector3.down * pointOffset, Vector3.up, radius, 2);
            // draw forward indicator
            Handles.DrawLine(Vector3.zero, Vector3.forward, 4);
        }
#endif
    }
}