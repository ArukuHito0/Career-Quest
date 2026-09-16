using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using CareerQuest.Core;

namespace CareerQuest.Player
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-10)]
    //  íeä«óùÉNÉâÉX
    public sealed class BulletManager : MonoBehaviour
    {
        public static readonly List<IBullet> ActiveBullets = new List<IBullet>();

        readonly int _maxBullets = 2000; // ç≈ëÂíeêî
        NativeArray<BulletData> _bulletBuffer;

        public int ActiveCount => Mathf.Min(ActiveBullets.Count, _maxBullets);
        public NativeArray<BulletData> BulletBuffer => _bulletBuffer;

        void Awake()
        {
            _bulletBuffer = new NativeArray<BulletData>(_maxBullets, Allocator.Persistent);

            ServiceLocator.Register(this);
        }

        void Update()
        {
            int count = ActiveCount;

            for (int i = 0; i < count; i++)
            {
                var bullet = ActiveBullets[i];
               _bulletBuffer[i] = new BulletData
               {
                    Position = bullet.Position,
                    Radius = bullet.Tickness,
                    Damage = bullet.Damage,
                    IsActive = bullet.IsActive
                };
            }
        }

        void OnDestroy()
        {
            if (_bulletBuffer.IsCreated)
            {
                _bulletBuffer.Dispose();
            }

            ServiceLocator.Unregister(this);
        }
    }
}