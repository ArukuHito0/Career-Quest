using UnityEngine;
using CareerQuest.Core;

namespace CareerQuest.Enemy
{
    //  “G‚ÌˆÊ’u‚ğƒ}ƒbƒvƒZƒ‹‚Å”cˆ¬‚·‚éƒNƒ‰ƒX
    [DefaultExecutionOrder(-10)]
    public sealed class EnemyHashManager: SpatialHashManagerBase<EnemyContoroller>
    {
        void Awake()
        {
            // ServiceLocator‚É“o˜^(•K{)
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

            // // ServiceLocator‚©‚ç“o˜^‰ğœ(•K{)
            ServiceLocator.Unregister(this);
        }
    }
}