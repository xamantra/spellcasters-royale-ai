using UnityEngine;

public class Eyes : MonoBehaviour
{
    [SerializeField] private Eye left;
    [SerializeField] private Eye right;

    public bool HasVision()
    {
        return left.HasVision && right.HasVision;
    }

    public void Open(float maxDistance, Vector3 direction, LayerMask layerMask, Vector3? customOrigin = null)
    {
        left.Open(maxDistance, direction, layerMask, customOrigin);
        right.Open(maxDistance, direction, layerMask, customOrigin);
    }
}