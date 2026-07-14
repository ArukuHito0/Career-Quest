using UnityEngine;
using System.Collections.Generic;

namespace CareerQuest.Enemy
{
    //  敵の能力値集約クラス
    [CreateAssetMenu(fileName = "EnemyStats", menuName = "Character/Enemy/Stats")]
    public sealed class EnemyStatHolder : ScriptableObject, ISerializationCallbackReceiver
    {
        //  敵のIDと能力値のクラス
        [System.Serializable]
        public sealed class EnemyIDStat
        {
            public EnemyID ID;  // ID
            public EnemyStat Stat;  // 能力値
        }

        [Header("能力値リスト")]
        [SerializeField] List<EnemyIDStat> _idStatList = new();  //  IDと能力値のリスト(インスペクター用)
        Dictionary<EnemyID, EnemyStat> _statMap = new();  //  IDと能力値の辞書(処理用)

        public void OnAfterDeserialize()
        {
            //  能力値辞書の構築
            BuildStatMap();
        }
        public void OnBeforeSerialize() { }

        //  能力値の取得
        public EnemyStat GetStat(EnemyID enemyID)
        {
            return _statMap.TryGetValue(enemyID, out EnemyStat stat) ? stat : null;
        }
        
        //  能力値辞書の構築
        void BuildStatMap()
        {
            _statMap.Clear();
            foreach (var pair in _idStatList)
            {
                if (!_statMap.ContainsKey(pair.ID))
                {
                    _statMap.Add(pair.ID, pair.Stat);
                }
            }
        }
    }
}