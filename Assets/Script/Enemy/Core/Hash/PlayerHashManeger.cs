using UnityEngine;
using CareerQuest.Core;

namespace CareerQuest.Enemy
{
    //  お宝の位置をグリッドマップで把握するクラス
    [DefaultExecutionOrder(-10)]
    public sealed class PlayerHashManager : SpatialHashManagerBase<PlayerMove>
    {
        void Awake()
        {
            // ServiceLocatorに登録(必須)
            ServiceLocator.Register(this);
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // // ServiceLocatorから登録解除(必須)
            ServiceLocator.Unregister(this);
        }
    }
}