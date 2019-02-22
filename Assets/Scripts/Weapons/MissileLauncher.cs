using UnityEngine;

public class MissileLauncher : MonoBehaviour, IWeapon, ILootable
{
    [SerializeField] private Missile projectilePrefab;

    private Pooler<Missile> missilePool;
    public Material Material {
        get {
            return GetComponentInChildren<MeshRenderer>().sharedMaterial;
        }
    }

    private void Start()
    {
        missilePool = new Pooler<Missile>(() => { return Instantiate(projectilePrefab); });
    }

    public void Attack(Transform spawnPoint, IPlayer attacker)
    {
        var projectile = missilePool.Get();
        projectile.Spawn(spawnPoint, ref attacker, Material);
    }

    public void Equip(ref IPlayer player)
    {
        player.SetWeapon(this);
        transform.SetParent(player.transform);
        transform.localPosition = Vector3.zero;
        gameObject.SetActive(false);
    }

    public bool Exists()
    {
        try
        {
            return gameObject.activeSelf;
        }
        catch
        {
            return false;
        }
    }

    public void Pickup(ref IPlayer player)
    {
        Equip(ref player);
    }
}