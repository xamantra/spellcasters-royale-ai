using UnityEngine;

public class MissileLauncher : MonoBehaviour, IWeapon
{
    [SerializeField] private IProjectile projectilePrefab;

    public Material Material {
        get {
            return GetComponentInChildren<MeshRenderer>().sharedMaterial;
        }
    }

    public void Attack()
    {
        projectilePrefab.Spawn(transform.position, transform.rotation);
    }

    public void Equip(Player player)
    {
        player.SetWeapon(this);
        Destroy(gameObject);
    }
}