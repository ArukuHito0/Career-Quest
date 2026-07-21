using CareerQuest.Core;
using CareerQuest.Enemy;
using System.Collections.Generic;
using UnityEngine;

public class Test_Treasuer: MonoBehaviour, ISpatialEntity
{
    TreasureHashManager _hashManager;
    public List<int> nearbyEntities = new List<int>(64);


    public int Index { get; set; }  // この宝物の番号(一意)
    public float Tickness { get; set; }  // オブジェクトの厚さ

    void Awake()
    {
        _hashManager = ServiceLocator.Resolve<TreasureHashManager>();
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
            if (_hashManager.Entities[index] == this)
            {
                continue;
            }
            var otherEnemy = _hashManager.Entities[index];

            float dist = Vector3.Distance(transform.position, otherEnemy.transform.position);
            if (dist < 20.0f)
            {
            }
        }
    }
}