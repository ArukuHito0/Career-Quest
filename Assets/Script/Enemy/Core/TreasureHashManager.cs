using UnityEngine;
using CareerQuest.Core;

namespace CareerQuest.Enemy
{
    //  ‚¨•ó‚ÌˆÊ’u‚ğƒ}ƒbƒvƒZƒ‹‚Å”cˆ¬‚·‚éƒNƒ‰ƒX
    [DefaultExecutionOrder(-10)]
    public sealed class TreasureHashManager : SpatialHashManagerBase<Test_Treasuer>
    {
        void Awake()
        {
            // ServiceLocator‚É“o˜^(•K{)
            ServiceLocator.Register(this);
        }

        protected override void Update()
        {
            base.Update();
        }

        void OnDestroy()
        {
            // // ServiceLocator‚©‚ç“o˜^‰ğœ(•K{)
            ServiceLocator.Unregister(this);
        }
    }
}