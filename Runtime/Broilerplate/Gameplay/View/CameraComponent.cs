using Broilerplate.Core.Components;
using Cinemachine;
using UnityEngine;

namespace Broilerplate.Gameplay.View {
    /// <summary>
    /// Broilerplate camera component.
    /// This does not attempt to replace the unity camera or cinemachine camera.
    /// It adds an extra layer of control that is exposed for the CameraManager
    /// to make use of controlling view targets and so forth.
    /// </summary>
    [RequireComponent(typeof(CinemachineVirtualCamera))]
    public class CameraComponent : SceneComponent {
        private CinemachineVirtualCamera cameraHandle;

        public CinemachineVirtualCamera CameraHandle => cameraHandle;

        public override void BeginPlay() {
            base.BeginPlay();
            cameraHandle = GetComponent<CinemachineVirtualCamera>();
            DisableCamera();
        }

        public void DisableCamera() {
            cameraHandle.enabled = false;
            cameraHandle.Priority = -1;
        }

        public void EnableCamera(int priorityMargin = 0) {
            cameraHandle.enabled = true;
            cameraHandle.Priority = priorityMargin + 1;
        }
    }
}