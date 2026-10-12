using System;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace RoadTurret.Common
{
    /// <summary>
    /// Pool of prefab instances. Instances are created through the container, so their [Inject] methods are called.
    /// </summary>
    public sealed class PrefabPool<T> : IDisposable where T : Component
    {
        private readonly ObjectPool<T> _pool;

        public PrefabPool(IObjectResolver resolver, T prefab, Transform root, int prewarmCount = 0)
        {
            _pool = new ObjectPool<T>(
                createFunc: () => resolver.Instantiate(prefab, root),
                actionOnGet: item => item.gameObject.SetActive(true),
                actionOnRelease: item => item.gameObject.SetActive(false),
                actionOnDestroy: item =>
                {
                    if (item != null)
                        Object.Destroy(item.gameObject);
                },
                collectionCheck: false,
                defaultCapacity: Mathf.Max(prewarmCount, 1));

            Prewarm(prewarmCount);
        }

        public T Get() => _pool.Get();

        public void Release(T item) => _pool.Release(item);

        public void Dispose() => _pool.Clear();

        private void Prewarm(int count)
        {
            var items = new T[count];

            for (int i = 0; i < count; i++)
                items[i] = _pool.Get();

            foreach (T item in items)
                _pool.Release(item);
        }
    }
}
