using Broilerplate.Core;
using Broilerplate.Gameplay;
using Broilerplate.Gameplay.Input;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tests.Runtime.Broilerplate {
    /// <summary>
    /// This tests that everything pertaining to creating a playable state in the
    /// game is bootstrapping correctly and behaves as expected.
    /// todo: These tests rely on the configuration. Problem is that it may vary and isn't always the same.
    /// We have to find a way to mock the configuration when testing. Or better yet. Create a mocking game instance 
    /// </summary>
    public class BootstrappingTests {
        private GameInstance instance;
        [SetUp]
        public void Pretest() {
            instance = GameInstance.GetInstance();
        }
        
        [Test]
        public void TestBroilerConfigBasicTypes() {
            Assert.NotNull(instance.GameInstanceConfiguration, "instance.Configuration != null");
            var cfg = instance.GameInstanceConfiguration;
            // assert rudimentary facts
            Assert.NotNull(cfg.WorldType, "cfg.WorldType != null");
            Assert.NotNull(cfg.GameInstanceType, "cfg.GameInstanceType != null");
        }
        
        [Test]
        public void TestBroilerConfigGameModeBehaviour() {
            // game modes can return null as they are actor types.
            // that means the world takes care of defaulting to something
            // Because if the config returned a default object it would already be spawned in the scene.
            // And we don't want that. Hence we test that this behaviour is working as expected.
            // There is, of course, a way with the prefab utils but it's inherently editor bound and this
            // oughta work at runtime so this must be the way to go.
            var cfg = ScriptableObject.CreateInstance<BroilerConfiguration>();
            
            Scene nullOverrideScene = SceneManager.CreateScene("TemporaryScene", new CreateSceneParameters { localPhysicsMode = LocalPhysicsMode.Physics3D });
            Scene notNullOverrideScene = SceneManager.GetActiveScene();
            
            // for testing purposes we just create a game object with a game mode on it
            // and pass it in as override. No other way to test this.
            var gm = new GameObject("TmpGameMode", typeof(GameMode));
            
            Assert.DoesNotThrow(() => { cfg.RegisterSceneOverride(nullOverrideScene, null); }, "RegisterSceneOverride does not throw");
            Assert.DoesNotThrow(() => { cfg.RegisterSceneOverride(notNullOverrideScene, gm.GetComponent<GameMode>()); }, "RegisterSceneOverride does not throw");
            
            Assert.IsNull(cfg.GetGameModeFor(nullOverrideScene), "cfg.GetGameModeFor(nullOverrideScene) == null");
            Assert.NotNull(cfg.GetGameModeFor(notNullOverrideScene), "cfg.GetGameModeFor(notNullOverrideScene) != null");
            
            // and to test that the non null is correct value:
            Assert.True(gm.GetComponent<GameMode>() == cfg.GetGameModeFor(notNullOverrideScene), "gm == cfg.GetGameModeFor(notNullOverrideScene)");
        }
        
        [Test]
        public void TestGameModeIntegrity() {
            GameMode mode = instance.GetWorld().GetGameMode();
            Assert.NotNull(mode, "mode != null");
            var cfg = instance.GameInstanceConfiguration;
            var gmType = cfg.GetGameModeFor(SceneManager.GetActiveScene());
            
            if (gmType == null) {
                Assert.True(mode.GetType() == typeof(GameMode), "mode.GetType() == typeof(GameMode)");
            }
            else {
                
                Assert.True(mode.GetType() == gmType.GetType(), "mode.GetType() == gmType.GetType()");
            }
            
        }
        
        
        [Test]
        public void TestPlayerControllerIntegrity() {
            GameMode mode = instance.GetWorld().GetGameMode();
            var pc = mode.GetMainPlayerController();
            Assert.NotNull(pc, "pc != null");
            
            var pcType = mode.GetPlayerControllerType();
            if (pcType == null) {
                Assert.True(pc.GetType() == typeof(PlayerController));
            }
            else {
                Assert.True(pc.GetType() == pcType.GetType());
            }
        }
        
        [Test]
        public void TestPlayerPawnIntegrity() {
            GameMode mode = instance.GetWorld().GetGameMode();
            var pc = mode.GetMainPlayerController();
            Pawn pcPawn = pc.ControlledPawn;
            Assert.NotNull(pcPawn, "pcPawn != null");

            var pawnType = mode.GetPlayerPawnType();
            // pawn types can be null (it's a little sad but these are not actually c# types but prefabs, so no way around that)
            if (pawnType == null) {
                Assert.True(pcPawn.GetType() == typeof(Pawn), "pcPawn.GetType() == typeof(Pawn)");
            }
            else {
                Assert.True(pcPawn.GetType() == pawnType.GetType(), "pcPawn.GetType() == pawnType.GetType()");
            }
            
            Assert.True(pcPawn.GetController() == pc, "pcPawn.GetController() == pc");
        }
    }
}