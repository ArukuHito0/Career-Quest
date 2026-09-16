using CareerQuest.Core;
using CareerQuest.Enemy;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMove : MonoBehaviour, ISpatialEntity
{
    //  �v���C���[��NavMeshAgent
    private NavMeshAgent agent;

    PlayerHashManager _hashManager;
    public List<int> nearbyEntities = new List<int>(64);


    public int Index { get; set; }  // ���̕󕨂̔ԍ�(���)
    public float Tickness { get; set; }  // �I�u�W�F�N�g�̌���

    private PlayerStateManager stateManager;

    private void Awake()
    {
        // �����ɂ��Ă���NavMeshAgent���擾
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
        stateManager = GetComponent<PlayerStateManager>();
    }

    // �w�肵�����W�ֈړ�����
    public void MoveTo(Vector3 position)
    {
        // ���ւ����Ȃ�ړ��ł��Ȃ�
        if (stateManager != null && !stateManager.CanMove())
            return;

        // �^�����Ȃǂ�Agent�������Ȃ牽�����Ȃ�
        if (!agent.enabled)
            return;

        // NavMesh��ɂ��Ȃ��ꍇ���ړ��ł��Ȃ�
        if (!agent.isOnNavMesh)
            return;

        // �ړI�n��ݒ�
        agent.SetDestination(position);
    }
}