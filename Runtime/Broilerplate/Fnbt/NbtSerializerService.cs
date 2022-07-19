using System;
using System.IO;
using GameKombinat.Fnbt;

namespace GameKombinat.Fnbt {
    public static class NbtSerializerService {

        public static byte[] Serialize(NbtCompound root) {
            using (MemoryStream ms = new MemoryStream()) {
                NbtFile f = new NbtFile(root);
                f.SaveToStream(ms, NbtCompression.None);
                ms.Position = 0;
                return ms.ToArray();
            }
        }
        
        public static NbtCompound Deserialize(byte[] raw) {
            NbtFile f = new NbtFile();
            f.LoadFromBuffer(raw, 0, raw.Length, NbtCompression.None);
            return f.RootTag;
        }
        
        public static byte[] SerializeInteger(int val) {
            return BitConverter.GetBytes(val);
        }
        
        public static int DeserializeInteger(byte[] val) {
            return BitConverter.ToInt32(val, 0);
        }
    }
}