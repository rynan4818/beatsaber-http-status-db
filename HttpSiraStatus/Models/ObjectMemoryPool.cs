using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace HttpSiraStatus.Models
{
    public class ObjectMemoryPool<T> where T : new ()
    {
        private readonly ConcurrentBag<T> _objects = new ConcurrentBag<T>();
        private readonly Action<T> ReInitialize;
        private readonly Action<T> Initialize;
        public T Spawn()
        {
            if (this._objects.TryTake(out var result)) {
                this.ReInitialize?.Invoke(result);
                return result;
            }
            else {
                result = new T();
                this.Initialize?.Invoke(result);
                this.ReInitialize?.Invoke(result);
                return result;
            }
        }

        public void Despawn(T item)
        {
            this._objects.Add(item);
        }

        public ObjectMemoryPool(Action<T> init, Action<T> reInit, int size)
        {
            this.ReInitialize = reInit;
            this.Initialize = init;
            for (var i = 0; i < size; i++) {
                var item = new T();
                this.Initialize?.Invoke(item);
                this._objects.Add(item);
            }
        }
    }
}
