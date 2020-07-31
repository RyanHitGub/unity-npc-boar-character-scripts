using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Pathfinding;

public class BoarAwareness : MonoBehaviour
{
    // Targets for character's basic attack
    [Header("---Target Settings---")]
    public List<GameObject> potentialAwarenessTargets;
    public List<GameObject> confirmedAwarenessTargets;

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
        CheckLineOfSight();
    }

    // Get all possible targets in range of the boar's sensors
    // Targets enter the boar's awareness sphere
    private void OnTriggerEnter(Collider other)
    {
        if (other != null)
        {
            if (//other.tag == "Boar" ||
                other.tag == "Player")
            {
                // Do not detect the character object (do not detect self)
                // Attack sphere and attack target are child objects of the character
                if (transform.parent != other.transform.parent)
                {
                    // Add recognised object to potential targets
                    potentialAwarenessTargets.Add(other.transform.parent.gameObject);
                }
            }
        }
    }

    // Detect when target objects leave the boar's basic attack range
    private void OnTriggerExit(Collider other)
    {
        if (other.tag != null)
        {
            if (//other.tag == "Boar" ||
                other.tag == "Player")
            {
                // Ignore character object (ignore self)
                if (transform.parent != other.transform.parent)
                {
                    // Remove the out of range potential target
                    potentialAwarenessTargets.Remove(other.transform.parent.gameObject);

                    bool canRemoveTarget = false;
                    // Search through confirmed targets to see if the exiting object is was a confirmed target
                    foreach (var target in confirmedAwarenessTargets)
                    {
                        if (target == other.transform.parent.gameObject)
                        {
                            canRemoveTarget = true;
                        }
                    }

                    // Remove for confirmed targets
                    if (canRemoveTarget)
                    {
                        confirmedAwarenessTargets.Remove(other.transform.parent.gameObject);
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
        foreach (var target in potentialAwarenessTargets)
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
                    if (//hit.transform.tag == "Boar" ||
                        hit.transform.tag == "Player")
                    {
                        bool canAddTarget = true;
                        // Search through confirmed targets to see if the potential target already exists
                        foreach (var t in confirmedAwarenessTargets)
                        {
                            if (t == target)
                            {
                                canAddTarget = false;
                            }
                        }

                        // Add the target to confirmed targets
                        if (canAddTarget)
                        {
                            confirmedAwarenessTargets.Add(target);
                        }
                    }
                    // The raycast hit something other than an attackable object
                    else
                    {
                        bool canRemoveTarget = false;
                        // Search through the confirmed targets for the target that is obstructed
                        foreach (var t in confirmedAwarenessTargets)
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
                            confirmedAwarenessTargets.Remove(target);
                        }
                    }
                }
            }
        }
    }
}