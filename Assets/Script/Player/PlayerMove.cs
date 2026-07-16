using UnityEngine;
using UnityEngine.AI;

public class PlayerMove : MonoBehaviour
{
    private NavMeshAgent agent;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }


    public void MoveTo(Vector3 position)
    {
        if (!agent.enabled)
            return;

        if (!agent.isOnNavMesh)
            return;

        agent.SetDestination(position);
    }
}