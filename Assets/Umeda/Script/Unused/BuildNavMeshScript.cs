using System.Collections;
using UnityEngine;
using Unity.AI.Navigation;

[RequireComponent(typeof(NavMeshSurface))]

public class BuildNavMeshScript : MonoBehaviour
{
    private NavMeshSurface m_NavMeshSurface;
    public GameObject[] Characters;
    public Transform[] SpawnPoints;

    private void Awake()
    {
        m_NavMeshSurface = GetComponent<NavMeshSurface>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_NavMeshSurface.BuildNavMesh();
        StartCoroutine(SpawnCharacters());
    }

    IEnumerator SpawnCharacters()
    {
        yield return new WaitForEndOfFrame();

        for (int i = 0; i < Characters.Length; i++)
        {
            Instantiate(Characters[i], SpawnPoints[i].position, SpawnPoints[i].rotation);
        }
    }
}
