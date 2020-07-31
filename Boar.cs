using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Pathfinding;

public class Boar : MonoBehaviour
{
    // Character settings
    [Header("---Boar Settings---")]
    public string characterType = "Boar";
    public float maxHealth = 100;
    public float currentHealth = 100;
    public float healthRegen = 1;
    public float maxEnergy = 100;
    public float currentEnergy = 100;
    public float armour = 0;
    public float maxShield = 0;
    public float currentShield = 0;
    public float movementSpeed = 6;
    public float chargeSpeed = 9;
    public float grazeAroundSpeed = 2;
    public float attackDamage = 2;
    public float attackRange = 1.5f;
    public float attackSpeed = 1;
    public float awarenessRange = 10;

    // Basic Attack settings
    [Header("---Basic Attack Settings---")]
    //public GameObject attackSphere;
    public GameObject newAttackTarget;
    public GameObject currentAttackTarget;
    // Character is currently attacking a target
    public bool basicAttackActive;
    // Target has moved out of range and character should follow
    public bool attackTargetFound;
    public Coroutine basicAttackCoroutine = null;
    // Controls when the boar can damage its target
    public bool canBasicAttack = true;

    // Standing Ground settings
    [Header("---Standing Ground Settings---")]
    public GameObject awarenessTarget;
    public bool standingGroundActive;
    public float chargeDistance;
    public float targetDistance;

    // Graze Around setting 
    [Header("---Grazing Settings---")]
    public bool startGrazingCountdown;
    public float grazingCooldown;
    public float grazingCountdown;
    public bool canGrazeNewLocation = true;
    public float grazingRadius;
    public Coroutine grazeAroundCooldownRoutine = null;

    private GameObject _destroyedTarget = null;

    private BoarTargetFinder _boarTargetFinder;
    private AIPath _aiPath;
    private BoarController _boarController;
    private BoarAwareness _boarAwareness;

    private void Awake()
    {
        //_aiPath = GetComponent<AIPath>();
    }

    private void Start()
    {
        _aiPath = GetComponent<AIPath>();
        _boarTargetFinder = GetComponentInChildren<BoarTargetFinder>();
        _boarController = GetComponent<BoarController>();
        _boarAwareness = GetComponentInChildren<BoarAwareness>();
    }

    // Boar's basic attack
    public void BasicAttack()
    {
        // Search through confirmed targets for the attack target
        foreach (var target in _boarTargetFinder.confirmedTargets)
        {
            if (target == newAttackTarget)
            {
                currentAttackTarget = newAttackTarget;
                attackTargetFound = true;

                // Character begins its basic attack routine
                basicAttackCoroutine = StartCoroutine(BasicAttackRoutine(currentAttackTarget));
                canBasicAttack = false;
                basicAttackActive = true;
                Debug.Log("Boar basic attack!");
                break;
            }
            attackTargetFound = false;
        }
    }

    // Basic attack cooldown
    public IEnumerator BasicAttackRoutine(GameObject target)
    {
        // Wait time in seconds between attacks
        float basicAttacksPerSecond;
        basicAttacksPerSecond = 60 / attackSpeed / 60;

        while (true)
        {
            yield return new WaitForSeconds(basicAttacksPerSecond);

            // Ignore if there is an active ability ie. Blade Dash
            if (!_boarController.hasActiveAbility)
            {
                // Check if the target is dead
                if (target == null)
                {
                    Debug.Log("Target DEAD!");

                    _destroyedTarget = null;
                    // Find destroyed target in target lists
                    foreach (var t in _boarTargetFinder.confirmedTargets)
                    {
                        if (t == target)
                        {
                            _destroyedTarget = t;
                        }
                    }

                    // Remove destroyed target from target lists
                    _boarTargetFinder.confirmedTargets.Remove(_destroyedTarget);
                    _boarTargetFinder.potentialTargets.Remove(_destroyedTarget);
                    _boarAwareness.confirmedAwarenessTargets.Remove(_destroyedTarget);
                    _boarAwareness.potentialAwarenessTargets.Remove(_destroyedTarget);

                    // Clear attack targets
                    newAttackTarget = null;
                    currentAttackTarget = null;

                    // Finish attacking
                    basicAttackActive = false;
                    Debug.Log("5 Stopped basic attack!");

                    // Resume pathfinding
                    _aiPath.canSearch = true;

                    // Movement target set to current position
                    _aiPath.destination = gameObject.transform.position;
                    _aiPath.SearchPath();

                    canBasicAttack = true;
                    break;
                }

                Debug.Log("Boar Attack!");
                CharacterID characterID = target.GetComponent<CharacterID>();
                int targetID = characterID.characterID;
                // Switch between Character scripts to access the target's current health
                switch (targetID)
                {
                    case -1:
                        Boar boar = target.GetComponent<Boar>();
                        boar.currentHealth -= attackDamage;
                        break;
                    case 0:
                        TestCharacter testCharacter = target.GetComponent<TestCharacter>();
                        testCharacter.currentHealth -= attackDamage;
                        break;
                    case 1:
                        TrainingDummy trainingDummy = target.GetComponent<TrainingDummy>();
                        trainingDummy.currentHealth -= attackDamage;
                        break;
                    default:
                        break;
                }
            }
            canBasicAttack = true;
            break;
        }
    }

    // Boar's default movement
    public void GrazeAround()
    {
        if (canGrazeNewLocation)
        {
            canGrazeNewLocation = false;

            Debug.Log("graze routine");
            grazeAroundCooldownRoutine = StartCoroutine(GrazeAroundCooldownRoutine());

            movementSpeed = grazeAroundSpeed;
            _aiPath.destination = GrazingPoint();
            _aiPath.SearchPath();
        }
    }

    // Reset all Grazing Around settings
    public void StopGrazingAround()
    {
        StopCoroutine(grazeAroundCooldownRoutine);
        canGrazeNewLocation = false;
    }

    // Graze around cooldown
    public IEnumerator GrazeAroundCooldownRoutine()
    {
        grazingCooldown = Random.Range(5, 11);
        grazingCountdown = grazingCooldown;
        while (true)
        {
            yield return new WaitForSeconds(1.0f);
            grazingCountdown--;
            if (grazingCountdown <= 0)
            {
                canGrazeNewLocation = true;
                break;
            }
        }
    }

    // The random location the boar will graze at
    private Vector3 GrazingPoint()
    {
        Vector3 point = Random.insideUnitSphere * grazingRadius;
        point.y = 0;
        point += _aiPath.position;
        return point;
    }

    // Boar is standing ground and ready to attack target if it moves closer
    public void StandGround()
    {
        // A target is close to the boar
        if (_boarAwareness.confirmedAwarenessTargets.Count > 0)
        {
            standingGroundActive = true;

            StopGrazingAround();

            // Stop moving on the grazing path
            if (!_boarController.isFollowingTarget)
            {
                _aiPath.destination = transform.position;
                _aiPath.SearchPath();
            }

            // Allows quick rotation to face target
            _aiPath.updateRotation = false;
            _aiPath.enableRotation = false;

            // The first target to move into the boar's awareness range
            awarenessTarget = _boarAwareness.confirmedAwarenessTargets[0];

            // Rotate to face target
            _boarController.FaceTargetWhileStandingGround();

            // If anyone gets too close to the boar, it will charge
            foreach (var target in _boarAwareness.confirmedAwarenessTargets)
            {
                if (target != null)
                {
                    targetDistance = Vector3.Distance(target.transform.position, transform.position);

                    if (targetDistance <= chargeDistance)
                    {
                        Debug.Log("Too Close!");

                        // Set charge speed
                        movementSpeed = chargeSpeed;

                        //// Charge the closest target
                        //_boar.newAttackTarget = target;
                        // Charge the first target that alerted the boar
                        newAttackTarget = _boarAwareness.confirmedAwarenessTargets[0];

                        // Boar will attack the target
                        _boarController.isFollowingTarget = true;
                    }
                }
            }
        }
        else
        {
            // Target left the boar's awareness range
            if (standingGroundActive)
            {
                // Start grazing again
                canGrazeNewLocation = true;
            }
            standingGroundActive = false;
            awarenessTarget = null;
        }
    }

    // Boar will charge at any character that attacks it.
    public void ChargeAtThreat(GameObject target)
    {
        StopGrazingAround();

        movementSpeed = chargeSpeed;
        newAttackTarget = target;
        _boarController.isFollowingTarget = true;
    }
}