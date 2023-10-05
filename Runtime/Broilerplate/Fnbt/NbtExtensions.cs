namespace GameKombinat.Fnbt {
    public static class NbtExtensions {
        public static void SetIntTag(this NbtCompound compound, string tagName, int tagValue) {
            if (compound.TryGet(tagName, out NbtInt t)) {
                t.Value = tagValue;
            }
            else {
                compound.Add(new NbtInt(tagName, tagValue));
            }
        }
        
        public static void SetBoolTag(this NbtCompound compound, string tagName, float tagValue) {
            if (compound.TryGet(tagName, out NbtFloat t)) {
                t.Value = tagValue;
            }
            else {
                compound.Add(new NbtFloat(tagName, tagValue));
            }
        }
        
        public static void SetStringTag(this NbtCompound compound, string tagName, string tagValue) {
            if (tagValue == null) {
                compound.Remove(tagName);
            }
            else if (compound.TryGet(tagName, out NbtString t)) {
                t.Value = tagValue;
            }
            else {
                compound.Add(new NbtString(tagName, tagValue));
            }
        }
        
        public static void SetLongTag(this NbtCompound compound, string tagName, long tagValue) {
            if (compound.TryGet(tagName, out NbtLong t)) {
                t.Value = tagValue;
            }
            else {
                compound.Add(new NbtLong(tagName, tagValue));
            }
        }
    }
}