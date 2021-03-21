using System;
using System.Collections;
using System.Collections.Generic;

namespace BoxVRPlaylistManagerNETCore.FitXr
{
    [Serializable]
    public class ReadOnlyDictionary<TKey, TValue> :
      IDictionary<TKey, TValue>,
      ICollection<KeyValuePair<TKey, TValue>>,
      IEnumerable<KeyValuePair<TKey, TValue>>,
      IEnumerable
    {
        private readonly IDictionary<TKey, TValue> _dictionary;

        public ReadOnlyDictionary() => this._dictionary = (IDictionary<TKey, TValue>)new Dictionary<TKey, TValue>();

        public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionary) => this._dictionary = dictionary;

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value) => throw ReadOnlyDictionary<TKey, TValue>.ReadOnlyException();

        public bool ContainsKey(TKey key) => this._dictionary.ContainsKey(key);

        public ICollection<TKey> Keys => this._dictionary.Keys;

        bool IDictionary<TKey, TValue>.Remove(TKey key) => throw ReadOnlyDictionary<TKey, TValue>.ReadOnlyException();

        public bool TryGetValue(TKey key, out TValue value) => this._dictionary.TryGetValue(key, out value);

        public ICollection<TValue> Values => this._dictionary.Values;

        public TValue this[TKey key] => this._dictionary[key];

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get => this[key];
            set => throw ReadOnlyDictionary<TKey, TValue>.ReadOnlyException();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(
          KeyValuePair<TKey, TValue> item)
        {
            throw ReadOnlyDictionary<TKey, TValue>.ReadOnlyException();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear() => throw ReadOnlyDictionary<TKey, TValue>.ReadOnlyException();

        public bool Contains(KeyValuePair<TKey, TValue> item) => this._dictionary.Contains(item);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => this._dictionary.CopyTo(array, arrayIndex);

        public int Count => this._dictionary.Count;

        public bool IsReadOnly => true;

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(
          KeyValuePair<TKey, TValue> item)
        {
            throw ReadOnlyDictionary<TKey, TValue>.ReadOnlyException();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => this._dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => (IEnumerator)this.GetEnumerator();

        private static Exception ReadOnlyException() => (Exception)new NotSupportedException("This dictionary is read-only");
    }
}
