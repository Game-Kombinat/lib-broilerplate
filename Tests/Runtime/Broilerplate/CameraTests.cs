using System.Collections;
using Broilerplate.Core;
using Broilerplate.Gameplay;
using Broilerplate.Gameplay.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.Runtime.Broilerplate {
    public class CameraTests {
        private GameInstance instance;
        
        [SetUp]
        public void Pretest() {
            instance = GameInstance.GetInstance();
        }
        
        [Test]
        public void TestCameraComponentIntegrity() {
            var go = new GameObject("Camera Component test");
            var actor = instance.GetWorld().SpawnActorOn<Actor>(go);
            var cp = actor.AddGameComponent<CameraComponent>();
            Assert.IsNotNull(cp.CameraHandle, "cp.CameraHandle != null");
            
        }

        [Test]
        public void TestPlayerControllerInstantiatedCameraManager() {
            var pc = instance.GetWorld().GetGameMode().GetMainPlayerController();
            Assert.IsNotNull(pc.CameraManager, "pc.CameraManager != null");
            Assert.IsNotNull(pc.CameraManager.MainCamera, "pc.CameraManager.MainCamera != null");
        }
        
        [UnityTest]
        public IEnumerator TestAutoViewTargeting() {
            var pc = instance.GetWorld().GetGameMode().GetMainPlayerController();
            var go = new GameObject("Actor 1");
            var pawn1 = instance.GetWorld().SpawnActorOn<Pawn>(go);
            pawn1.AddGameComponent<CameraComponent>();

            var go2 = new GameObject("Actor 2");
            var pawn2 = instance.GetWorld().SpawnActorOn<Pawn>(go2);
            pawn2.AddGameComponent<CameraComponent>();
            
            var go3 = new GameObject("Actor 3");
            var noCameraPawn = instance.GetWorld().SpawnActorOn<Pawn>(go3);

            yield return null;
            
            pc.CameraManager.SetAutoViewTargeting(true);
            pc.ControlPawn(pawn1);

            Assert.True(pawn1 == pc.CameraManager.ViewTarget, "pawn1 == pc.CameraManager.ViewTarget");
            
            pc.ControlPawn(pawn2);
            
            Assert.True(pawn2 == pc.CameraManager.ViewTarget, "pawn2 == pc.CameraManager.ViewTarget");
            
            pc.ControlPawn(noCameraPawn);
            
            Assert.True(noCameraPawn != pc.CameraManager.ViewTarget, "noCameraPawn != pc.CameraManager.ViewTarget");
            
        }
        // todo: test camera manager auto view targeting works by switching control between 2 different actors with cameras attached.
    }
}