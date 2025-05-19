using Broilerplate.Core.Components;
using Unity.Cinemachine;
using UnityEngine;

namespace Broilerplate.Gameplay.View {
    /// <summary>
    /// Broilerplate camera component.
    /// This does not attempt to replace the unity camera or cinemachine camera.
    /// It adds an extra layer of control that is exposed for the CameraManager
    /// to make use of controlling view targets and so forth.
    /// </summary>
    public class CameraComponent : ActorComponent {
        private CinemachineVirtualCameraBase cameraHandle;

        public CinemachineVirtualCameraBase CameraHandle => cameraHandle;

        public Camera MainCamera { get; protected set; }

        public override void BeginPlay() {
            base.BeginPlay();
            cameraHandle = GetComponent<CinemachineVirtualCameraBase>();
            if (!cameraHandle) {
                Debug.LogWarning("No Virtual Camera found on CameraComponents GameObject. Creating default one!");
                cameraHandle = gameObject.AddComponent<CinemachineVirtualCamera>();
            }

            MainCamera = GetWorld().GetGameMode().GetMainPlayerController().CameraManager.MainCamera;
            
            DisableCamera();
        }

        public virtual void DisableCamera() {
            cameraHandle.enabled = false;
            cameraHandle.Priority = -1;
        }

        public virtual void EnableCamera(int priorityMargin = 0) {
            cameraHandle.enabled = true;
            cameraHandle.Priority = priorityMargin + 1;
        }
    }
}