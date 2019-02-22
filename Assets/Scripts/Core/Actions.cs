using UnityEngine;
using UnityEngine.AI;

public static class Actions
{
    public static void Stop(ref NavMeshAgent agent, Transform transform)
    {
        agent.SetDestination(transform.position);
    }

    public static void Attack(ref IPlayer enemy, ref IBotControl botControl, ref float attackIntervalTimer, Transform transform)
    {
        // not exportable.
        var canAttack = enemy != null && botControl != null && botControl.CanAttack();
        if (canAttack && attackIntervalTimer <= 0)
        {
            transform.LookAt(enemy.transform);
            botControl.Attack();
        }
    }
}