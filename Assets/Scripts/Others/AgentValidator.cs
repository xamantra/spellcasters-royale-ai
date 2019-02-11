using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AgentValidator : MonoBehaviour
{
    [Header("Spawn Range")]
    [SerializeField] private float minRange;
    [SerializeField] private float maxRange;
    [SerializeField] private NavMeshAgent agent;

    private void Start()
    {
        var position = new Vector3(Random.Range(minRange, maxRange), transform.position.y, Random.Range(minRange, maxRange));
        Warp(ref agent, ref position);
    }

    private void OnValidate()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Warp(ref NavMeshAgent agent, ref Vector3 position)
    {
        if (agent.Warp(position))
        {
            Destroy(this);
            return;
        }
        else
        {
            position = new Vector3(Random.Range(minRange, maxRange), transform.position.y, Random.Range(minRange, maxRange));
            Warp(ref agent, ref position);
        }
    }
}