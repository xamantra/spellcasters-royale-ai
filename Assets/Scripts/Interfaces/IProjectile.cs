using UnityEngine;

public interface IProjectile
{
    int Damage { get; }
    void Spawn(Vector3 position, Quaternion rotation);
}