using System;
using System.Collections.Generic;
using UnityEngine;

namespace Broilerplate.Bt.Nodes {
    public abstract class NodeWithChildren<T> : BaseNode where T : class {
        [Output(dynamicPortList = true)]
        [SerializeField] // funny enough, even though this is a public field, it needs this attrib to save it.
        public T[] children = Array.Empty<T>();
        
        protected readonly List<BaseNode> childNodes = new List<BaseNode>();

        protected override void InternalSpawn() {
            int numChildren = children.Length;
            for (int i = 0; i < numChildren; ++i) {
                var port = GetOutputPort(nameof(children) + " " + i); // very nasty but xnode does it like this.
                if (port.IsConnected) {
                    int connections = port.ConnectionCount;
                    for (int j = 0; j < connections; ++j) {
                        var p = port.GetConnection(j);
                        if (p?.node is BaseNode n) {
                            childNodes.Add(n);
                        }
                    }
                }
            }
        }
    }
}