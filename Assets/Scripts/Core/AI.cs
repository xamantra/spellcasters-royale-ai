﻿using System.Linq;
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
    #endregion

    #region unity methods
    private void Start()
    {
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
            var weapons = Scan<IWeapon>(roamRange, lootLayerMask.value);
            if (remainingDistance <= 0) // no weapon and not moving
            {
                if (rotated)
                {
                    Move();
                }
                else
                {
                    if (lootDetected && !gettingNearestObject && nearestWeapon == null)
                    {
                        GetNearest(ref nearestWeapon, weapons, transform.position);
                    }
                    else if (nearestWeapon != null && !lootInRange)
                    {
                        try
                        {
                            Move(nearestWeapon.transform.position);
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
                        RotateRandom();
                    }
                }
            }
            else // no weapon and moving
            {
                if (rotated)
                {
                    Move(transform.position);
                }
                else
                {
                    if (lootDetected && !gettingNearestObject && nearestWeapon == null)
                    {
                        GetNearest(ref nearestWeapon, weapons, transform.position);
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
                                Move(nearestWeapon.transform.position);
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
            var enemies = Scan<Player>(roamRange, enemyLayerMask.value);
            if (remainingDistance <= 0) // has weapon and not moving
            {
                if (rotated)
                {
                    Move();
                }
                else
                {
                    if (enemyDetected && !gettingNearestObject && nearestPlayer == null)
                    {
                        GetNearest(ref nearestPlayer, enemies, transform.position);
                    }
                    else if (enemyDetected && !gettingNearestObject && nearestPlayer != null && !enemyInRange)
                    {
                        GetNearest(ref nearestPlayer, enemies, transform.position);
                        if (!rotated)
                        {
                            RotateRandom();
                        }
                    }
                    else if (enemyDetected && !gettingNearestObject && nearestPlayer != null && enemyInRange)
                    {
                        GetNearest(ref nearestPlayer, enemies, transform.position);
                        Stop();
                        Attack(ref nearestPlayer, ref player, ref attackIntervalTimer);
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
                        RotateRandom();
                    }
                }
            }
            else // has weapon and moving
            {
                if (rotated)
                {
                    Move(transform.position);
                }
                else
                {
                    if (enemyDetected && !gettingNearestObject && nearestPlayer == null)
                    {
                        GetNearest(ref nearestPlayer, enemies, transform.position);
                    }
                    else if (nearestPlayer != null && !enemyInRange)
                    {
                        Move(nearestPlayer.transform.position);
                    }
                    else if (enemyInRange)
                    {
                        Stop();
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
    private void Move(Vector3? destination = null)
    {
        currentDestination = destination.HasValue ? destination.Value : directionGuide.position;
        var validPath = agent.CalculatePath(currentDestination, agent.path);
        if (validPath)
        {
            agent.SetDestination(currentDestination);
            rotated = false;
        }
        else
        {
            RotateRandom();
        }
    }

    private void Stop()
    {
        rotated = false;
        agent.SetDestination(transform.position);
    }

    private void Attack(ref Player enemy, ref Player self, ref float attackIntervalTimer)
    {
        var canAttack = enemy != null && self != null && self.CanAttack();
        if (canAttack && attackIntervalTimer <= 0)
        {
            transform.LookAt(enemy.transform);
            self.Attack();
        }
    }

    private void RotateRandom()
    {
        desiredRotationY = Mathf.RoundToInt(Random.Range(0, 360));
        directionEngine.transform.rotation = Quaternion.Euler(0, desiredRotationY, 0);
        rotated = true;
    }

    private void SmoothRotate()
    {
        //currentRotationY = Mathf.RoundToInt(transform.localEulerAngles.y);
        //if (desiredRotationY == currentRotationY)
        //{
        //    Move();
        //}
        //else
        //{
        //    var r = transform.rotation;
        //    var tor = Quaternion.Euler(new Vector3(r.x, desiredRotationY, r.z));
        //    transform.rotation = Quaternion.Lerp(transform.rotation, tor, Time.time * rotationLerp);
        //}
    }

    private Collider[] Scan<T>(float radius, int layerMask)
    {
        return Physics.OverlapSphere(transform.position, radius, layerMask).Where(x => x.GetComponent<T>() != null && x != collider).Distinct().ToArray() ?? new Collider[0];
    }

    private void GetNearest<T>(ref T result, Collider[] objects, Vector3 position, int nearestIndex = 0, int index = 0)
    {
        if (objects.Length == 0) return;
        gettingNearestObject = true;
        var nearest = objects[nearestIndex];
        var i = index;
        if (i == objects.Length - 1)
        {
            result = nearest.GetComponent<T>();
            gettingNearestObject = false;
            return;
        }
        else
        {
            var candidateDistance = Mathf.Abs(Vector3.Distance(position, nearest.transform.position));
            var currentDistance = Mathf.Abs(Vector3.Distance(position, objects[i].transform.position));
            if (currentDistance < candidateDistance)
            {
                GetNearest(ref result, objects, position, i, index + 1);
            }
            else
            {
                GetNearest(ref result, objects, position, nearestIndex, index + 1);
            }
        }
    }

    private void UpdateStates()
    {
        lootDetected = Scan<IWeapon>(roamRange, lootLayerMask.value).Length > 0 ? true : false;
        enemyDetected = Scan<Player>(roamRange, enemyLayerMask.value).Length > 0 ? true : false;
        lootInRange = Scan<IWeapon>(pickupRange, lootLayerMask.value).Length > 0 ? true : false;
        enemyInRange = Scan<Player>(attackRange, enemyLayerMask.value).Length > 0 ? true : false;
    }
    #endregion
}