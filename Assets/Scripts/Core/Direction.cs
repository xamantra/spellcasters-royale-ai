using UnityEngine;

public static class Direction
{
    public static void SelectRandomDirection(ref int desiredRotationY, ref Transform directionEngine, ref bool rotated)
    {
        desiredRotationY = Mathf.RoundToInt(Random.Range(0, 360));
        directionEngine.transform.rotation = Quaternion.Euler(0, desiredRotationY, 0);
        rotated = true;
    }
}