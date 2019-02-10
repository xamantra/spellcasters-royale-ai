using System.Collections.Generic;
using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    [SerializeField, Range(20, 200)] private int objectCount;
    [SerializeField] private GameObject prefab;
    [SerializeField] private List<Material> materials;
    [SerializeField, Range(1f, 5f)] private float minScale;
    [SerializeField, Range(1f, 5f)] private float maxScale;
    [Header("Spawn Range")]
    [SerializeField] private float minSpawnRange;
    [SerializeField] private float maxSpawnRange;

    [ContextMenu("Spawn Objects")]
    private void SpawnObjects()
    {
        for (int i = 0; i < objectCount; i++)
        {
            var randomScale = new Vector3(Random.Range(minScale, maxScale), Random.Range(minScale, maxScale), Random.Range(minScale, maxScale));
            var randomPosition = new Vector3(Random.Range(minSpawnRange, maxSpawnRange), randomScale.y / 2f, Random.Range(minSpawnRange, maxSpawnRange));
            var randomMaterial = materials[Random.Range(0, materials.Count)];
            var spawnedObject = Instantiate(prefab, randomPosition, Quaternion.identity, transform);
            spawnedObject.transform.localScale = randomScale;
            spawnedObject.GetComponent<MeshRenderer>().sharedMaterial = randomMaterial;
        }
    }

    [ContextMenu("Clear Objects")]
    private void ClearObjects()
    {
        if (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
            ClearObjects();
        }
        return;
    }
}