using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class Player : MonoBehaviour, IPlayer, IBotControl
{
    [SerializeField] private MeshRenderer weaponHandler;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField, Range(10, 100)] private int maxHealth;

    private Transform playerCamera;
    public IWeapon Weapon { get; set; }
    public int CurrentHealth { get; private set; }
    public int Id { get; private set; }
    private Material defaultMaterial;
    public bool IsSelected { get; private set; }

    private void Start()
    {
        CurrentHealth = maxHealth;
        Id = Random.Range(10000000, 99999999);
        defaultMaterial = weaponHandler.sharedMaterial;
    }

    public void SetWeapon(IWeapon weapon)
    {
        Weapon = weapon;
        weaponHandler.sharedMaterial = weapon.Material;
    }

    public void UnsetWeapon(IWeapon weapon)
    {
        if (Weapon == weapon)
        {
            Weapon = null;
            weaponHandler.sharedMaterial = defaultMaterial;
        }
    }

    public bool CanAttack()
    {
        return Weapon != null;
    }

    public void Attack()
    {
        Weapon?.Attack(projectileSpawnPoint, this);
    }

    public void Select()
    {
        IsSelected = true;
    }

    public void Deselect()
    {
        IsSelected = false;
    }

    public void TakeDamage(IProjectile projectile)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth - projectile.Damage, 0, maxHealth);
        if (CurrentHealth <= 0)
        {
            Destroy(gameObject);
        }
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
}