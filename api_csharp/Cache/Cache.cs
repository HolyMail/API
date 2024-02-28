using MailKit.Net.Imap;

namespace api_csharp.Cache;

public sealed class Cache<TKey, TValue> where TKey : notnull
{
        private readonly Dictionary<TKey, TValue> _dictionary;
        private readonly Queue<TKey> _keys;
        private readonly int _capacity;

        public Cache(int capacity)
        {
            _keys = new Queue<TKey>(capacity);
            _capacity = capacity;
            _dictionary = new Dictionary<TKey, TValue>(capacity);
        }

        public TValue? Add(TKey key, TValue value)
        {
            TValue? removedValue = default;
            
            if (_dictionary.Count == _capacity)
            {
                var oldestKey = _keys.Dequeue();
                removedValue = _dictionary[oldestKey];
                _dictionary.Remove(oldestKey);
            }

            if (_dictionary.ContainsKey(key))
            {
                _dictionary[key] = value;
            }
            
            _dictionary.TryAdd(key, value);
            _keys.Enqueue(key);

            return removedValue;
        }

        public TValue? Get(TKey key) => !_dictionary.TryGetValue(key, out var value) ? default : value;
}

public static class Cache 
{
    public static Cache<TKey, TValue> Create<TKey, TValue>(int capacity) where TKey : notnull => new(capacity);
}