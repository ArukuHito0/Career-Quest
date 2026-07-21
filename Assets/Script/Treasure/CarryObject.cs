using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class CarryObject : MonoBehaviour
{
    [SerializeField] private int requiredPeople = 4;
    [SerializeField] private Transform[] carryPoints;

    private List<PlayerCarry> carriers = new();

    private NavMeshAgent agent;
    private bool canCarry;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }


    public bool JoinCarry(PlayerCarry player)
    {
        if (carriers.Contains(player))
            return false;

        if (carriers.Count >= carryPoints.Length)
            return false;


        carriers.Add(player);

        player.AttachToObject(this, carryPoints[carriers.Count - 1]);


        if (carriers.Count >= requiredPeople)
        {
            canCarry = true;
            Debug.Log("‰^”À‰Â”\");
        }

        return true;
    }


    public void MoveTo(Vector3 target)
    {
        if (!canCarry)
            return;

        agent.SetDestination(target);
    }


    public bool CanCarry()
    {
        return canCarry;
    }


    public Vector3 GetPosition()
    {
        return transform.position;
    }
}