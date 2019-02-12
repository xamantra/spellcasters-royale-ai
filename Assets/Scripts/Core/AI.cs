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
    #endregion

    #region unity methods
    private void Start()
    {
        findDirection = () => { Direction.SelectDirectionRandom(ref desiredRotationY, ref directionEngine, ref rotated); };
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
            var weapons = Sensor.Scan<IWeapon>(transform, collider, roamRange, lootLayerMask.value);
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
                        ObjectSearch.GetNearestObject(ref nearestWeapon, ref gettingNearestObject, weapons, transform.position);
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
                        ObjectSearch.GetNearestObject(ref nearestWeapon, ref gettingNearestObject, weapons, transform.position);
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
                    #region old code
                    //else if (nearestWeapon == null && !lootDetected)
                    //{
                    //    Move();
                    //}
                    //else if (nearestWeapon != null && !lootInRange)
                    //{
                    //    try
                    //    {
                    //        Move(nearestWeapon.transform.position);
                    //    }
                    //    catch
                    //    {
                    //        nearestWeapon = null;
                    //    }
                    //}
                    //else if (lootInRange)
                    //{
                    //    Stop();
                    //    try
                    //    {
                    //        nearestWeapon.Equip(ref player);
                    //    }
                    //    catch
                    //    {
                    //        nearestWeapon = null;
                    //    }
                    //}
                    #endregion
                }
            }
        }
        else
        {
            nearestWeapon = null;
            var enemies = Sensor.Scan<Player>(transform, collider, roamRange, enemyLayerMask.value);
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
                        ObjectSearch.GetNearestObject(ref nearestPlayer, ref gettingNearestObject, enemies, transform.position);
                    }
                    else if (enemyDetected && !gettingNearestObject && nearestPlayer != null && !enemyInRange)
                    {
                        ObjectSearch.GetNearestObject(ref nearestPlayer, ref gettingNearestObject, enemies, transform.position);
                        if (!rotated)
                        {
                            findDirection?.Invoke();
                        }
                    }
                    else if (enemyDetected && !gettingNearestObject && nearestPlayer != null && enemyInRange)
                    {
                        ObjectSearch.GetNearestObject(ref nearestPlayer, ref gettingNearestObject, enemies, transform.position);
                        Actions.Stop(ref agent, ref rotated, transform);
                        Actions.Attack(ref nearestPlayer, ref player, ref attackIntervalTimer, transform);
                    }
                    //else if (nearestPlayer != null && !enemyInRange)
                    //{
                    //    Move(nearestPlayer.transform.position);
                    //}
                    //else if (nearestPlayer != null && enemyInRange)
                    //{
                    //    Stop();
                    //    Attack(ref nearestPlayer, ref player, ref attackIntervalTimer);
                    //}
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
                        ObjectSearch.GetNearestObject(ref nearestPlayer, ref gettingNearestObject, enemies, transform.position);
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

        #region old code
        //var condition1 = remainingDistance <= 0 && !isMoving && !rotating;
        //var condition2 = remainingDistance <= 0 && isMoving && !rotating;
        //var condition3 = remainingDistance <= 0 && !isMoving && rotating && !lootDetected;
        //var condition4 = player.Weapon == null && nearestWeapon == null && isMoving && lootDetected;
        //var condition5 = player.Weapon == null && nearestWeapon != null;
        //var condition6 = player.Weapon == null && nearestWeapon != null && lootInRange;

        //if (condition1)
        //{
        //    RotateRandom();
        //    return;
        //}

        //if (condition2)
        //{
        //    isMoving = false;
        //    return;
        //}

        //if (condition3)
        //{
        //    SmoothRotate();
        //    return;
        //}

        //if (condition4)
        //{
        //    var weapons = Scan(scanRange, lootLayerMask);
        //    if (weapons.Length > 0 && !gettingNearestObject)
        //    {
        //        GetNearest(ref nearestWeapon, weapons, transform.position);
        //    }
        //}

        //if (condition5)
        //{
        //    Move(nearestWeapon.transform.position);
        //}

        //if (condition6)
        //{
        //    nearestWeapon?.Equip(player);
        //}
        #endregion
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