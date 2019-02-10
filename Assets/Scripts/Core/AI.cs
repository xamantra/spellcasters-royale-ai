using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Player), typeof(NavMeshAgent), typeof(CapsuleCollider))]
public class AI : MonoBehaviour
{
    #region variables
    [SerializeField, Range(5f, 15f)] private float roamRange;
    [SerializeField, Range(3f, 5f)] private float scanRange;
    [SerializeField, Range(2f, 4f)] private float pickupRange;
    [SerializeField, Range(0f, 1f)] private float rotationLerp;
    [SerializeField] private LayerMask enemyLayerMask;
    [SerializeField] private LayerMask lootLayerMask;
    [SerializeField] private LayerMask areaMask;
    [SerializeField] private Transform directionGuide;

    private bool lootDetected;
    private bool lootInRange;
    private bool enemyDetected;
    private Player player;
    private NavMeshAgent agent;
    private Collider collider;

    private List<Player> scannedPlayers;
    private List<IWeapon> scannedWeapons;
    private Vector3 currentDestination;
    private int remainingDistance;
    private bool gettingNearestObject;
    private int desiredRotationY;
    private int currentRotationY;
    private bool isMoving;
    private bool rotating;
    private IWeapon nearestWeapon;
    #endregion

    #region unity methods
    private void Start()
    {
        Serialize();
    }

    private void Update()
    {
        UpdateStates();
        remainingDistance = Mathf.RoundToInt(agent.remainingDistance);

        var condition1 = player.Weapon == null && remainingDistance <= 0 && nearestWeapon != null && !isMoving && !rotating && !lootDetected && !lootInRange;
        var condition2 = player.Weapon == null && remainingDistance <= 0 && nearestWeapon != null && !isMoving && !rotating && !lootDetected && !lootInRange;
        var condition3 = player.Weapon == null && remainingDistance <= 0 && nearestWeapon != null && !isMoving && !rotating && !lootDetected && !lootInRange;
        var condition4 = player.Weapon == null && remainingDistance <= 0 && nearestWeapon != null && !isMoving && !rotating && !lootDetected && !lootInRange;
        var condition5 = player.Weapon == null && remainingDistance <= 0 && nearestWeapon != null && !isMoving && !rotating && !lootDetected && !lootInRange;
        var condition6 = player.Weapon == null && remainingDistance <= 0 && nearestWeapon != null && !isMoving && !rotating && !lootDetected && !lootInRange;

        if (condition1) 
        {
            RotateRandom();
            return;
        }

        if (condition2)
        {
            isMoving = false;
            return;
        }

        if (condition3)
        {
            SmoothRotate();
            return;
        }

        if (condition4)
        {
            var weapons = Scan(scanRange, lootLayerMask);
            if (weapons.Length > 0 && !gettingNearestObject)
            {
                GetNearest(ref nearestWeapon, weapons, transform.position);
            }
        }

        //if (player.Weapon == null && remainingDistance > 0 && nearestWeapon == null)
        //{
        //    var weapons = Scan(scanRange, lootLayerMask);
        //    if (weapons.Length > 0)
        //    {
        //        if (!gettingNearestObject)
        //        {
        //            nearestWeapon = null;
        //            GetNearest(ref nearestWeapon, weapons, transform.position);
        //        }
        //        if (nearestWeapon != null)
        //        {
        //            Move(nearestWeapon.transform.position);
        //        }
        //    }
        //    else
        //    {
        //        return;
        //    }
        //}
        //if (player.Weapon == null && nearestWeapon == null && lootDetected)
        //{
        //    var weapons = Scan(scanRange, lootLayerMask);
        //    if (weapons.Length > 0 && !gettingNearestObject)
        //    {
        //        GetNearest(ref nearestWeapon, weapons, transform.position);
        //    }
        //}

        if (condition5)
        {
            Move(nearestWeapon.transform.position);
        }

        if (condition6)
        {
            nearestWeapon?.Equip(player);
        }

        //if (player.Weapon == null && remainingDistance <= pickupRange && nearestWeapon != null)
        //{
        //    nearestWeapon?.Equip(player);
        //}
    }

    private void OnValidate()
    {
        Serialize();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = lootInRange ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
        Gizmos.color = enemyDetected | lootDetected ? Color.red : Color.blue;
        Gizmos.DrawWireSphere(transform.position, roamRange);
    }
    #endregion

    #region helper methods
    private void Serialize()
    {
        if (directionGuide != null)
            directionGuide.position = new Vector3(directionGuide.position.x, directionGuide.position.y, roamRange);
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
            isMoving = true;
        }
        else
        {
            RotateRandom();
        }
    }

    private void RotateRandom()
    {
        desiredRotationY = Mathf.RoundToInt(Random.Range(0, 360));
        rotating = true;
    }

    private void SmoothRotate()
    {
        currentRotationY = Mathf.RoundToInt(transform.localEulerAngles.y);
        if (desiredRotationY == currentRotationY)
        {
            Move();
            rotating = false;
        }
        else
        {
            var r = transform.rotation;
            var tor = Quaternion.Euler(new Vector3(r.x, desiredRotationY, r.z));
            transform.rotation = Quaternion.Lerp(transform.rotation, tor, Time.time * rotationLerp);
            rotating = true;
        }
    }

    private Collider[] Scan(float radius, int layerMask)
    {
        return Physics.OverlapSphere(transform.position, radius, layerMask).Where(x => x != collider).ToArray() ?? new Collider[0];
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
        lootDetected = Scan(roamRange, lootLayerMask.value).Length > 0 ? true : false;
        lootInRange = Scan(pickupRange, lootLayerMask.value).Length > 0 ? true : false;
        enemyDetected = Scan(roamRange, enemyLayerMask.value).Length > 0 ? true : false;
    }
    #endregion
}