using System.Collections.Generic;
using UnityEngine;

public class LootSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> weapons;
    [SerializeField] private int count;
    [Header("Spawn Range")]
    [SerializeField] private float minRange;
    [SerializeField] private float maxRange;

    private void Start()
    {
        for (int i = 0; i < count; i++)
        {
            var randomWeapon = weapons[Random.Range(0, weapons.Count)];
            var randX = Random.Range(minRange, maxRange);
            var randZ = Random.Range(minRange, maxRange);
            Instantiate(randomWeapon, new Vector3(randX, 1f, randZ), Quaternion.identity);
        }
    }
}