using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Pathfinding;

public class BoarController : MonoBehaviour
{
    // Movement
    [Header("---Movement Settings---")]
    public float requiredRotationDistance;

    private Vector3 _lastPosition;
    private bool _waitingForMovement;

    // Enable AIPath rotation
    private float _enableRotationDistance;
    private Vector3 _currentStoppedPosition;

    // Grazing movement
    [Header("---Grazing Movement Settings---")]
    public bool canBeginGrazing = true;
    public bool canBeginWalking;

    [SerializeField]
    private float _grazeTime;

    // StandGround variables
    [Header("---StandGround Settings---")]
    public SphereCollider boarAwarenessSphere;

    // Basic attack
    [Header("---Basic Attack Settings---")]
    public SphereCollider boarAttackSphere;
    public bool isFollowingTarget;
    public float basicAttackRotationSpeed;

    private Vector3 _basicAttackDirection;

    // ReactToDistantDamage variables
    [Header("---ReactToDistantDamage Settings---")]
    public float tempCurrentHealth;
    public bool boarTakingDamage;

    // Abilities
    [Header("---Abilities---")]
    public bool hasActiveAbility;

    // Required components
    private AIPath _aiPath;
    private Boar _boar;
    private BoarTargetFinder _boarTargetFinder;
    private BoarAwareness _boarAwareness;

    private void Awake()
    {
        //_aiPath = GetComponent<AIPath>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _aiPath = GetComponent<AIPath>();
        _boarTargetFinder = GetComponentInChildren<BoarTargetFinder>();
        _boarAwareness = GetComponentInChildren<BoarAwareness>();
        _boar = GetComponent<Boar>();

        tempCurrentHealth = _boar.currentHealth;
    }

    // Update is called once per frame
    void Update()
    {
        CharacterDeath();

        // Character moving or standing still
        DetermineMovingOrStationary();

        // Character abilities
        UpdateAbilities();

        // Character settings
        UpdateCharacterSettings();

        // Follow the target
        FollowTarget();

        _boar.StandGround();

        _boar.GrazeAround();

        // Rotate to face target while attacking
        FaceTargetWhileAttacking();
    }

    // Update character's settings
    private void UpdateCharacterSettings()
    {
        // AIPath component controlls speed
        _aiPath.maxSpeed = _boar.movementSpeed;

        boarAttackSphere.radius = _boar.attackRange;
        boarAwarenessSphere.radius = _boar.awarenessRange;
    }
  
    // Determine whether the character is currently moving or standing still
    private void DetermineMovingOrStationary()
    {
        // Character is moving
        if (_lastPosition != gameObject.transform.position)
        {
            _enableRotationDistance = Vector3.Distance(_currentStoppedPosition, transform.position);

            // Give character time to start on new path before resuming _aiPath rotation
            if (_enableRotationDistance >= requiredRotationDistance)
            {
                _waitingForMovement = false;

                if (!hasActiveAbility)
                {
                    _aiPath.updateRotation = true;
                }

                if (canBeginWalking)
                {
                    canBeginWalking = false;

                    // Animation for Boar to begin walking
                    Debug.Log("Walking");
                    canBeginGrazing = true;
                }
            }
        }
        // Character is not moving
        else
        {
            if (!_waitingForMovement)
            {
                _waitingForMovement = true;

                // Used to establish when the character is moving
                _currentStoppedPosition = transform.position;

                if (canBeginGrazing)
                {
                    canBeginGrazing = false;

                    // Animation for Boar to begin grazing
                    Debug.Log("Grazing");
                    canBeginWalking = true;
                }
            }
        }
        // Used to establish when the character is moving
        _lastPosition = gameObject.transform.position;
    }

    // Follow target to attack
    public void FollowTarget()
    {
        // An attackable target has been clicked on
        if (isFollowingTarget && _boar.newAttackTarget != null)
        {
            // Move to attack target
            //movementTarget.transform.position = _boar.newAttackTarget.transform.position;
            _aiPath.destination = _boar.newAttackTarget.transform.position;
            _aiPath.SearchPath();

            // Begin attacking
            if (_boar.canBasicAttack)
            {
                _boar.BasicAttack();
            }
        }
    }

    // Rotate to face the target while standing ground
    public void FaceTargetWhileStandingGround()
    {
        if (!hasActiveAbility)
        {
            // Character rotates to face target when attacking
            if (_boar.standingGroundActive)
            {
                if (_boar.awarenessTarget != null)
                {
                    _basicAttackDirection = _boar.awarenessTarget.transform.position - transform.position;
                    _basicAttackDirection.y = 0;
                    Quaternion targetRotation = Quaternion.LookRotation(_basicAttackDirection);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, basicAttackRotationSpeed * Time.deltaTime);
                }
            }
            else
            {
                _aiPath.enableRotation = true;
            }
        }
    }

    // Rotate to face the target while attacking
    private void FaceTargetWhileAttacking()
    {
        if (!hasActiveAbility)
        {
            // Character rotates to face target when attacking
            if (_boar.basicAttackActive)
            {
                if (_boar.currentAttackTarget != null)
                {
                    _basicAttackDirection = _boar.currentAttackTarget.transform.position - transform.position;
                    _basicAttackDirection.y = 0;
                    Quaternion targetRotation = Quaternion.LookRotation(_basicAttackDirection);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, basicAttackRotationSpeed * Time.deltaTime);
                }
            }
            else
            {
                _aiPath.enableRotation = true;
            }
        }
    }

    // Certain parts of the Boar's abilities are called during Update()
    // Update Boar's abilities
    private void UpdateAbilities()
    {

    }

    // Boar death
    private void CharacterDeath()
    {
        if (_boar.currentHealth <= 0)
        {
            Debug.Log("Boar Died!");
            Destroy(this.gameObject);
        }
    }

    // Add to objects I want the Camera to follow
    public void OnMouseDown()
    {
        CameraController.instance.followTransform = transform;
    }
}