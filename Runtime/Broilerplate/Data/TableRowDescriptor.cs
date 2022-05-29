using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Broilerplate.Data {
    public class ColumnDescriptor {
        public string propertyName;
        public string displayName;
        public bool readOnly;
        public FieldInfo field;
    }
    
    /// <summary>
    /// Basically caches reflection info of a data table row type
    /// </summary>
    public class TableRowDescriptor {
#if UNITY_EDITOR
        public List<FieldInfo> Columns { get; }
        public List<ColumnDescriptor> ColumnInfo { get; }

        public Type RowType { get; }

        public TableRowDescriptor(Type typeInfo) {
            var fields = typeInfo.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance)
                                 .Where(x => x.GetCustomAttribute<HideInInspector>() == null);
            Columns = new List<FieldInfo>(fields);
            Columns.Sort(SortByAttribute);

            // need reversal because it will fetch the objects in the order of appearance
            // going down in the inheritance (so id and machine name last, where they should be first)
            ColumnInfo = Columns.Select(ExtractColumnInfo).ToList();

            RowType = typeInfo;
        }

        private static int SortByAttribute(FieldInfo a, FieldInfo b) {
            var colA = a.GetCustomAttribute<ColumnAttribute>() ?? new ColumnAttribute() { sort = 0 };
            var colB = b.GetCustomAttribute<ColumnAttribute>() ?? new ColumnAttribute() { sort = 0 };
            return colA.sort.CompareTo(colB.sort);
        }

        private static ColumnDescriptor ExtractColumnInfo(FieldInfo a) {
            var desc = new ColumnDescriptor();
            
            var colA = a.GetCustomAttribute<ColumnAttribute>();
            bool hasAttribute = colA != null;
            desc.displayName = hasAttribute && !string.IsNullOrEmpty(colA.displayName) ? colA.displayName : ObjectNames.NicifyVariableName(a.Name);
            desc.propertyName = a.Name;
            desc.readOnly = hasAttribute && colA.readOnly;
            desc.field = a;
            return desc;

        }
#endif
    }
}
