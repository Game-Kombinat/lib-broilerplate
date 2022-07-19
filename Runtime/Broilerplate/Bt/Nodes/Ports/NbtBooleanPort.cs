using System;
using UnityEngine;

namespace Broilerplate.Bt.Nodes.Ports {
    [Serializable]
    public class NbtBooleanPort : NbtPort {
        [SerializeField]
        public bool @default;
    }
}