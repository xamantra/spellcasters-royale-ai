using UnityEngine;

public class PositionValidator : MonoBehaviour
{
    [Header("Spawn Range")]
    [SerializeField] private float minRange;
    [SerializeField] private float maxRange;
    [SerializeField] private LayerMask[] ignoreLayers;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) return;
        var random = new Vector3(Random.Range(minRange, maxRange), transform.position.y, Random.Range(minRange, maxRange));
        transform.position = random;
    }
}