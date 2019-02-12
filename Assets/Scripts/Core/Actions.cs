using UnityEngine;
using UnityEngine.AI;

public static class Actions
{
    public static void Stop(ref NavMeshAgent agent, ref bool rotated, Transform transform)
    {
        rotated = false;
        agent.SetDestination(transform.position);
    }

    public static void Attack(ref Player enemy, ref Player self, ref float attackIntervalTimer, Transform transform)
    {
        var canAttack = enemy != null && self != null && self.CanAttack();
        if (canAttack && attackIntervalTimer <= 0)
        {
            transform.LookAt(enemy.transform);
            self.Attack();
        }
    }
}