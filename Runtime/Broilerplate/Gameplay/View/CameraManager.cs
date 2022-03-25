using System;
using Broilerplate.Core;
using Cinemachine;
using UnityEngine;

namespace Broilerplate.Gameplay.View {
    
    /// <summary>
    /// Camera manager handles 
    /// </summary>
    public class CameraManager : Actor {
        /// <summary>
        /// Automatically activate the camera on pawns controlled
        /// by the player controller, if they have a camera component attached to them.
        /// </summary>
        [SerializeField]
        private bool autoViewTargeting = true;

        [SerializeField]
        private Camera mainCameraType;
        
        private Actor viewTargetActor;
        private CameraComponent targetCamera;

        private Camera mainCamera;

        public bool AutoViewTargeting => autoViewTargeting;

        public Actor ViewTarget => viewTargetActor;

        public CameraComponent CameraComponent => targetCamera;

        public Camera MainCamera => mainCamera;

        public override void BeginPlay() {
            base.BeginPlay();
            // we need a main camera for cinemachine to work of course.
            // So lets make sure there is one in the scene.
            mainCamera = Camera.main;
            if (!mainCamera) {
                if (mainCameraType) {
                    // This is a "unity native" type. We can't spawn it via world.
                    mainCamera = Instantiate(mainCameraType);
                    var testBrain = mainCamera.gameObject.GetComponentInChildren<CinemachineBrain>();
                    if (!testBrain) {
                        Debug.LogWarning("the camera type in CameraManager does not have a CinemachineBrain on it. Camera management may not work correctly.");
                    }
                }
                else {
                    var go = new GameObject("Main Camera");
                    mainCamera = go.AddComponent<Camera>();
                    mainCamera.tag = "MainCamera";
                    var brain = go.AddComponent<CinemachineBrain>();
                    brain.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.EaseInOut, .3f);
                }
                
            }
        }

        protected override void DestroyActor() {
            base.DestroyActor();
            if (mainCamera) {
                Destroy(mainCamera.gameObject);
            }
        }

        public void SetViewTarget(Actor actor) {
            // we must use the broilerplate GetGameComponent here as the camera
            // is likely on a detached game object and could not be found with
            // unitys GetComponent
            var newTargetCamera = actor.GetGameComponent<CameraComponent>();
            if (newTargetCamera) {
                if (targetCamera) {
                    targetCamera.DisableCamera();
                }
                newTargetCamera.EnableCamera();
                viewTargetActor = actor;
                targetCamera = newTargetCamera;
            }
        }

        public void SetAutoViewTargeting(bool enable) {
            autoViewTargeting = enable;
        }
    }
}