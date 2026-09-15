using CareerQuest.Core;
using CareerQuest.Enemy;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMove : MonoBehaviour, ISpatialEntity
{
    //  プレイヤーのNavMeshAgent
    private NavMeshAgent agent;

    PlayerHashManager _hashManager;
    public List<int> nearbyEntities = new List<int>(64);


    public int Index { get; set; }  // この宝物の番号(一意)
    public float Tickness { get; set; }  // オブジェクトの厚さ


    private void Awake()
    {
        // 自分についているNavMeshAgentを取得
        agent = GetComponent<NavMeshAgent>();
        _hashManager = ServiceLocator.Resolve<PlayerHashManager>();
        _hashManager.Register(this);
        Tickness = 0.2f;
    }

    void Update()
    {
        nearbyEntities.Clear();

        int myX = Mathf.FloorToInt(transform.position.x / _hashManager.cellSize);
        int myZ = Mathf.FloorToInt(transform.position.z / _hashManager.cellSize);
        int myCellId = myX + (myZ * _hashManager.girdWidth);

        for (int dz = -1; dz <= 1; dz++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                int targetCellId = (myX + dx) + ((myZ + dz) * _hashManager.girdWidth);

                _hashManager.GetEntitiesInCell(targetCellId, nearbyEntities);
            }
        }

        foreach (int index in nearbyEntities)
        {
            if (_hashManager.ActiveEntities[index] == this)
            {
                continue;
            }
            var otherEnemy = _hashManager.ActiveEntities[index];

            float dist = Vector3.Distance(transform.position, otherEnemy.transform.position);
            if (dist < 20.0f)
            {
            }
        }
    }

    // 指定した座標へ移動する
    public void MoveTo(Vector3 position)
    {
        // 運搬中などでAgentが無効なら何もしない
        if (!agent.enabled)
            return;

        // NavMesh上にいない場合も移動できない
        if (!agent.isOnNavMesh)
            return;

        // 目的地を設定
        agent.SetDestination(position);
    }
}