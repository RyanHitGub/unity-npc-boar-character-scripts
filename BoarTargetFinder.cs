using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Pathfinding;

public class BoarTargetFinder : MonoBehaviour
{
    // Targets for character's basic attack
    [Header("---Target Settings---")]
    public List<GameObject> potentialTargets;
    public List<GameObject> confirmedTargets;

    private Boar _boar;
    private BoarController _boarController;
    private AIPath _aiPath;

    private void Awake()
    {
        //_aiPath = GetComponentInParent<AIPath>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _aiPath = GetComponentInParent<AIPath>();
        _boar = GetComponentInParent<Boar>();
        _boarController = GetComponentInParent<BoarController>();
    }

    // Update is called once per frame
    void Update()
    {
        // Determine confirmed targets from potential targets
        CheckLineOfSight();
        // Stop follow movement when target is in attack range
        TargetInRange();
        // Start follow movement when target leaves attack range
        TargetOutOfRange();
    }

    // Get all possible targets for the Space Marine's basic attack
    // Targets enter the character's attack sphere
    private void OnTriggerEnter(Collider other)
    {
        if (other != null)
        {
            if (other.tag == "Player")
            {
                // Do not detect the character object (do not detect self)
                // Attack sphere and attack target are child objects of the character
                if (transform.parent != other.transform.parent)
                {
                    // Add recognised object to potential targets
                    potentialTargets.Add(other.transform.parent.gameObject);
                }
            }
        }
    }

    // Detect when target objects leave the Space Marine's basic attack range
    private void OnTriggerExit(Collider other)
    {
        if (other.tag != null)
        {
            if (other.tag == "Player")
            {
                // Ignore character object (ignore self)
                if (transform.parent != other.transform.parent)
                {
                    // Remove the out of range potential target
                    potentialTargets.Remove(other.transform.parent.gameObject);

                    bool canRemoveTarget = false;
                    // Search through confirmed targets to see if the exiting object is was a confirmed target
                    foreach (var target in confirmedTargets)
                    {
                        if (target == other.transform.parent.gameObject)
                        {
                            canRemoveTarget = true;
                        }
                    }

                    // Remove for confirmed targets
                    if (canRemoveTarget)
                    {
                        confirmedTargets.Remove(other.transform.parent.gameObject);

                        // Stop attacking current character
                        if (_boar.basicAttackActive && other.transform.parent.gameObject == _boar.currentAttackTarget)
                        {
                            //_boar.StopCoroutine(_boar.basicAttackCoroutine);
                            _boar.basicAttackActive = false;
                            Debug.Log("3 Stopped basic attack!");
                        }
                    }
                }
            }
        }
    }

    // Determine if a potential target is a confirmed target
    // Does the character have a clear line of sight at the potential target
    public void CheckLineOfSight()
    {
        // Search through potential targets
        foreach (var target in potentialTargets)
        {
            if (target != null)
            {
                // Send raycast from character to potential targets
                Vector3 rayDirection = target.transform.position - transform.position;
                Debug.DrawRay(transform.position, rayDirection, Color.cyan);
                RaycastHit hit;

                // Ignore layer 11 'Attack Spheres'
                int layerMask = 1 << 11;
                layerMask = ~layerMask;

                if (Physics.Raycast(transform.position, rayDirection, out hit, Mathf.Infinity, layerMask))
                {
                    Debug.Log(hit.transform.tag);

                    // The raycast hits "Player" when there are no obstractions
                    // Meaning a clear line of sight
                    if (hit.transform.tag == "Player")
                    {
                        bool canAddTarget = true;
                        // Search through confirmed targets to see if the potential target already exists
                        foreach (var t in confirmedTargets)
                        {
                            if (t == target)
                            {
                                canAddTarget = false;
                            }
                        }

                        // Add the target to confirmed targets
                        if (canAddTarget)
                        {
                            confirmedTargets.Add(target);
                        }
                    }
                    // The raycast hit something other than an attackable object
                    else
                    {
                        bool canRemoveTarget = false;
                        // Search through the confirmed targets for the target that is obstructed
                        foreach (var t in confirmedTargets)
                        {
                            if (t == target)
                            {
                                canRemoveTarget = true;
                            }
                        }

                        // Remove the target from confirmed targets
                        // It still exists in potential targets
                        if (canRemoveTarget)
                        {
                            confirmedTargets.Remove(target);

                            // Stop attacking current character
                            if (_boar.basicAttackActive)
                            {
                                //_boar.StopCoroutine(_boar.basicAttackCoroutine);
                                _boar.basicAttackActive = false;
                                Debug.Log("4 Stopped basic attack!");
                            }
                        }
                    }
                }
            }
        }
    }

    // Stop moving toward the target when the target is in range of the basic attack
    public void TargetInRange()
    {
        // Ignore if there is an active ability ie. Blade Dash
        if (!_boarController.hasActiveAbility)
        {
            // Search through the confirmed targets for the attack target
            foreach (var target in confirmedTargets)
            {
                if (target == _boar.newAttackTarget)
                {
                    //_testCharacterController.movementTarget.transform.position = _testCharacter.currentAttackTarget.transform.position;
                    //_seeker.CancelCurrentPathRequest();

                    _aiPath.updateRotation = false;
                    _aiPath.enableRotation = false;


                    Debug.Log("MOVE STOPPED");
                    // Clear the path
                    //_aiPath.SetPath(null);
                    // Stop pathfinding
                    //_aiPath.canSearch = false;
                    _aiPath.isStopped = true;
                    _aiPath.SetPath(null);

                    //StartCoroutine(StopWhenChasingRoutine());
                    //_aiPath.destination = _testCharacter.transform.position;

                    // Stop TestCharacter rotation
                    //_aiPath.updateRotation = false;
                    break;
                }
            }
        }
    }

    // Keep moving towards the target when the target leaves the basic attack range
    public void TargetOutOfRange()
    {
        if (_boarController.isFollowingTarget &&
            _boar.attackTargetFound &&
            _boar.currentAttackTarget != null &&
            !_boarController.hasActiveAbility)
        {
            // Resume pathfinding
            //_aiPath.canSearch = true;
            _aiPath.isStopped = false;
            //_aiPath.updateRotation = true;
        }
    }

    //private IEnumerator StopWhenChasingRoutine()
    //{
    //    yield return new WaitForSeconds(0.1f);
    //    _aiPath.SetPath(null);
    //}
}