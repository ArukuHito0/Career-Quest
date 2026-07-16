using UnityEngine;
using UnityEngine.AI;

public class PlayerCarry : MonoBehaviour
{
    private CarryObject carryObject;
    private NavMeshAgent agent;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }


    private void OnTriggerEnter(Collider other)
    {
        CarryObject obj = other.GetComponentInParent<CarryObject>();

        if (obj != null)
        {
            obj.JoinCarry(this);
        }
    }


    public void AttachToObject(CarryObject obj, Transform point)
    {
        carryObject = obj;

        NavMeshAgent agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.ResetPath();
            agent.enabled = false;
        }


        transform.position = point.position;
        transform.rotation = point.rotation;

        transform.SetParent(obj.transform);
    }

    public bool IsCarrying()
    {
        return carryObject != null;
    }


    public void Release()
    {
        transform.SetParent(null);

        NavMeshAgent agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(transform.position);
        }

        carryObject = null;
    }
}