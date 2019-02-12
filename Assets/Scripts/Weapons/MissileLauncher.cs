using UnityEngine;

public class MissileLauncher : MonoBehaviour, IWeapon
{
    [SerializeField] private Missile projectilePrefab;

    public Material Material {
        get {
            return GetComponentInChildren<MeshRenderer>().sharedMaterial;
        }
    }

    public void Attack(Transform spawnPoint, Player attacker)
    {
        var projectile = Instantiate(projectilePrefab);
        projectile.Spawn(spawnPoint, ref attacker, Material);
    }

    public void Equip(ref Player player)
    {
        player.SetWeapon(this);
        transform.SetParent(player.transform);
        transform.localPosition = Vector3.zero;
        gameObject.SetActive(false);
    }

    public bool Exists()
    {
        return gameObject.activeSelf;
    }
}