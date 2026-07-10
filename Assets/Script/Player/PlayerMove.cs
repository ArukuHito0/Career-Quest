using UnityEngine;
using UnityEngine.AI;

public class PlayerMove : MonoBehaviour
{
    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void MoveTo(Vector3 position)
    {
        agent.SetDestination(position);
    }
}
