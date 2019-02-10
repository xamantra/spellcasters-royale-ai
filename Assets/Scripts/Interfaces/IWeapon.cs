using UnityEngine;

public interface IWeapon
{
    void Attack();
    void Equip(Player player);
    Material Material { get; }
    Transform transform { get; }
}