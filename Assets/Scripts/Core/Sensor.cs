using System.Linq;
using UnityEngine;

public static class Sensor
{
    public static Collider[] Scan<T>(Transform ai, Collider self, float radius, int layerMask)
    {
        return Physics.OverlapSphere(ai.position, radius, layerMask).Where(x => x.GetComponent<T>() != null && x != self).Distinct().ToArray() ?? new Collider[0];
    }

    public static RaycastHit[] Scan<T>(Transform ai, float radius, float maxDistance, int layerMask)
    {
        return Physics.SphereCastAll(ai.position, radius, ai.forward, maxDistance, layerMask).Where(x => x.transform.GetComponent<T>() != null && x.transform != ai).Distinct().ToArray() ?? new RaycastHit[0];
    }

    public static bool InRange<T>(Transform ai, Collider collider, float range, LayerMask layerMask)
    {
        return Scan<T>(ai, collider, range, layerMask.value).Length > 0 ? true : false;
    }

    public static bool InSight<T>(Transform ai, float radius, float maxDistance, LayerMask layerMask)
    {
        return Scan<T>(ai, radius, maxDistance, layerMask.value).Length > 0 ? true : false;
    }

    public static void GetNearestObject<T>(ref T result, ref bool flag, Collider[] objects, Vector3 aiPosition, int nearestIndex = 0, int currentIndex = 0)
    {
        if (objects.Length == 0) return;
        flag = true;
        #region old version
        //var nearest = objects[nearestIndex];
        //if (currentIndex == objects.Length - 1)
        //{
        //    result = nearest.GetComponent<T>();
        //    flag = false;
        //    return;
        //}
        //else
        //{
        //    var nearestDistance = Mathf.RoundToInt(Vector3.Distance(aiPosition, nearest.transform.position));
        //    var currentDistance = Mathf.RoundToInt(Vector3.Distance(aiPosition, objects[currentIndex].transform.position));
        //    if (currentDistance < nearestDistance)
        //    {
        //        GetNearestObject(ref result, ref flag, objects, aiPosition, currentIndex, currentIndex + 1);
        //    }
        //    else
        //    {
        //        GetNearestObject(ref result, ref flag, objects, aiPosition, nearestIndex, currentIndex + 1);
        //    }
        //}
        #endregion
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