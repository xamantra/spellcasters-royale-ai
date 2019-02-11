using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private Transform selfCamera;
    [SerializeField] private Player prefab;
    [SerializeField, Range(1, 10)] private int count;
    [Header("Spawn Range")]
    [SerializeField] private float minRange;
    [SerializeField] private float maxRange;

    private void Start()
    {
        for (int i = 0; i < count; i++)
        {
            var player = Instantiate(prefab, new Vector3(Random.Range(minRange, maxRange), prefab.transform.position.y, Random.Range(minRange, maxRange)), Quaternion.identity);
            if (i == 0)
            {
                player.SetAsSelf(selfCamera);
            }
        }
    }
}