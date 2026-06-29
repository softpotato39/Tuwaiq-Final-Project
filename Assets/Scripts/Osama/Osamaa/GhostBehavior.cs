using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// æÍÔ ÇáÎØæÇÊ: ÊÌæÇá Úáì ÇáÃÑÖ/ÇáÌÏÑÇä/ÇáÓŞİ + ÊíáíÈæÑÊ ŞÑíÈ ãä ÇááÇÚÈ + ÑÏ İÚá Úáì ÇáİáÇÔáÇíÊ.
/// ãÊØáÈÇÊ ÅÚÏÇÏ:
/// 1. Öíİå Úáì äİÓ ÇáÃæÈÌßÊ Çááí İíå Animator (ãËá Ch45_nonPBR).
/// 2. ÇÓÍÈ "player" ÈÇáÜ Inspector.
/// 3. (ÇÎÊíÇÑí) ÇÓÍÈ "flashlight" áæ ÊÈí ÑÏ İÚá ÚäÏ ÇáÅÖÇÁÉ Úáíå.
/// 4. ÚÔÇä íãÔí Úáì ÇáÌÏÑÇä/ÇáÓŞİ: áÇ ÊÖíİ NavMeshAgent (Ãæ ÚØøáå)¡ æÊÃßÏ Åä ÇáÌÏÑÇä æÇáÓŞİ ÚäÏåã Collider.
/// 5. ÇÓã ÇáÈæá ÈÇÑÇãÊÑ ÈÇáÃäãíÊÑ "walkBoolParam" íØÇÈŞ ÇáãæÌæÏ (ãËá Monster-Walking).
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

    [Header("ÇáãÑÇÌÚ")]
    public Transform player;
    public PurpleFlashlight flashlight;
    public Animator animator;
    public string walkBoolParam = "IsWalking";

    [Header("ÅÚÏÇÏÇÊ ÇáÊÌæÇá")]
    public float moveSpeed = 1.5f;
    public float wanderRadius = 12f;
    public float rotationSpeed = 4f;
    [Tooltip("ãÏì ÇáÒãä Èíä äŞØÉ æÃÎÑì æŞÊ ãÇ íŞİ ÇáæÍÔ")]
    public Vector2 wanderWaitRange = new Vector2(2f, 6f);

    [Header("ÇáãÔí Úáì ÇáÃÓØÍ (ÌÏÑÇä/ÓŞİ)")]
    [Tooltip("áæ ãİÚøá¡ ÇáæÍÔ íáÊÕŞ ÈÃŞÑÈ ÓØÍ (ÃÑÖíÉ/ÌÏÇÑ/ÓŞİ) æíÊÍÑß Úáíå ÈÏá ãÇ íÈŞì ËÇÈÊ Úáì ÇáÃÑÖíÉ İŞØ")]
    public bool enableSurfaceWalking = true;
    public float surfaceCheckDistance = 1.5f;
    public LayerMask surfaceMask = ~0;
    public float surfaceRotateSpeed = 8f;

    [Header("ÇáÊíáíÈæÑÊ ŞÑÈ ÇááÇÚÈ")]
    public bool enableTeleport = true;
    public Vector2 teleportIntervalRange = new Vector2(10f, 25f);
    public float minDistanceFromPlayer = 6f;
    public float maxDistanceFromPlayer = 14f;
    [Tooltip("áæ ÇáÒÇæíÉ ÃßÈÑ ãä åĞÇ íÚÊÈÑ ÈÚíÏ Úä äÙÑ ÇááÇÚÈ¡ íÕáÍ ááÊíáíÈæÑÊ")]
    public float avoidPlayerViewAngle = 50f;

    [Header("ÑÏ ÇáİÚá ÚäÏ ÇáİáÇÔáÇíÊ")]
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

    // ---------------- ÇáÊÌæÇá ----------------

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

    // ---------------- ãäØŞ ÇáãÔí Úáì ÇáÃÓØÍ (ÌÏÇÑ/ÓŞİ/ÃÑÖíÉ) ----------------

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

    // ---------------- ÇáÊÌãÏ (ÑÏ İÚá ÇáİáÇÔáÇíÊ) ----------------

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

    // ---------------- ÇáÊíáíÈæÑÊ ŞÑÈ ÇááÇÚÈ ----------------

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

    // ---------------- ÑÏ ÇáİÚá ÚäÏ ÇáİáÇÔáÇíÊ ----------------

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