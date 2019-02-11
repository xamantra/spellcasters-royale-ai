using UnityEngine;

public interface IWeapon
{
    void Attack(Transform spawnPoint, Player attacker);
    void Equip(ref Player player);
    Material Material { get; }
    Transform transform { get; }
}