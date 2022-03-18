using System.Collections;
using Broilerplate.Core;
using Broilerplate.Ticking;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Broilerplate.Tests.PlaymodeTests {
    public class GameInstanceTests {

        private GameInstance instance;
        
        [SetUp]
        public void Pretest() {
            instance = GameInstance.GetInstance();
        }
        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator CheckInstanceIsBootstrapped()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            
            // did the game instance spawn?
            Assert.NotNull(instance, "GameInstance.GetInstance() != null");
            // wait for level to be loaded
            // during testing this call doesn't happen
            // did the game instance spawn a world?
            Assert.NotNull(instance.GetWorld(), "GameInstance.GetInstance().GetWorld() != null");
            // has the ticker object been created?
            Assert.NotNull(Object.FindObjectsOfType<UnityTicker>(), "Object.FindObjectsOfType<UnityTicker>() != null");
            yield return null;
            
        }
    }
}