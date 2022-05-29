using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Broilerplate.Data {
    // Exists to do some editor stuffs cause in editor we don't know the generic type.
    public interface IDataTable {
        void AddRow();

        void RemoveRow(int index);
        
        void RemoveRow(RowData row);

        void Reset();

        List<RowData> GetRows();

        RowData GetRow(int index);

        List<ColumnDescriptor> GetColumnInfo();
    }

    public abstract class DataTable<TRowType> : ScriptableObject, IDataTable where TRowType : RowData {
        [SerializeField, HideInInspector] private List<TRowType> rows;

        [SerializeField, HideInInspector] private int autoIncrementIndex;

#if UNITY_EDITOR
        private TableRowDescriptor descriptor;
#endif

        private void Awake() {
            Reset();
        }

        public void RemoveRow(RowData row) {
            int idx = rows.IndexOf((TRowType)row);
            if (idx >= 0) {
                RemoveRow(idx);
            }
        }

        public void Reset() {
            if (rows == null) {
                rows = new List<TRowType>();
                autoIncrementIndex = 1;
            }

#if UNITY_EDITOR
            if (descriptor == null) {
                descriptor = new TableRowDescriptor(typeof(TRowType));
            }
#endif
        }

        public void AddRow() {
#if UNITY_EDITOR
            var inst = CreateInstance<TRowType>();
            inst.id = autoIncrementIndex++;
            AssetDatabase.AddObjectToAsset(inst, this);
            rows.Add(inst);
#endif
        }

        public void RemoveRow(int index) {
#if UNITY_EDITOR
            var inst = rows[index];
            rows.RemoveAt(index);
            AssetDatabase.RemoveObjectFromAsset(inst);
#endif
        }

        // non-generic call that will work on the editor window
        public List<RowData> GetRows() {
            // returns a copy only because we shall not touch the original list
            // form the outside in order to keep the data sane.
            return new List<RowData>(rows);
        }

        // generic call to save the casting at runtime.
        public List<TRowType> GetRowsTyped() {
            return new List<TRowType>(rows);
        }

        // non-generic call that will work on the editor window
        public RowData GetRow(int index) {
            return rows[index];
        }
        
        // generic call to save the casting at runtime
        public TRowType GetRowTyped(int index) {
            return rows[index];
        }

        // Editor only!
        public List<ColumnDescriptor> GetColumnInfo() {
#if UNITY_EDITOR
            return descriptor.ColumnInfo;
#else
            return null;
#endif
        }
    }
}