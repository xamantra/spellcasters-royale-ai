using UnityEngine;

/// <summary>
/// Must be implemented outside AICore.
/// </summary>
public interface IBotControl
{
    Transform transform { get; }
    bool CanAttack();
    void Attack();
}