using System;
using System.Collections;
using System.Text.RegularExpressions;
using Broilerplate.Core;
using Broilerplate.Core.Components;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

// todo: fix these tests, the actor/scene component differentiation has been removed.
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
            var sc = actor.AddGameComponent<ActorComponent>();
            Assert.NotNull(sc, "sc != null");
            Assert.True(sc.gameObject == actor.gameObject, "sc.gameObject == actor.gameObject");
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator TestAddGetComponents() {
            var go = new GameObject("Test Actor");
            var actor = instance.GetWorld().SpawnActorOn<Actor>(go);
            Assert.True(actor.HasTickFunc, "actor.HasTickFunc");
            // we assert in another test that the adding works
            var sc = actor.AddGameComponent<ActorComponent>();
            var testSc = actor.GetGameComponent<ActorComponent>();
            // Asset that the scene component we added added is the same that is returned afterwards
            Assert.True(testSc == sc, "testSc == sc");
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator TestDestroyComponents() {
            var go = new GameObject("Test Actor");
            var actor = instance.GetWorld().SpawnActorOn<Actor>(go);
            // we assert in another test that the adding works
            var cp1 = actor.AddGameComponent<ActorComponent>();
            var cp2 = actor.AddGameComponent<ActorComponent>();
            // assert that we have 2 components now
            Assert.True(actor.NumComponents == 2, "actor.NumComponents == 2");
            
            Object.Destroy(cp1);
            yield return null;
            
            Assert.True(actor.NumComponents == 1, "actor.NumComponents == 1");
            
            cp1 = actor.AddGameComponent<ActorComponent>();
            Object.Destroy(cp1);
            Object.Destroy(cp2);
            yield return null;
            Assert.True(actor.NumComponents == 0, "actor.NumComponents == 0");

            var ac = actor.AddGameComponent<ActorComponent>();
            cp1 = actor.AddGameComponent<ActorComponent>();
            Assert.True(actor.NumComponents == 2, "actor.NumComponents == 2");
            Object.Destroy(cp1);
            Object.Destroy(ac);
            Assert.True(actor.NumComponents == 2, "actor.NumComponents == 2 (after destroy, same frame)");
            yield return null;
            Assert.True(actor.NumComponents == 0, "actor.NumComponents == 0 (after destroy, next frame)");
            
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator TestActorIntegrityMeasures() {
            var actorObject = new GameObject("Test Actor");
            var actorChild = new GameObject("Actor Child");
            actorChild.transform.parent = actorObject.transform;

            var actorGrandChild = new GameObject("Actor Grandchild");
            actorGrandChild.transform.parent = actorChild.transform;
            
            var actor = instance.GetWorld().SpawnActorOn<Actor>(actorObject);
            
            // note: we test that we cannot add components on non-actor objects in another test, no need to repeat that here.

            var ac = actor.AddGameComponent<ActorComponent>();
            Assert.True(ac != null, "ac != null");
            Assert.True(actor.NumComponents == 1, "actor.NumComponents == 1");

            // This should still register to actor
            ac = actorChild.AddComponent<ActorComponent>();
            Assert.True(ac != null, "ac != null");
            Assert.True(actor.NumComponents == 2, "actor.NumComponents == 2");
            
            // Doesn't matter where we add the actor component, it must find its actor
            ac = actorGrandChild.AddComponent<ActorComponent>();
            Assert.True(ac != null, "ac != null");
            Assert.True(actor.NumComponents == 3, "actor.NumComponents == 3");

            // change the root, this must make no difference to the component integrity measures
            var root = new GameObject("Root");
            actor.transform.parent = root.transform;

            // None of these may throw and should find their actors
            ac = actor.AddGameComponent<ActorComponent>();
            Assert.True(ac != null, "ac != null");
            ac = actorGrandChild.AddComponent<ActorComponent>();
            Assert.True(ac != null, "ac != null");
            
            Assert.True(actor.NumComponents == 5, "actor.NumComponents == 3");
            
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator TestComponentIntegrityWithNestedActors() {
            var actorObject = new GameObject("Test Actor");
            var nestedActorObject = new GameObject("Nested Actor");

            var actor = instance.GetWorld().SpawnActorOn<Actor>(actorObject);
            var nestedActor = instance.GetWorld().SpawnActorOn<Actor>(nestedActorObject);

            var ac = actor.AddGameComponent<ActorComponent>();
            Assert.True(ac != null, "ac != null");
            Assert.True(ac.Owner == actor, "ac.Owner == actor");
            
            var nac = nestedActor.AddGameComponent<ActorComponent>();
            Assert.True(nac != null, "nac != null");
            Assert.True(nac.Owner == nestedActor, "nac.Owner == nestedActor");

            var baseNestedObject = new GameObject("Actor Nest");
            baseNestedObject.transform.parent = actor.transform;
            ac = baseNestedObject.AddComponent<ActorComponent>();
            Assert.True(ac != null, "ac != null");
            Assert.True(ac.Owner == actor, "ac.Owner == actor");

            var nestedNestedObject = new GameObject("Nested Actor Nest");
            nestedNestedObject.transform.parent = nestedActor.transform;

            nac = nestedNestedObject.AddComponent<ActorComponent>();
            Assert.True(nac != null, "nac != null");
            Assert.True(nac.Owner == nestedActor, "nac.Owner == nestedActor");
            
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestDetachedActorIsDestroyed() {
            var go = new GameObject("Test Actor");
            var actor = instance.GetWorld().SpawnActorOn<Actor>(go);
            var sc = actor.AddGameComponent<ActorComponent>();
            sc.DetachFromActor();
            Assert.True(actor.NumComponents == 1, "actor.NumComponents == 1 (after scene component detached)");
            Object.Destroy(actor);
            yield return null;
            // we cannot assert.null here because, apparently, the "unity null" is not the null that the
            // assert is expecting (it failed, literally because of "expected null but got <null>" ...
            Assert.True(sc == null, "sc == null after actor destruction");
        }
        
        [UnityTest]
        public IEnumerator TestAddComponentOnEmptyObject() {
            var go = new GameObject("None Actor");
            var sc = go.AddComponent<ActorComponent>();
            // Assert.Throws<InvalidOperationException>(() => { sc = go.AddComponent<ActorComponent>(); });
            LogAssert.Expect(LogType.Exception, new Regex("requires an Actor component on the root object"));
            yield return null;
            Assert.True(sc == null, "sc == null after invalid component adding");
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
        public IEnumerator TestGetComponentFromDetachedObject() {
            var go = new GameObject("Test Actor");
            var actor = instance.GetWorld().SpawnActorOn<Actor>(go);
            var sc = actor.AddGameComponent<ActorComponent>();
            sc.DetachFromActor();
            actor.GetGameComponent<ActorComponent>();

            var fetchedSc = actor.GetGameComponent<ActorComponent>();
            
            Assert.True(sc == fetchedSc, "sc == fetchedSc");
            yield break;
        }
        
        [UnityTest]
        public IEnumerator TestTicking() {
            var go = new GameObject("Test Actor");
            var actor = instance.GetWorld().SpawnActorOn<Actor>(go);
            actor.SetEnableTick(true);
            // we assert in another test that the adding works
            var sc = actor.AddGameComponent<ActorComponent>();
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
