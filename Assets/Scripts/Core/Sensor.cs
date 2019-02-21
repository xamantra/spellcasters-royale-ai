using System.Linq;
using UnityEngine;

public static class Sensor
{
    public static Collider[] Scan<T>(Transform ai, Collider self, float radius, int layerMask)
    {
        return Physics.OverlapSphere(ai.position, radius, layerMask).Where(x => x.GetComponent<T>() != null && x != self).Distinct().ToArray() ?? new Collider[0];
    }

    public static bool InRange<T>(Transform ai, Collider collider, float range, LayerMask layerMask)
    {
        return Scan<T>(ai, collider, range, layerMask.value).Length > 0 ? true : false;
    }

    public static void GetNearestObject<T>(ref T result, ref bool flag, Collider[] objects, Vector3 aiPosition, int nearestIndex = 0, int currentIndex = 0)
    {
        if (objects.Length == 0) return;
        flag = true;
        Collider nearest = null;
        for (int i = 0; i < objects.Length; i++)
        {
            if (i == 0)
            {
                nearest = objects[i];
            }
            else
            {
                var nearestDistance = Mathf.RoundToInt(Vector3.Distance(aiPosition, nearest.transform.position));
                var currentDistance = Mathf.RoundToInt(Vector3.Distance(aiPosition, objects[i].transform.position));
                if (currentDistance < nearestDistance)
                {
                    nearest = objects[i];
                }
            }

            if (i == objects.Length - 1)
            {
                result = nearest.GetComponent<T>();
                flag = false;
            }
        }
    }
}