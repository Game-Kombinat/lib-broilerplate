using System;
using GameKombinat.Fnbt;
using UnityEngine;

namespace Broilerplate.Bt.Data {
    [CreateAssetMenu(menuName = "Game Kombinat/Create Data Context Asset", fileName = "New Data Context")]
    public class DataContext : ScriptableObject, ISerializationCallbackReceiver {
        // runtime data
        private NbtList dataRoot = new NbtList("dataList", NbtTagType.Compound);
        

        // backend data
        [SerializeField]
        [HideInInspector]
        private byte[] serializedNbt;

        public int GetInt(string name) {
            NbtTag tag = Find(name);
            if (tag == null) {
                return 0;
            }

            try {
                return tag["value"].IntValue;
            }
            catch (NullReferenceException) {
                Debug.LogWarning($"Trying to use tag {name} which has no value assigned to it yet");
                return 0;
            }
            catch {
                Debug.LogError($"{name} is not attached to an int value. Tag is {tag["value"].GetType()}");
                return 0;
            }
        }
        
        public float GetFloat(string name) {
            NbtTag tag = Find(name);
            if (tag == null) {
                return 0f;
            }

            try {
                return tag["value"].FloatValue;
            }
            catch (NullReferenceException) {
                Debug.LogWarning($"Trying to use tag {name} which has no value assigned to it yet");
                return 0f;
            }
            catch {
                Debug.LogError($"{name} is not attached to a float value. Tag is {tag["value"].GetType()}");
                return 0f;
            }
        }
        
        public bool GetBool(string name) {
            NbtTag tag = Find(name);
            if (tag == null) {
                return false;
            }
            try {
                return tag["value"].ByteValue > 0;
            }
            catch (NullReferenceException) {
                Debug.LogWarning($"Trying to use tag {name} which has no value assigned to it yet");
                return false;
            }
            catch {
                Debug.LogError($"{name} is not attached to a byte value that can be converted to bool. Tag is {tag["value"].GetType()}");
                return false;
            }
        }
        
        public string GetString(string name) {
            NbtTag tag = Find(name);
            if (tag == null) {
                return string.Empty;
            }
            try {
                return tag["value"].StringValue;
            }
            catch (NullReferenceException) {
                Debug.LogWarning($"Trying to use tag {name} which has no value assigned to it yet");
                return string.Empty;
            }
            catch {
                Debug.LogError($"{name} is not attached to a string value. Tag is {tag["value"].GetType()}");
                return string.Empty;
            }
        }

        public void Set(string name, int val) {
            NbtTag tag = FindOrCreate(name, out bool wasCreated);
            if (wasCreated || tag["value"] == null) {
                tag["value"] = new NbtInt("value", val);
            }
            else {
                
                ((NbtInt)tag["value"]).Value = val;
            }
        }
        
        public void Set(string name, float val) {
            NbtTag tag = FindOrCreate(name, out bool wasCreated);
            if (wasCreated || tag["value"] == null) {
                tag["value"] = new NbtFloat("value", val);
            }
            else {
                ((NbtFloat)tag["value"]).Value = val;
            }
        }
        
        public void Set(string name, string val) {
            NbtTag tag = FindOrCreate(name, out bool wasCreated);
            if (wasCreated || tag["value"] == null) {
                tag["value"] = new NbtString("value", val);
            }
            else {
                ((NbtString)tag["value"]).Value = val;
            }
        }
        
        public void Set(string name, bool val) {
            NbtTag tag = FindOrCreate(name, out bool wasCreated);
            byte byteVal = (byte)(val ? 1 : 0);
            if (wasCreated || tag["value"] == null) {
                tag["value"] = new NbtByte("value", byteVal);
            }
            else {
                ((NbtByte)tag["value"]).Value = byteVal;
            }
        }

        public bool HasAnyTagByName(string name) {
            return Find(name) != null;
        }

        private NbtTag FindOrCreate(string tagName, out bool wasCreated) {
            NbtTag tag = Find(tagName);
            if (tag == null) {
                wasCreated = true;
                var comp = new NbtCompound();
                comp.Add(new NbtString("tagName", tagName));
                dataRoot.Add(comp);
                return comp;
            }

            wasCreated = false;
            return tag;
        }

        public NbtTag Find(string tagName) {
            for (int i = 0; i < dataRoot.Count; ++i) {
                var t = dataRoot[i];
                if (t["tagName"].StringValue == tagName) {
                    return t;
                }
            }

            return null;
        }

        #region editortime serialisation
        public void OnBeforeSerialize() {
            if (dataRoot.Parent != null) {
                serializedNbt = NbtSerializerService.Serialize((NbtCompound)dataRoot.Parent);
            }
            else {
                var root = new NbtCompound("root");
                root.Add(dataRoot);
                serializedNbt = NbtSerializerService.Serialize(root);
            }
            
        }

        public void OnAfterDeserialize() {
            var compound =  NbtSerializerService.Deserialize(serializedNbt);
            dataRoot = (NbtList)compound["dataList"];
        }

        public NbtList DataList => dataRoot;
        #endregion

        #region runtime serialisation
        public NbtCompound GetSerializedData() {
//            // This happens when loaded from a savegame
//            if (dataRoot == null) {
//                
//            }
            if (dataRoot.Parent != null) {
                return (NbtCompound)dataRoot.Parent;
            }
            var root = new NbtCompound("root");
            root.Add(dataRoot);
            return root;
        }

        public void SetSerializedData(NbtCompound data) {
            dataRoot = (NbtList)data["dataList"];
        }
        #endregion
    }
}