using System;
using System.Collections.Concurrent;

namespace HttpSiraStatus.Models
{
    public class ObjectMemoryPool<T> where T : new()
    {
        private readonly ConcurrentBag<T> _objects = new ConcurrentBag<T>();
        private readonly Action<T> _reInitialize;
        private readonly Action<T> _initialize;
        public T Spawn()
        {
            if (this._objects.TryTake(out var result)) {
                this._reInitialize?.Invoke(result);
                return result;
            }
            else {
                result = new T();
                this._initialize?.Invoke(result);
                this._reInitialize?.Invoke(result);
                return result;
            }
        }

        public void Despawn(T item)
        {
            this._objects.Add(item);
        }

        public ObjectMemoryPool(Action<T> init, Action<T> reInit, int size)
        {
            this._reInitialize = reInit;
            this._initialize = init;
            for (var i = 0; i < size; i++) {
                var item = new T();
                this._initialize?.Invoke(item);
                this._objects.Add(item);
            }
        }
    }
}
