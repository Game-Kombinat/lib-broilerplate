using System;
using Broilerplate.Bt.Nodes.Data.Graph;
using GameKombinat.ControlFlow.Bt.Data;
using UnityEngine.SceneManagement;

namespace Broilerplate.Editor.Broilerplate.Bt {
    public class DataContextNameListProvider {
        private DataContext data;

        private string[] nameList;
        
        public string[] NameList => nameList;

        public DataContext Data => data;

        private int lastHash;
        private int lastSize;
        private ContextDataAccessNode referenceNode;

        public DataContextNameListProvider(ContextDataAccessNode referenceNode) {
            this.referenceNode = referenceNode;
            SwitchScope(referenceNode.DataContextScope); 
            UpdateNameList();
        }

        public void UpdateNameList(bool force = false) {
            if (data == null) {
                nameList = new string[0];
                return;
            }
            if (lastHash != data.DataList.GetHashCode() || lastSize != data.DataList.Count || force) {
                lastHash = data.DataList.GetHashCode();
                lastSize = data.DataList.Count;
                nameList = new string[data.DataList.Count];
                for (int i = 0; i < data.DataList.Count; ++i) {
                    nameList[i] = data.DataList[i]["tagName"].StringValue;
                }

            }
            
        }

        public int GetIndex(string name) {
            return Array.IndexOf(nameList, name);
        }

        /// <summary>
        /// This checks the situation in which a variable might have been renamed.
        /// Since the actual value types don't matter in this context, it will suffice if,
        /// at the given variable index, we now have a different name, if the old name vanished.
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="index"></param>
        /// <param name="expectedTagType"></param>
        /// <returns></returns>
        public bool CheckForVariableRename(ref string oldName, int index) {
            if (index < 0 || index >= nameList.Length) {
                return false;
            }

            // All is the same. Lets get out.
            if (data.HasAnyTagByName(oldName) && GetIndex(oldName) == index) {
                return false;
            }
            // Check if we get a new name at the old index
            string name = nameList[index];
            var tag = data.Find(name);
            if (tag == null) {
                // variable was deleted or does not exist yet. Might cover that too but not here.
                // Usually the user code has it already accounted for
                return false;
            }
            
            if (tag["tagName"].StringValue != oldName) {
                oldName = tag["tagName"].StringValue;
                return true;
            }

            return false;
        }

        public void SwitchScope(DataContextScope dataScope) {
            switch (dataScope) {
                case DataContextScope.Game:
                    data = DataContextRegistry.Instance.GetGlobalContext();
                    UpdateNameList(true);
                    break;
                case DataContextScope.Scene:
                    data = DataContextRegistry.Instance.GetSceneDataContext(SceneManager.GetActiveScene().name);
                    UpdateNameList(true);
                    break;
                case DataContextScope.Graph:
                    data = referenceNode.Tree.Data;
                    UpdateNameList(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataScope), $"{dataScope} is unaccounted for. New?");
            }
        }
    }
}