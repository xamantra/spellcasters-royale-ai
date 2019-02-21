using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Player), typeof(NavMeshAgent), typeof(CapsuleCollider))]
public class AICore : MonoBehaviour
{
    #region variables
    #region serialized fields
    [SerializeField, Range(5f, 20f)] private float roamRange;
    [SerializeField, Range(5f, 15f)] private float attackRange;
    [SerializeField, Range(2f, 4f)] private float pickupRange;
    [SerializeField, Range(0f, 1f)] private float rotationLerp;
    [SerializeField, Range(1f, 3f)] private float attackInterval;
    [SerializeField] private LayerMask enemyLayerMask;
    [SerializeField] private LayerMask lootLayerMask;
    [SerializeField] private Transform directionEngine;
    [SerializeField] private Transform directionGuide;
    [SerializeField] private float directionGuideElevation;

    [Header("Debug Colors")]
    [SerializeField] private Color detectionColor;
    [SerializeField] private Color roamColor;
    [SerializeField] private Color attackRangeColor;
    [SerializeField] private Color pickupRangeColor;
    #endregion

    #region static data
    private Player player;
    private NavMeshAgent agent;
    private new Collider collider;
    #endregion

    #region dynamic data
    private Player nearestEnemy;
    private IWeapon nearestWeapon;
    private bool lootDetected;
    private bool enemyDetected;
    private bool lootInRange;
    private bool enemyInRange;
    private Vector3 currentDestination;
    private int remainingDistance;

    public bool IsMoving { get; private set; }

    private bool gettingNearestObject;
    private int desiredRotationY;
    private bool directionSelected;
    private int currentRotationY;
    private float attackIntervalTimer;
    private Collider[] scannedWeapons;
    private Collider[] scannedEnemies;
    #endregion
    #endregion

    #region unity methods
    private void Start()
    {
        Serialize();
    }

    private void Update()
    {
        UpdateStates();
        remainingDistance = Mathf.RoundToInt(Mathf.Abs(agent.remainingDistance));
        IsMoving = remainingDistance > 0;

        if (attackIntervalTimer <= 0)
        {
            attackIntervalTimer = attackInterval;
        }
        else
        {
            attackIntervalTimer -= Time.deltaTime;
        }

        if (player.Weapon == null)
        {
            scannedWeapons = Sensor.Scan<IWeapon>(transform, collider, roamRange, lootLayerMask.value);
            if (!IsMoving) // no weapon and not moving
            {
                if (lootDetected && !gettingNearestObject && nearestWeapon == null)
                {
                    GetNearestWeapon();
                }
                else if (nearestWeapon != null && !lootInRange)
                {
                    try
                    {
                        MoveToNearestWeapon();
                    }
                    catch
                    {
                        nearestWeapon = null;
                    }
                }
                else if (lootInRange)
                {
                    Stop();
                    try
                    {
                        nearestWeapon.Equip(ref player);
                    }
                    catch
                    {
                        nearestWeapon = null;
                    }
                }
                else
                {
                    FindDirection(Move);
                }
            }
            else
            {
                if (lootDetected && !gettingNearestObject && nearestWeapon == null)
                {
                    GetNearestWeapon();
                }
                else if (nearestWeapon != null && !lootDetected && !lootInRange)
                {
                    nearestWeapon = null;
                    Move();
                }
                else if (nearestWeapon != null && lootDetected && !lootInRange)
                {
                    try
                    {
                        if (nearestWeapon.Exists())
                            MoveToNearestWeapon();
                        else
                            nearestWeapon = null;
                    }
                    catch
                    {
                        nearestWeapon = null;
                    }
                }
                else if (nearestWeapon != null && lootDetected && lootInRange)
                {
                    Stop();
                    try
                    {
                        nearestWeapon.Equip(ref player);
                    }
                    catch
                    {
                        nearestWeapon = null;
                    }
                }
            }
        }
        else
        {
            nearestWeapon = null;
            scannedEnemies = Sensor.Scan<Player>(transform, collider, roamRange, enemyLayerMask.value);
            if (!IsMoving) // has weapon and not moving
            {
                if (enemyDetected && !gettingNearestObject && nearestEnemy == null)
                {
                    GetNearestEnemy();
                }
                else if (enemyDetected && !gettingNearestObject && nearestEnemy != null && !enemyInRange)
                {
                    GetNearestEnemy();
                    MoveToNearestEnemy();
                }
                else if (enemyDetected && !gettingNearestObject && nearestEnemy != null && enemyInRange)
                {
                    GetNearestEnemy();
                    Stop();
                    Attack();
                }
                else
                {
                    FindDirection(Move);
                }
            }
            else // has weapon and moving
            {
                if (enemyDetected && !gettingNearestObject && nearestEnemy == null)
                {
                    GetNearestEnemy();
                }
                else if (nearestEnemy != null && !enemyInRange)
                {
                    MoveToNearestEnemy();
                }
                else if (enemyInRange)
                {
                    Stop();
                    Attack();
                }
            }
        }
    }

    private void OnValidate()
    {
        Serialize();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = lootInRange ? detectionColor : pickupRangeColor;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
        Gizmos.color = enemyDetected | lootDetected ? detectionColor : roamColor;
        Gizmos.DrawWireSphere(transform.position, roamRange);
        Gizmos.color = enemyInRange ? detectionColor : attackRangeColor;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    #endregion

    #region helper methods
    private void Serialize()
    {
        if (directionGuide != null)
            directionGuide.localPosition = new Vector3(0, directionGuideElevation, roamRange);
        player = GetComponent<Player>();
        agent = GetComponent<NavMeshAgent>();
        collider = GetComponent<Collider>();
    }
    #endregion

    #region AI methods
    private void UpdateStates()
    {
        lootDetected = Sensor.InRange<IWeapon>(transform, collider, roamRange, lootLayerMask);
        enemyDetected = Sensor.InRange<Player>(transform, collider, roamRange, enemyLayerMask);
        lootInRange = Sensor.InRange<IWeapon>(transform, collider, pickupRange, lootLayerMask);
        enemyInRange = Sensor.InRange<Player>(transform, collider, attackRange, enemyLayerMask);
    }

    private void FindDirection(System.Action callback = null)
    {
        Direction.SelectRandomDirection(ref desiredRotationY, ref directionEngine, ref directionSelected, callback);
    }

    private void Stop()
    {
        Actions.Stop(ref agent, ref directionSelected, transform);
    }

    private void Attack()
    {
        Actions.Attack(ref nearestEnemy, ref player, ref attackIntervalTimer, transform);
    }

    private void Move()
    {
        NavPath.Move(ref agent, ref currentDestination, ref directionGuide, ref directionSelected);
    }

    private void MoveStill()
    {
        NavPath.Move(ref agent, ref currentDestination, ref directionGuide, ref directionSelected, null, transform.position);
    }

    private void MoveToNearestWeapon()
    {
        NavPath.Move(ref agent, ref currentDestination, ref directionGuide, ref directionSelected, null, nearestWeapon.transform.position);
    }

    private void MoveToNearestEnemy()
    {
        NavPath.Move(ref agent, ref currentDestination, ref directionGuide, ref directionSelected, null, nearestEnemy.transform.position);
    }

    private void GetNearestWeapon()
    {
        Sensor.GetNearestObject(ref nearestWeapon, ref gettingNearestObject, scannedWeapons, transform.position);
    }

    private void GetNearestEnemy()
    {
        Sensor.GetNearestObject(ref nearestEnemy, ref gettingNearestObject, scannedEnemies, transform.position);
    }
    #endregion
}