using UnityEngine;

public interface IWeapon
{
    void Attack();
    void Equip(ref Player player);
    Material Material { get; }
    Transform transform { get; }
}