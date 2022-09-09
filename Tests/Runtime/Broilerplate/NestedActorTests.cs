using System.Collections;
using Broilerplate.Core;
using Broilerplate.Core.Components;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.Runtime.Broilerplate {
    public class NestedActorTests {
        private GameInstance instance;
        
        [SetUp]
        public void Pretest() {
            instance = GameInstance.GetInstance();
        }
        
        [UnityTest]
        public IEnumerator TestSimpleActorNesting() {
            var rootActorGo = new GameObject("Test Actor");
            var childActorGo = new GameObject("Actor Child");
            childActorGo.transform.parent = rootActorGo.transform;

            var rootActor = instance.GetWorld().SpawnActorOn<Actor>(rootActorGo);
            var childActor = instance.GetWorld().SpawnActorOn<Actor>(childActorGo);
            
            Assert.True(rootActor != null, "rootActor != null");
            Assert.True(childActor != null, "childActor != null");
            
            Assert.True(rootActor.ParentActor == null, "rootActor.ParentActor == null");
            Assert.True(childActor.ParentActor == rootActor, "childActor.ParentActor == rootActor");
            
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator TestUpdatingComponentOwnerOnActorInsertion() {
            var rootActorGo = new GameObject("Test Actor");
            var childActorGo = new GameObject("Actor Child");
            childActorGo.transform.parent = rootActorGo.transform;

            

            var rootActor = instance.GetWorld().SpawnActorOn<Actor>(rootActorGo);
            Assert.True(rootActor != null, "rootActor != null");
            Assert.True(rootActor.ParentActor == null, "rootActor.ParentActor == null");
            
            var ac = childActorGo.AddComponent<ActorComponent>();
            ac.name = "nested component from root";
            Assert.True(ac.Owner == rootActor, "ac.Owner == rootActor");

            var childActor = instance.GetWorld().SpawnActorOn<Actor>(childActorGo);
            Assert.True(childActor.ParentActor == rootActor, "childActor.ParentActor == rootActor");
            Assert.True(ac.Owner == childActor, "ac.Owner == childActor");
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator TestParentActorUpdatingOnParentChange() {
            var rootActorGo = new GameObject("Test Actor");
            var childActorGo = new GameObject("Actor Child");
            childActorGo.transform.parent = rootActorGo.transform;

            

            var rootActor = instance.GetWorld().SpawnActorOn<Actor>(rootActorGo);
            var childActor = instance.GetWorld().SpawnActorOn<Actor>(childActorGo);
            Assert.True(rootActor.ParentActor == null, "rootActor.ParentActor == null");
            Assert.True(childActor.ParentActor == rootActor, "childActor.ParentActor == rootActor");
            childActor.transform.parent = null;
            Assert.True(childActor.ParentActor == null, "childActor.ParentActor == null");
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
    }
}