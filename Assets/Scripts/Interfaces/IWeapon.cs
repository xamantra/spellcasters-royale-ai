using UnityEngine;

public interface IWeapon
{
    void Attack(Transform spawnPoint, IPlayer attacker);
    void Equip(ref IPlayer player);
    bool Exists();
    Material Material { get; }
    Transform transform { get; }
}