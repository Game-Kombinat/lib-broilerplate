using System;
using Broilerplate.Bt.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Broilerplate.Bt.Nodes.Data.Graph {
    /// <summary>
    /// Base class node for more nodes accessing a DataContext.
    /// Contains information required for editor and runtime
    /// to get and store data correctly.
    /// </summary>
    public abstract class ContextDataAccessNode : BaseNode {
        
        #if UNITY_EDITOR
        // Used within the editor code to detect renames
        [SerializeField]
        [HideInInspector]
        private int nameListIndex = 0;
        #endif

        /// <summary>
        /// Used to determine where the value node should be sourced from.
        /// Needed in editor as well as within the nodes themselves.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private DataContextScope dataContextScope = DataContextScope.Game;
        public DataContextScope DataContextScope => dataContextScope;

        public string varName;

        /// <summary>
        /// Returns the data context for the selected scope.
        /// Determined by dataContextScope
        /// </summary>
        /// <returns></returns>
        public DataContext GetDataContextFromScope() {
            switch (dataContextScope) {
                case DataContextScope.Game:
                    return DataContextRegistry.Instance.GetGlobalContext();
                case DataContextScope.Scene:
                    return DataContextRegistry.Instance.GetSceneDataContext(SceneManager.GetActiveScene().name);
                case DataContextScope.Graph:
                    return Tree.Data;
                default:
                    throw new ArgumentOutOfRangeException($"{dataContextScope} scope type is unaccounted for. New?");
            }
        }
    }
}