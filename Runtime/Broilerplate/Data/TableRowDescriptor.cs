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
        public List<ColumnDescriptor> ColumnInfo { get; }

        public TableRowDescriptor(Type typeInfo) {
            var columns = new List<FieldInfo>();
            var fields = typeInfo.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                 .Where(FieldIsSerializable);
            
            columns.AddRange(fields);
            // By a (questionable) design decision, GetFields will not return private fields from parent classes.
            // So we iterate through the hierarchy and return the private fields for all the parent classes.
            Type current = typeInfo.BaseType;
            while (current != null) {
                // only private fields here, we already got the public and protected ones.
                fields = current.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                                    .Where(FieldIsSerializable);
            
                columns.AddRange(fields);
                current = current.BaseType;
            }
            
            columns.Sort(SortByAttribute);

            // need reversal because it will fetch the objects in the order of appearance
            // going down in the inheritance (so id and machine name last, where they should be first)
            ColumnInfo = columns.Select(ExtractColumnInfo).ToList();
        }

        private static bool FieldIsSerializable(FieldInfo x) {
            return x.GetCustomAttribute<HideInInspector>() == null && (!x.IsPrivate || x.GetCustomAttribute<SerializeField>() != null);
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
