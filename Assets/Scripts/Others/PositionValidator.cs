using UnityEngine;

public class PositionValidator : MonoBehaviour
{
    [Header("Spawn Range")]
    [SerializeField] private float minRange;
    [SerializeField] private float maxRange;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) return;
        transform.position = new Vector3(Random.Range(minRange, maxRange), transform.position.y, Random.Range(minRange, maxRange));
    }
}