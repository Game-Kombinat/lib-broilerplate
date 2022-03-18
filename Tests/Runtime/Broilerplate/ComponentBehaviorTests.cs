using System.Collections;
using Broilerplate.Core;
using Broilerplate.Core.Components;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.Runtime.Broilerplate {
    public class ComponentBehaviorTests
    {
        private GameInstance instance;
        
        [SetUp]
        public void Pretest() {
            instance = GameInstance.GetInstance();
        }

        [UnityTest]
        public IEnumerator TestComponentTargetObjects() {
            var go = new GameObject("Test Actor");
            var actor = instance.GetWorld().SpawnActorOn<Actor>(go);
            var sc = actor.AddGameComponent<SceneComponent>();
            Assert.NotNull(sc, "sc != null");
            // assert that scene component landed on a different game object because that is the expected behaviour
            Assert.True(sc.gameObject != actor.gameObject, "sc.gameObject != actor.gameObject");

            var ac = actor.AddGameComponent<ActorComponent>();
            Assert.NotNull(ac, "ac != null");
            // assert that adding a new actor component did not put it on a new gameobject
            Assert.True(ac.gameObject == actor.gameObject, "ac.gameObject == actor.gameObject");
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator TestAddGetComponents() {
            var go = new GameObject("Test Actor");
            var actor = instance.GetWorld().SpawnActorOn<Actor>(go);
            Assert.True(actor.HasTickFunc, "actor.HasTickFunc");
            // we assert in another test that the adding works
            var sc = actor.AddGameComponent<SceneComponent>();
            var ac = actor.AddGameComponent<ActorComponent>();
            var testSc = actor.GetGameComponent<SceneComponent>();
            var testAc = actor.GetGameComponent<ActorComponent>();
            // Asset that the scene component we added added is the same that is returned afterwards
            Assert.True(testSc == sc, "testSc == sc");
            Assert.True(testAc == ac, "testAc == ac");
            
            // assert that adding a new actor component did not put it on a new gameobject
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator TestRemoveComponents() {
            var go = new GameObject("Test Actor");
            var actor = instance.GetWorld().SpawnActorOn<Actor>(go);
            // we assert in another test that the adding works
            actor.AddGameComponent<SceneComponent>();
            actor.AddGameComponent<SceneComponent>();
            // ssert that we have 2 components now
            Assert.True(actor.NumComponents == 2, "actor.NumComponents == 2");
            
            actor.RemoveGameComponent<SceneComponent>();
            yield return null;
            Assert.DoesNotThrow(() => { actor.ProcessComponentRemoval(); });
            
            Assert.True(actor.NumComponents == 1, "actor.NumComponents == 1");
            
            actor.AddGameComponent<SceneComponent>();
            actor.RemoveGameComponent<SceneComponent>(true);
            yield return null;
            Assert.DoesNotThrow(() => { actor.ProcessComponentRemoval(); }); 
            
            Assert.True(actor.NumComponents == 0, "actor.NumComponents == 0");

            actor.AddGameComponent<ActorComponent>();
            actor.AddGameComponent<SceneComponent>();
            
            actor.RemoveGameComponent<SceneComponent>();
            yield return null;
            Assert.DoesNotThrow(() => { actor.ProcessComponentRemoval(); }); 
            Assert.True(actor.NumComponents == 1, "actor.NumComponents == 1 (actor component left)");
            
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestSceneComponentRemovalWithOtherComponentsOn() {
            var go = new GameObject("Test Actor");
            var actor = instance.GetWorld().SpawnActorOn<Actor>(go);
            var sc = actor.AddGameComponent<SceneComponent>();
            // This is ofc illegal but this must not blow regardless
            sc.gameObject.AddComponent<ActorComponent>();
            actor.RemoveGameComponent(sc);
            yield return null;
            Assert.DoesNotThrow(() => { actor.ProcessComponentRemoval(); });
        }
        
        [UnityTest]
        public IEnumerator TestTicking() {
            var go = new GameObject("Test Actor");
            var actor = instance.GetWorld().SpawnActorOn<Actor>(go);
            actor.SetEnableTick(true);
            // we assert in another test that the adding works
            var sc = actor.AddGameComponent<SceneComponent>();
            sc.SetEnableTick(true);
            yield return null;
            // todo: we would have to mock up the actor and component types here
            // and ensure that their tick functions have been called accordingly.
            // for that we need a mocking framework
            // for now I have tested it manually and it works but this definitely needs to be
            // tested constantly
        }
    }
}
