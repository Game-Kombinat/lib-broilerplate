using System;
using System.Collections;
using System.Text.RegularExpressions;
using Broilerplate.Core;
using Broilerplate.Core.Components;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

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
        public IEnumerator TestDestroyComponents() {
            var go = new GameObject("Test Actor");
            var actor = instance.GetWorld().SpawnActorOn<Actor>(go);
            // we assert in another test that the adding works
            var cp1 = actor.AddGameComponent<SceneComponent>();
            var cp2 = actor.AddGameComponent<SceneComponent>();
            // ssert that we have 2 components now
            Assert.True(actor.NumComponents == 2, "actor.NumComponents == 2");
            
            Object.Destroy(cp1);
            yield return null;
            
            Assert.True(actor.NumComponents == 1, "actor.NumComponents == 1");
            
            cp1 = actor.AddGameComponent<SceneComponent>();
            Object.Destroy(cp1);
            Object.Destroy(cp2);
            yield return null;
            Assert.True(actor.NumComponents == 0, "actor.NumComponents == 0");

            var ac = actor.AddGameComponent<ActorComponent>();
            cp1 = actor.AddGameComponent<SceneComponent>();
            Assert.True(actor.NumComponents == 2, "actor.NumComponents == 2");
            Object.Destroy(cp1);
            Object.Destroy(ac);
            Assert.True(actor.NumComponents == 2, "actor.NumComponents == 2 (after destroy, same frame)");
            yield return null;
            Assert.True(actor.NumComponents == 0, "actor.NumComponents == 0 (after destroy, next frame)");
            
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestDetachedActorIsDestroyed() {
            var go = new GameObject("Test Actor");
            var actor = instance.GetWorld().SpawnActorOn<Actor>(go);
            var sc = actor.AddGameComponent<SceneComponent>();
            sc.DetachFromActor();
            Assert.True(actor.NumComponents == 1, "actor.NumComponents == 1 (after scene component detached)");
            Object.Destroy(actor);
            yield return null;
            // we cannot assert.null here because, apparently, the "unity null" is not the null that the
            // assert is expecting (it failed, literally because of "expected null but got <null>" ...
            Assert.True(sc == null, "sc == null after actor destruction");
        }
        
        [UnityTest]
        public IEnumerator TestAddActorComponentOnChildObjectThrows() {
            var go = new GameObject("Test Actor");
            var actor = instance.GetWorld().SpawnActorOn<Actor>(go);
            var sc = actor.AddGameComponent<SceneComponent>();

            // this actually throws in ActorComponent.Awake() but unity catches that and
            // turns it into a log message therefore we have to do a LogAssert.
            var ac = sc.gameObject.AddComponent<ActorComponent>();
            LogAssert.Expect(LogType.Exception, new Regex("Actor Component must be added to the root"));
            yield return null;
            // again: can't assert.null because unitys null override isn't the exact same as the null expected from the assert.
            Assert.True(ac == null, "ac == null after invalid component adding");
        }
        
        [UnityTest]
        public IEnumerator TestAddComponentOnEmptyObject() {
            var go = new GameObject("None Actor");
            var sc = go.AddComponent<SceneComponent>();
            LogAssert.Expect(LogType.Exception, new Regex("requires an Actor component on the root object"));
            yield return null;
            Assert.True(sc == null, "sc == null after invalid component adding");
            // because scene components delete their game object along with them
            Assert.True(go == null, "go == null after invalid component adding");

            go = new GameObject("None Actor");
            var ac = go.AddComponent<ActorComponent>();
            LogAssert.Expect(LogType.Exception, new Regex("requires an Actor component on the root object"));
            yield return null;
            Assert.True(ac == null, "ac == null after invalid component adding");
            // actor components don't delete their game object
            Assert.True(go != null, "go != null after invalid component adding");
        }
        
        [UnityTest]
        public IEnumerator TestSceneCompDeletesOwnGameObject() {
            var go = new GameObject("Test Actor");
            var actor = instance.GetWorld().SpawnActorOn<Actor>(go);
            var sc = actor.AddGameComponent<SceneComponent>();
            var scGo = sc.gameObject;
            Assert.True(go != scGo, "go != scGo");
            Object.Destroy(sc);
            yield return null;
            Assert.True(sc == null, "sc == null");
            Assert.True(scGo == null, "scGo == null");
            Assert.True(go != null, "go != null");
        }
        
        [UnityTest]
        public IEnumerator TestActorLiveListIntegrity() {
            var go = new GameObject("Test Actor");
            var actor = instance.GetWorld().SpawnActorOn<Actor>(go);
            Assert.True(instance.GetWorld().HasActors, "instance.GetWorld().HasActors");
            int numActors = instance.GetWorld().NumActors;
            Object.Destroy(actor);
            yield return null;
            Assert.True(instance.GetWorld().NumActors < numActors, "instance.GetWorld().NumActors < numActors");
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
