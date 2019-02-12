using System;
using UnityEngine;
using UnityEngine.AI;

public static class NavPath
{
    public static void Move(ref NavMeshAgent agent, ref Vector3 currentDestination, ref Transform directionGuide, ref bool rotated, Vector3? destination = null, Action failCallBack = null)
    {
        currentDestination = destination.HasValue ? destination.Value : directionGuide.position;
        var validPath = agent.CalculatePath(currentDestination, agent.path);
        if (validPath)
        {
            agent.SetDestination(currentDestination);
            rotated = false;
        }
        else
        {
            //SelectDirectionRandom();
            failCallBack?.Invoke();
        }
    }
}