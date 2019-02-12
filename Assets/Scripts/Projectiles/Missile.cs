using UnityEngine;

public class Missile : MonoBehaviour, IProjectile, IPoolable
{
    [SerializeField, Range(1, 100)] private int damage;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float projectileRange;
    [SerializeField] private MeshRenderer meshRenderer;

    public int Damage {
        get {
            return damage;
        }
    }
    public MeshRenderer MeshRenderer {
        get {
            return meshRenderer;
        }
    }

    public bool Enabled {
        get {
            return gameObject.activeSelf;
        }
    }

    private Player owner;
    private Vector3 spawnPosition;

    public void Spawn(Transform spawnPoint, ref Player owner, Material material)
    {
        gameObject.SetActive(false);
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        MeshRenderer.sharedMaterial = material;
        this.owner = owner;
        spawnPosition = transform.position;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        transform.Translate(0, 0, Time.deltaTime * projectileSpeed);
        var distance = Mathf.Abs(Vector3.Distance(spawnPosition, transform.position));
        if (distance >= projectileRange)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        var player = collision.gameObject.GetComponent<Player>();
        var otherProjectile = collision.gameObject.GetComponent<IProjectile>();
        if (otherProjectile != null)
        {
            return;
        }
        if (player != null)
        {
            if (player.Id == owner.Id)
            {
                return;
            }
            else
            {
                player.TakeDamage(this);
                gameObject.SetActive(false);
                return;
            }
        }

        gameObject.SetActive(false);
    }
}