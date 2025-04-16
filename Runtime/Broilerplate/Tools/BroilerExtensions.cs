namespace Broilerplate.Tools {
    /// <summary>
    /// These are misc extensions for this and that use.
    /// </summary>
    public static class BroilerExtensions {
        /// <summary>
        /// Fowler-Noll-Vo Hash function.
        /// Good for dictionary key hashes.
        /// https://en.wikipedia.org/wiki/Fowler–Noll–Vo_hash_function
        ///
        /// I don't pretend I understand this very much but that's how the article says to implement it.
        /// And monkey sees, monkey does.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static int GetFnv1aHash(this string input) {
            uint hash = 2166136261; // 32 bit offset basis
            for (int i = 0; i < input.Length; i++) {
                var c = input[i];
                hash = (hash ^ c) * 16777619; // 32 bit FNV prime
            }

            return unchecked((int)hash);
        }
    }
}