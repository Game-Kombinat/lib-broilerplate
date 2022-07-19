using System;
using UnityEngine;

namespace Broilerplate.Bt.Nodes.Ports {
    /// <summary>
    /// Empty thing to represent a port on a behaviour tree node.
    /// </summary>
    [Serializable]
    public class Port {
        [SerializeField]
        [HideInInspector]
        public byte dummyValue;
        public static Port portValue = null;
    }
}