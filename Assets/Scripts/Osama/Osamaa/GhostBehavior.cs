using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Óáæß ÇáæÍÔ ÇáÃÓæÏ: ãÒíÌ Èíä ÊãÔíÉ ÚÔæÇÆíÉ + ÊíáíÈæÑÊ ŞÑíÈ ãä ÇááÇÚÈ + ÑÏ İÚá
/// áãÇ ÇáİáÇÔ áÇíÊ íÖæí Úáíå. åĞÇ ÇáÓßÑÈÊ áÇ íÃËÑ Úáì ÇááÇÚÈ ÃÈÏÇğ (ÈÏæä ÖÑÑ/Game Over)¡
/// ßáå ÊÃËíÑÇÊ ÌæíÉ/ÑÚÈ ÈÕÑí ÈÓ.
///
/// ØÑíŞÉ ÇáÅÚÏÇÏ:
/// 1. Öíİå Úáì ÇáÃæÈÌßÊ ÇáÌĞÑ ááæÍÔ (äİÓ ãÓÊæì Animator æ Ch45_nonPBR).
/// 2. ÇÑÈØ "player" ÈÊÑÇäÓİæÑã ÇááÇÚÈ.
/// 3. (ÇÎÊíÇÑí) ÇÑÈØ "flashlight" ÈÓßÑÈÊ PurpleFlashlight ÚÔÇä íÊİÇÚá æŞÊ ÇáÅÖÇÁÉ Úáíå.
/// 4. áæ ÚäÏß NavMesh ãÈíøß ÈÇáÓíä¡ Öíİ NavMeshAgent Úáì äİÓ ÇáÃæÈÌßÊ æÑÇÍ íÓÊÎÏãå
///    ÊáŞÇÆíÇğ ááÊäŞá ÇáĞßí. áæ ãÇÚäÏß NavMesh¡ íÊÍÑß ÈÎØ ãÈÇÔÑ ÈÔßá ÊáŞÇÆí ÈÏæä ãÔÇßá.
/// 5. ÊÃßÏ Åä ÇÓã ÇáÈÑÇãíÊÑ "walkBoolParam" íØÇÈŞ ÇáÈÑÇãíÊÑ ÇáãæÌæÏ ÈÇáÜ Animator ÇáÎÇÕ Èß
///    (Çááí íÔÛøá Monster-Walking).
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

    [Header("ÇáÑÈØ")]
    public Transform player;
    public PurpleFlashlight flashlight;
    public Animator animator;
    public string walkBoolParam = "IsWalking";

    [Header("ÇáÊãÔíÉ ÇáÚÔæÇÆíÉ")]
    public float moveSpeed = 1.5f;
    public float wanderRadius = 12f;
    public float rotationSpeed = 4f;
    [Tooltip("ÃŞá æÃßËÑ æŞÊ íäÊÙÑå ŞÈá ãÇ íÎÊÇÑ æÌåÉ ÌÏíÏÉ")]
    public Vector2 wanderWaitRange = new Vector2(2f, 6f);

    [Header("ÇáÊíáíÈæÑÊ ŞÑíÈ ãä ÇááÇÚÈ")]
    public bool enableTeleport = true;
    public Vector2 teleportIntervalRange = new Vector2(10f, 25f);
    public float minDistanceFromPlayer = 6f;
    public float maxDistanceFromPlayer = 14f;
    [Tooltip("áæ ÒÇæíÉ ÇááÇÚÈ Úä äŞØÉ ÇáÊíáíÈæÑÊ ÃŞá ãä åĞÇ¡ íÚÊÈÑ ÈãÌÇá äÙÑå æíÊÌäÈåÇ")]
    public float avoidPlayerViewAngle = 50f;

    [Header("ÑÏ ÇáİÚá Úáì ÇáİáÇÔ áÇíÊ")]
    public bool reactToFlashlight = true;
    [Range(0f, 1f)] public float lightReactionChance = 0.6f;
    public float lightDetectionDistance = 18f;
    public float lightDetectionAngle = 25f;
    public float reactionCooldown = 4f;
    [Tooltip("ÈÚÏ ãÇ íÊİÇÌÃ ãä ÇáÖæÁ¡ íÎÊİí æíØáÚ ÈãßÇä ËÇäí")]
    public bool teleportAwayOnLightReaction = true;
    [Tooltip("ÈÏá ÇáÊíáíÈæÑÊ¡ íÊÌãÏ æíÍÏøŞ ÈÇááÇÚÈ áÍÙÇÊ ŞÈá áÇ íÎÊİí (íÑİÚ ÇáÑÚÈ)")]
    public float freezeDurationOnReaction = 1.2f;

    private NavMeshAgent _agent;
    private GhostGlitchController _glitch;
    private State _state = State.Wandering;

    private Vector3 _wanderTarget;
    private float _wanderTimer;
    private float _teleportTimer;
    private float _reactionCooldownTimer;
    private float _freezeTimer;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>(); // ÇÎÊíÇÑí¡ ããßä íßæä null
        _glitch = GetComponent<GhostGlitchController>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Start()
    {
        PickNewWanderTarget();
        ResetTeleportTimer();
    }

    private void Update()
    {
        if (_reactionCooldownTimer > 0f)
        {
            _reactionCooldownTimer -= Time.deltaTime;
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
                // ÍÇáÉ áÍÙíÉ¡ ÇáÊÍæíá íÕíÑ İæÑÇğ ÏÇÎá DoTeleport
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

    // ---------------- ÇáÊãÔíÉ ÇáÚÔæÇÆíÉ ----------------

    private void UpdateWandering()
    {
        SetWalking(true);

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

    // ---------------- ÇáÊÌãÏ (ŞÈá ÇáÇÎÊİÇÁ) ----------------

    private void UpdateFrozen()
    {
        SetWalking(false);

        if (player != null)
        {
            Vector3 lookDir = player.position - transform.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir.normalized);
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
            PickNewWanderTarget();
        }
    }

    // ---------------- ÇáÊíáíÈæÑÊ ŞÑíÈ ãä ÇááÇÚÈ ----------------

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
        PickNewWanderTarget();
    }

    private Vector3 FindTeleportPosition()
    {
        // äÍÇæá áÚÏÉ ãÑÇÊ äáŞì äŞØÉ ÈÚíÏÉ Úä ãÌÇá äÙÑ ÇááÇÚÈ ÇáãÈÇÔÑ
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

        // áæ İÔáÊ ßá ÇáãÍÇæáÇÊ¡ íÑÌÚ äŞØÉ ÈÓíØÉ Îáİ ÇááÇÚÈ
        return player.position - player.forward * minDistanceFromPlayer;
    }

    // ---------------- ÑÏ ÇáİÚá Úáì ÇáİáÇÔ áÇíÊ ----------------

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

        // ÇáæÍÔ ãßÔæİ ÈÇáÖæÁ ÇáÂä
        if (Random.value > lightReactionChance)
        {
            return; // ãÇ ÑÏ¡ íÊÌÇåá åĞí ÇáãÑÉ ÚÔæÇÆíÇğ ÚÔÇä ãÇíÕíÑ ãÊæŞÚ
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
