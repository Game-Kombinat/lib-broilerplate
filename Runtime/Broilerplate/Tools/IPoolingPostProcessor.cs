using UnityEngine;

namespace Broilerplate.Tools {
    public interface IPoolingPostProcessor<T> {
        void PostProcessOnSpawn(T instance);
        void PostProcessOnGet(T instance);
    }
}