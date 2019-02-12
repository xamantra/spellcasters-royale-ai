using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Player), typeof(NavMeshAgent), typeof(CapsuleCollider))]
public class AI : MonoBehaviour
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
    private Collider collider;
    #endregion

    #region dynamic data
    private Player nearestEnemy;
    private IWeapon nearestWeapon;
    private bool lootDetected;
    private bool enemyDetected;
    private bool lootInRange;
    private bool enemyInRange;
    private Vector3 currentDestination;
    private float remainingDistance;
    private bool hasNearestWeapon;
    private bool gettingNearestObject;
    private int desiredRotationY;
    private bool rotated;
    private int currentRotationY;
    private float attackIntervalTimer;
    private System.Action move;
    private System.Action moveStill;
    private System.Action moveToNearestWeapon;
    private System.Action moveToNearestEnemy;
    private System.Action findDirection;
    private System.Action getNearestWeapon;
    private System.Action getNearestEnemy;
    private Collider[] weapons;
    private Collider[] enemies;
    #endregion
    #endregion

    #region unity methods
    private void Start()
    {
        move = () => { NavPath.Move(ref agent, ref currentDestination, ref directionGuide, ref rotated, findDirection); };
        moveStill = () => { NavPath.Move(ref agent, ref currentDestination, ref directionGuide, ref rotated, findDirection, transform.position); };
        moveToNearestWeapon = () => { NavPath.Move(ref agent, ref currentDestination, ref directionGuide, ref rotated, findDirection, nearestWeapon.transform.position); };
        moveToNearestEnemy = () => { NavPath.Move(ref agent, ref currentDestination, ref directionGuide, ref rotated, findDirection, nearestEnemy.transform.position); };
        findDirection = () => { Direction.SelectRandomDirection(ref desiredRotationY, ref directionEngine, ref rotated); };
        getNearestWeapon = () => { Sensor.GetNearestObject(ref nearestWeapon, ref gettingNearestObject, weapons, transform.position); };
        getNearestEnemy = () => { Sensor.GetNearestObject(ref nearestEnemy, ref gettingNearestObject, enemies, transform.position); };
        Serialize();
    }

    private void Update()
    {
        UpdateStates();
        remainingDistance = agent.remainingDistance;
        hasNearestWeapon = nearestWeapon != null;

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
            weapons = Sensor.Scan<IWeapon>(transform, collider, roamRange, lootLayerMask.value);
            if (remainingDistance <= 0) // no weapon and not moving
            {
                if (rotated)
                {
                    move?.Invoke();
                }
                else
                {
                    if (lootDetected && !gettingNearestObject && nearestWeapon == null)
                    {
                        getNearestWeapon?.Invoke();
                    }
                    else if (nearestWeapon != null && !lootInRange)
                    {
                        try
                        {
                            moveToNearestWeapon?.Invoke();
                        }
                        catch
                        {
                            nearestWeapon = null;
                        }
                    }
                    else if (lootInRange)
                    {
                        Actions.Stop(ref agent, ref rotated, transform);
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
                        findDirection?.Invoke();
                    }
                }
            }
            else // no weapon and moving
            {
                if (rotated)
                {
                    moveStill?.Invoke();
                }
                else
                {
                    if (lootDetected && !gettingNearestObject && nearestWeapon == null)
                    {
                        getNearestWeapon?.Invoke();
                    }
                    else if (nearestWeapon != null && !lootDetected && !lootInRange)
                    {
                        nearestWeapon = null;
                        move?.Invoke();
                    }
                    else if (nearestWeapon != null && lootDetected && !lootInRange)
                    {
                        try
                        {
                            if (nearestWeapon.Exists())
                                moveToNearestWeapon?.Invoke();
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
                        Actions.Stop(ref agent, ref rotated, transform);
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
        }
        else
        {
            nearestWeapon = null;
            enemies = Sensor.Scan<Player>(transform, collider, roamRange, enemyLayerMask.value);
            if (remainingDistance <= 0) // has weapon and not moving
            {
                if (rotated)
                {
                    move?.Invoke();
                }
                else
                {
                    if (enemyDetected && !gettingNearestObject && nearestEnemy == null)
                    {
                        getNearestEnemy?.Invoke();
                    }
                    else if (enemyDetected && !gettingNearestObject && nearestEnemy != null && !enemyInRange)
                    {
                        getNearestEnemy?.Invoke();
                        if (!rotated)
                        {
                            findDirection?.Invoke();
                        }
                    }
                    else if (enemyDetected && !gettingNearestObject && nearestEnemy != null && enemyInRange)
                    {
                        getNearestEnemy?.Invoke();
                        Actions.Stop(ref agent, ref rotated, transform);
                        Actions.Attack(ref nearestEnemy, ref player, ref attackIntervalTimer, transform);
                    }
                    else
                    {
                        findDirection?.Invoke();
                    }
                }
            }
            else // has weapon and moving
            {
                if (rotated)
                {
                    moveStill?.Invoke();
                }
                else
                {
                    if (enemyDetected && !gettingNearestObject && nearestEnemy == null)
                    {
                        getNearestEnemy?.Invoke();
                    }
                    else if (nearestEnemy != null && !enemyInRange)
                    {
                        moveToNearestEnemy?.Invoke();
                    }
                    else if (enemyInRange)
                    {
                        Actions.Stop(ref agent, ref rotated, transform);
                    }
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
    #endregion
}