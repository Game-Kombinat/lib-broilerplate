using System;

namespace Broilerplate.Data {
    public class ColumnAttribute : Attribute {
        public int sort;
        public string displayName;
        public bool readOnly;
    }
}