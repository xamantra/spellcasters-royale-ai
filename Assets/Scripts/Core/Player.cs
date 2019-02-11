using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class Player : MonoBehaviour
{
    [SerializeField] private bool self;
    [SerializeField] private MeshRenderer weaponHandler;
    [SerializeField, Range(10, 100)] private int maxHealth;

    private Transform playerCamera;
    public IWeapon Weapon { get; private set; }
    public int CurrentHealth { get; private set; }

    private void Start()
    {
        CurrentHealth = maxHealth;
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

    public void TakeDamage(IProjectile projectile)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth - projectile.Damage, 0, maxHealth);
        if (CurrentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}