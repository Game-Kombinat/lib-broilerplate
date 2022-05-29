using System;
using UnityEngine;

namespace Broilerplate.Data {
    /// <summary>
    /// Base type for data structures that can be displayed in a data table.
    /// </summary>
    public class RowData : ScriptableObject  {
        [Column(sort = -1000, readOnly = true)]
        public int id;
        [Column(sort = -999)]
        public string machineName;

        /// <summary>
        /// Used to facilitate searching in the data table editor.
        /// This is a transient field that is created on demand
        /// and updated every time any field info in a RowData changes.
        /// </summary>
        [NonSerialized]
        private string searchBlob;

        private void OnValidate() {
            name = machineName;
            searchBlob = JsonUtility.ToJson(this);
        }

        public bool Search(string substr) {
            if (string.IsNullOrWhiteSpace(substr)) {
                return true;
            }
            
            if (string.IsNullOrEmpty(searchBlob)) {
                searchBlob = JsonUtility.ToJson(this).ToLower();
            }

            return searchBlob.Contains(substr);
        }
    }
}