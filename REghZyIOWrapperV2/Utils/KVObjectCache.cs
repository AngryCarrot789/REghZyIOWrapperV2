using System;
using REghZyIOWrapperV2.Packeting;

namespace REghZyIOWrapperV2.Utils {
    public class KVObjectCache<K, V> {
        private K lastAccessedKey;
        private V lastAccessedValue;

        private readonly Func<K, V> getValue;

        public Func<K, V> GetValue { get => this.getValue; }

        public KVObjectCache(Func<K, V> getValue) {
            if (getValue == null) {
                throw new NullReferenceException("getValue cannot be null");
            }

            this.getValue = getValue;
        }

        public V Get(K key) {
            if (key == null) {
                this.lastAccessedKey = key;
                return this.lastAccessedValue = this.getValue(key);
            }
            else if (key.Equals(this.lastAccessedKey)) {
                this.lastAccessedKey = key;
                return this.lastAccessedValue;
            }
            else {
                this.lastAccessedKey = key;
                return this.lastAccessedValue = this.getValue(key);
            }
        }
    }
}
