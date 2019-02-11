using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class Player : MonoBehaviour
{
    [SerializeField] private bool self;
    [SerializeField] private MeshRenderer weaponHandler;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField, Range(10, 100)] private int maxHealth;

    private Transform playerCamera;
    public IWeapon Weapon { get; private set; }
    public int CurrentHealth { get; private set; }
    public int Id { get; private set; }
    private Material defaultMaterial;

    private void Start()
    {
        CurrentHealth = maxHealth;
        Id = Random.Range(10000000, 99999999);
        defaultMaterial = weaponHandler.sharedMaterial;
    }

    private void Update()
    {
        if (self)
        {
            playerCamera.position = transform.position;
        }
    }

    public void SetAsSelf(Transform camera)
    {
        playerCamera = camera;
        self = true;
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

    public void TakeDamage(IProjectile projectile)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth - projectile.Damage, 0, maxHealth);
        if (CurrentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}