using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Player), typeof(NavMeshAgent), typeof(CapsuleCollider))]
public class AI : MonoBehaviour
{
    #region variables
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

    private Player player;
    private NavMeshAgent agent;
    private Collider collider;
    private Player nearestPlayer;
    private IWeapon nearestWeapon;
    private Collider[] enemies;
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
    private System.Action findDirection;
    private System.Action getNearestWeapon;
    private System.Action getNearestEnemy;
    private Collider[] weapons;
    #endregion

    #region unity methods
    private void Start()
    {
        findDirection = () => { Direction.SelectDirectionRandom(ref desiredRotationY, ref directionEngine, ref rotated); };
        getNearestWeapon = () => { Sensor.GetNearestObject(ref nearestWeapon, ref gettingNearestObject, weapons, transform.position); };
        getNearestEnemy = () => { Sensor.GetNearestObject(ref nearestPlayer, ref gettingNearestObject, enemies, transform.position); };
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
                    NavPath.Move(ref agent, ref currentDestination, ref directionGuide, ref rotated, findDirection);
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
                            NavPath.Move(ref agent, ref currentDestination, ref directionGuide, ref rotated, findDirection, nearestWeapon.transform.position);
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
                    NavPath.Move(ref agent, ref currentDestination, ref directionGuide, ref rotated, findDirection, transform.position);
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
                        NavPath.Move(ref agent, ref currentDestination, ref directionGuide, ref rotated, findDirection);
                    }
                    else if (nearestWeapon != null && lootDetected && !lootInRange)
                    {
                        try
                        {
                            if (nearestWeapon.Exists())
                                NavPath.Move(ref agent, ref currentDestination, ref directionGuide, ref rotated, findDirection, nearestWeapon.transform.position);
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
                    NavPath.Move(ref agent, ref currentDestination, ref directionGuide, ref rotated, findDirection);
                }
                else
                {
                    if (enemyDetected && !gettingNearestObject && nearestPlayer == null)
                    {
                        getNearestEnemy?.Invoke();
                    }
                    else if (enemyDetected && !gettingNearestObject && nearestPlayer != null && !enemyInRange)
                    {
                        getNearestEnemy?.Invoke();
                        if (!rotated)
                        {
                            findDirection?.Invoke();
                        }
                    }
                    else if (enemyDetected && !gettingNearestObject && nearestPlayer != null && enemyInRange)
                    {
                        getNearestEnemy?.Invoke();
                        Actions.Stop(ref agent, ref rotated, transform);
                        Actions.Attack(ref nearestPlayer, ref player, ref attackIntervalTimer, transform);
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
                    NavPath.Move(ref agent, ref currentDestination, ref directionGuide, ref rotated, findDirection, transform.position);
                }
                else
                {
                    if (enemyDetected && !gettingNearestObject && nearestPlayer == null)
                    {
                        getNearestEnemy?.Invoke();
                    }
                    else if (nearestPlayer != null && !enemyInRange)
                    {
                        NavPath.Move(ref agent, ref currentDestination, ref directionGuide, ref rotated, findDirection, nearestPlayer.transform.position);
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