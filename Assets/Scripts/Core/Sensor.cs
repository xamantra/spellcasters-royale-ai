using System.Linq;
using UnityEngine;

public static class Sensor
{
    public static Collider[] Scan<T>(Transform scanner, Collider self, float radius, int layerMask)
    {
        return Physics.OverlapSphere(scanner.position, radius, layerMask).Where(x => x.GetComponent<T>() != null && x != self).Distinct().ToArray() ?? new Collider[0];
    }

    public static bool InRange<T>(Transform transform, Collider collider, float attackRange, LayerMask enemyLayerMask)
    {
        return Scan<T>(transform, collider, attackRange, enemyLayerMask.value).Length > 0 ? true : false;
    }

    public static void GetNearestObject<T>(ref T result, ref bool flag, Collider[] objects, Vector3 position, int nearestIndex = 0, int index = 0)
    {
        if (objects.Length == 0) return;
        flag = true;
        var nearest = objects[nearestIndex];
        var i = index;
        if (i == objects.Length - 1)
        {
            result = nearest.GetComponent<T>();
            flag = false;
            return;
        }
        else
        {
            var nearestDistance = Mathf.Abs(Vector3.Distance(position, nearest.transform.position));
            var currentDistance = Mathf.Abs(Vector3.Distance(position, objects[i].transform.position));
            if (currentDistance < nearestDistance)
            {
                GetNearestObject(ref result, ref flag, objects, position, i, index + 1);
            }
            else
            {
                GetNearestObject(ref result, ref flag, objects, position, nearestIndex, index + 1);
            }
        }
    }
}