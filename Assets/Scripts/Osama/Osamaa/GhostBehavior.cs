using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Ghost behavior: wanders on floor/walls/ceiling + teleports near the player + reacts to flashlight.
/// Setup requirements:
/// 1. Attach this to the same object that has the Animator (e.g. Ch45_nonPBR).
/// 2. Assign "player" in the Inspector.
/// 3. (Optional) Assign "flashlight" if you want a reaction when it's lit up.
/// 4. To walk on walls/ceiling: do NOT add a NavMeshAgent (or disable it), and make sure
///    walls and ceiling have Colliders.
/// 5. Make sure the walk bool parameter name "walkBoolParam" matches the Animator's
///    parameter (e.g. Monster-Walking).
/// </summary>
[RequireComponent(typeof(GhostGlitchController))]
public class GhostBehavior : MonoBehaviour
{
    private enum State
    {
        Wandering,
        Frozen,
        Teleporting
    }

    [Header("References")]
    public Transform player;
    public PurpleFlashlight flashlight;
    public Animator animator;
    public string walkBoolParam = "IsWalking";

    [Header("Wander Settings")]
    public float moveSpeed = 1.5f;
    public float wanderRadius = 12f;
    public float rotationSpeed = 4f;
    [Tooltip("Range of time between wander points while the ghost pauses")]
    public Vector2 wanderWaitRange = new Vector2(2f, 6f);

    [Header("Surface Walking (Walls/Ceiling)")]
    [Tooltip("If enabled, the ghost sticks to the nearest surface (floor/wall/ceiling) and moves along it instead of staying only on the floor")]
    public bool enableSurfaceWalking = true;
    public float surfaceCheckDistance = 1.5f;
    public LayerMask surfaceMask = ~0;
    public float surfaceRotateSpeed = 8f;

    [Header("Teleport Near Player")]
    public bool enableTeleport = true;
    public Vector2 teleportIntervalRange = new Vector2(10f, 25f);
    public float minDistanceFromPlayer = 6f;
    public float maxDistanceFromPlayer = 14f;
    [Tooltip("If the angle is greater than this, it's considered outside the player's view and valid for teleporting")]
    public float avoidPlayerViewAngle = 50f;

    [Header("Flashlight Reaction")]
    public bool reactToFlashlight = true;
    [Range(0f, 1f)] public float lightReactionChance = 0.6f;
    public float lightDetectionDistance = 18f;
    public float lightDetectionAngle = 25f;
    public float reactionCooldown = 4f;
    public bool teleportAwayOnLightReaction = true;
    public float freezeDurationOnReaction = 1.2f;

    private NavMeshAgent _agent;
    private GhostGlitchController _glitch;
    private State _state = State.Wandering;

    private Vector3 _wanderTarget;
    private Vector3 _wanderDirection;
    private float _wanderTimer;
    private float _teleportTimer;
    private float _reactionCooldownTimer;
    private float _freezeTimer;
    private float _flashlightFindTimer;

    private Vector3 _currentNormal = Vector3.up;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _glitch = GetComponent<GhostGlitchController>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Start()
    {
        if (enableSurfaceWalking)
        {
            PickNewWanderDirection();
        }
        else
        {
            PickNewWanderTarget();
        }
        ResetTeleportTimer();
    }

    private void Update()
    {
        // The flashlight is spawned at runtime when the player equips the tool,
        // so it isn't assignable in the Inspector. Auto-find it (keep retrying).
        if (flashlight == null && reactToFlashlight)
        {
            _flashlightFindTimer -= Time.deltaTime;
            if (_flashlightFindTimer <= 0f)
            {
                flashlight = FindFirstObjectByType<PurpleFlashlight>();
                _flashlightFindTimer = 0.3f;
            }
        }

        if (_reactionCooldownTimer > 0f)
        {
            _reactionCooldownTimer -= Time.deltaTime;
        }

        if (enableSurfaceWalking && _agent == null)
        {
            AlignToSurface();
        }

        switch (_state)
        {
            case State.Wandering:
                UpdateWandering();
                break;

            case State.Frozen:
                UpdateFrozen();
                break;

            case State.Teleporting:
                break;
        }

        if (reactToFlashlight && flashlight != null && _reactionCooldownTimer <= 0f)
        {
            CheckFlashlightReaction();
        }

        if (enableTeleport && _state == State.Wandering)
        {
            _teleportTimer -= Time.deltaTime;
            if (_teleportTimer <= 0f)
            {
                DoTeleportNearPlayer();
                ResetTeleportTimer();
            }
        }
    }

    // ---------------- Wandering ----------------

    private void UpdateWandering()
    {
        SetWalking(true);

        if (enableSurfaceWalking && _agent == null)
        {
            UpdateSurfaceWandering();
            return;
        }

        if (_agent != null && _agent.enabled)
        {
            _agent.speed = moveSpeed;
            _agent.SetDestination(_wanderTarget);

            if (_agent.remainingDistance <= _agent.stoppingDistance && !_agent.pathPending)
            {
                WaitThenPickNewTarget();
            }
        }
        else
        {
            Vector3 toTarget = _wanderTarget - transform.position;
            toTarget.y = 0f;

            if (toTarget.magnitude <= 0.3f)
            {
                WaitThenPickNewTarget();
            }
            else
            {
                Vector3 dir = toTarget.normalized;
                transform.position += dir * moveSpeed * Time.deltaTime;

                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }
    }

    // ---------------- Surface walking logic (wall/ceiling/floor) ----------------

    private void AlignToSurface()
    {
        Vector3 origin = transform.position + transform.up * 0.1f;

        if (Physics.Raycast(origin, -transform.up, out RaycastHit hit, surfaceCheckDistance, surfaceMask))
        {
            _currentNormal = hit.normal;
        }
        else
        {
            Vector3[] dirs = { transform.forward, -transform.forward, transform.right, -transform.right };
            foreach (var d in dirs)
            {
                Vector3 o = transform.position + d * 0.3f;
                if (Physics.Raycast(o, -transform.up, out RaycastHit h2, surfaceCheckDistance, surfaceMask))
                {
                    _currentNormal = h2.normal;
                    break;
                }
            }
        }

        Quaternion targetRot = Quaternion.FromToRotation(transform.up, _currentNormal) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, surfaceRotateSpeed * Time.deltaTime);
    }

    private void UpdateSurfaceWandering()
    {
        Quaternion lookRot = Quaternion.LookRotation(_wanderDirection, _currentNormal);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotationSpeed * Time.deltaTime);
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        _wanderTimer -= Time.deltaTime;
        if (_wanderTimer <= 0f)
        {
            WaitThenPickNewDirection();
        }
    }

    private void WaitThenPickNewDirection()
    {
        SetWalking(false);
        PickNewWanderDirection();
    }

    private void PickNewWanderDirection()
    {
        Vector3 randomTangent = Vector3.ProjectOnPlane(Random.onUnitSphere, _currentNormal).normalized;
        if (randomTangent.sqrMagnitude < 0.01f)
        {
            randomTangent = transform.forward;
        }
        _wanderDirection = randomTangent;
        _wanderTimer = Random.Range(wanderWaitRange.x, wanderWaitRange.y);
    }

    private void WaitThenPickNewTarget()
    {
        _wanderTimer -= Time.deltaTime;
        SetWalking(false);

        if (_wanderTimer <= 0f)
        {
            PickNewWanderTarget();
        }
    }

    private void PickNewWanderTarget()
    {
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        Vector3 candidate = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

        if (_agent != null && _agent.enabled && NavMesh.SamplePosition(candidate, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
        {
            candidate = hit.position;
        }

        _wanderTarget = candidate;
        _wanderTimer = Random.Range(wanderWaitRange.x, wanderWaitRange.y);
    }

    private void SetWalking(bool isWalking)
    {
        if (animator != null && !string.IsNullOrEmpty(walkBoolParam))
        {
            animator.SetBool(walkBoolParam, isWalking);
        }
    }

    // ---------------- Freeze (flashlight reaction) ----------------

    private void UpdateFrozen()
    {
        SetWalking(false);

        if (player != null)
        {
            Vector3 lookDir = player.position - transform.position;
            lookDir = Vector3.ProjectOnPlane(lookDir, _currentNormal);
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir.normalized, _currentNormal);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }

        _freezeTimer -= Time.deltaTime;
        if (_freezeTimer <= 0f)
        {
            if (teleportAwayOnLightReaction)
            {
                DoTeleportNearPlayer();
            }
            _state = State.Wandering;
            if (enableSurfaceWalking && _agent == null)
                PickNewWanderDirection();
            else
                PickNewWanderTarget();
        }
    }

    // ---------------- Teleport near player ----------------

    private void ResetTeleportTimer()
    {
        _teleportTimer = Random.Range(teleportIntervalRange.x, teleportIntervalRange.y);
    }

    private void DoTeleportNearPlayer()
    {
        if (player == null)
        {
            return;
        }

        Vector3 newPos = FindTeleportPosition();
        transform.position = newPos;

        if (_agent != null && _agent.enabled)
        {
            _agent.Warp(newPos);
        }

        if (_glitch != null)
        {
            _glitch.TriggerBurst();
        }

        _state = State.Wandering;
        if (enableSurfaceWalking && _agent == null)
            PickNewWanderDirection();
        else
            PickNewWanderTarget();
    }

    private Vector3 FindTeleportPosition()
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);

            Vector3 offset = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * distance;
            Vector3 candidate = player.position + offset;

            Vector3 toCandidate = (candidate - player.position).normalized;
            float viewAngle = Vector3.Angle(player.forward, toCandidate);

            if (viewAngle > avoidPlayerViewAngle)
            {
                if (_agent != null && _agent.enabled && NavMesh.SamplePosition(candidate, out NavMeshHit hit, maxDistanceFromPlayer, NavMesh.AllAreas))
                {
                    return hit.position;
                }
                return candidate;
            }
        }

        return player.position - player.forward * minDistanceFromPlayer;
    }

    // ---------------- Flashlight reaction ----------------

    private void CheckFlashlightReaction()
    {
        if (!flashlight.IsOn)
        {
            return;
        }

        Transform lightTransform = flashlight.transform;
        Vector3 toGhost = transform.position - lightTransform.position;
        float distance = toGhost.magnitude;

        if (distance > lightDetectionDistance)
        {
            return;
        }

        float angle = Vector3.Angle(lightTransform.forward, toGhost);
        if (angle > lightDetectionAngle)
        {
            return;
        }

        if (Random.value > lightReactionChance)
        {
            return;
        }

        _reactionCooldownTimer = reactionCooldown;

        if (_glitch != null)
        {
            _glitch.TriggerBurst();
        }

        if (freezeDurationOnReaction > 0f)
        {
            _state = State.Frozen;
            _freezeTimer = freezeDurationOnReaction;
        }
        else if (teleportAwayOnLightReaction)
        {
            DoTeleportNearPlayer();
        }
    }
}