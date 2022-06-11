using UnityEngine;

namespace Broilerplate.Tools {
    public abstract class PoolingPostProcessor : MonoBehaviour {

        public abstract void PostProcessOnSpawn(GameObject instance);
        public abstract void PostProcessOnGet(GameObject instance);
    }
}