using System.Linq;
using UnityEngine;

public static class Sensor
{
    public static Collider[] Scan<T>(Transform scanner, Collider self, float radius, int layerMask)
    {
        return Physics.OverlapSphere(scanner.position, radius, layerMask).Where(x => x.GetComponent<T>() != null && x != self).Distinct().ToArray() ?? new Collider[0];
    }
}