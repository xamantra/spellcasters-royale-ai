using UnityEngine;

public interface IPlayer
{
    int Id { get; }
    Transform transform { get; }
    bool Exists();

    // not exportable.
    IWeapon Weapon { get; set; }
    void TakeDamage(IProjectile projectile);
    void SetWeapon(IWeapon weapon);
}