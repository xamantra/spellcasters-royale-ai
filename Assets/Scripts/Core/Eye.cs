using UnityEngine;

public class Eye : MonoBehaviour
{
    private float maxDistance = 0f;
    private Vector3 direction = Vector3.zero;
    private int layerMask = 0;
    private Vector3? customOrigin;
    private Vector3 customOriginValue;

    public bool HasVision { get; private set; }

    public void Open(float maxDistance, Vector3 direction, LayerMask layerMask, Vector3? customOrigin = null)
    {
        this.maxDistance = maxDistance;
        this.direction = transform.forward;
        this.layerMask = layerMask.value;
        this.customOrigin = customOrigin;
        customOriginValue = customOrigin.HasValue ? customOrigin.Value : Vector3.zero;
    }

    private void Update()
    {
        HasVision = Physics.Raycast(customOrigin.HasValue ? customOrigin.Value : transform.position, transform.forward, maxDistance, layerMask);
    }
}