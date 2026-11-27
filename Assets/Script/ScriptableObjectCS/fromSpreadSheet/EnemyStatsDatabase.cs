using UnityEngine;
using System.Collections.Generic;
using CareerQuest.Core;

namespace CareerQuest.Enemy
{
    public sealed class EnemyStatsDatabase : SpreadSheetSO
    {
        [SerializeField] List<EnemyStat> _enemyStatList;

        public EnemyStat GetStat(EnemyID id)
        {
            if ((_enemyStatList?.Count ?? 0) == 0)
            {
                MyLogger.WarningLog("EnemyStats is odd");
                return null;
            }

            return _enemyStatList.Find(stat => stat.ID == id);
        }
    }
}