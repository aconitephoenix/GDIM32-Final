using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class freddyAI : MonoBehaviour
{
    // AI States
    public enum FreddyAIState
    {
        Inactive,       // Before activation
        Roaming,        // Stalking/roaming behavior
        Charging,       // Chasing the player
        Jumpscare       // Jumpscare animation
    }

    [Header("AI Activation")]
    [SerializeField] private float _activationDelay = 5f; // Time before Freddy activates after quest starts
    [SerializeField] private bool _allowDebugActivation = true; // Allow pressing P to activate
    private bool _isActive = false;
    private Coroutine _activationCoroutine;

    [Header("AI Components")]
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Animator _animator;
    private FreddyAIState _currentState = FreddyAIState.Inactive;

    [Header("Roaming Settings")]
    [SerializeField] private float _roamRadius = 30f; // How far Freddy can roam
    [SerializeField] private float _roamWaitTime = 3f; // Time to wait at each roam point
    [SerializeField] private float _roamSpeed = 2f;
    private Vector3 _roamTarget;
    private float _roamTimer;

    [Header("Charging Settings")]
    [SerializeField] private float _chargeDistance = 15f; // Distance at which Freddy starts charging
    [SerializeField] private float _chargeSpeed = 6f;
    [SerializeField] private float _losePlayerDistance = 25f; // Distance at which Freddy loses the player

    [Header("Jumpscare Settings")]
    [SerializeField] private float _jumpscareDistance = 2f; // Distance to trigger jumpscare
    [SerializeField] private float _jumpscareDuration = 3f; // How long the jumpscare lasts

    [Header("Debug Settings")]
    [SerializeField] private bool _enableDebugLogs = true; // Toggle debug logs on/off

    // Animation parameter names (adjust these to match your animator)
    private readonly string ANIM_IDLE = "idle";
    private readonly string ANIM_RUNNING = "running";
    private readonly string ANIM_JUMPSCARE = "jumpscare";

    private Transform _player;
    private float _diagnosticTimer = 0f;
    private Vector3 _initialModelLocalPosition; // Store initial local position of model
    private bool _hasChildModel = false; // Track if we have a separate child model

    void Start()
    {
        // Get player reference
        if (GameController.Instance != null && GameController.Instance.Player != null)
        {
            _player = GameController.Instance.Player.transform;
            DebugLog("Player reference found at position: " + _player.position);
        }
        else
        {
            Debug.LogError("Freddy AI: Could not find player reference!");
        }

        // Get NavMeshAgent if not assigned (should be on THIS object - the parent)
        if (_agent == null)
        {
            _agent = GetComponent<NavMeshAgent>();
            if (_agent != null)
            {
                DebugLog("NavMeshAgent component found on parent.");
            }
            else
            {
                Debug.LogError("Freddy AI: NavMeshAgent component missing!");
            }
        }

        // Get Animator if not assigned (might be on child)
        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
            if (_animator != null)
            {
                DebugLog("Animator component found (on " + _animator.gameObject.name + ")");
            }
            else
            {
                Debug.LogWarning("Freddy AI: Animator component missing! Animations will not play.");
            }
        }


        // Check NavMeshAgent status
        if (_agent != null)
        {
            DebugLog("=== NavMeshAgent Detailed Status ===");
            DebugLog("  - Is On NavMesh: " + _agent.isOnNavMesh);
            DebugLog("  - Is Stopped: " + _agent.isStopped);
            DebugLog("  - Speed: " + _agent.speed);
            DebugLog("  - Enabled: " + _agent.enabled);
            DebugLog("  - Update Position: " + _agent.updatePosition);
            DebugLog("  - Update Rotation: " + _agent.updateRotation);
            DebugLog("  - Stopping Distance: " + _agent.stoppingDistance);
            DebugLog("  - Auto Braking: " + _agent.autoBraking);
            DebugLog("  - Radius: " + _agent.radius);
            DebugLog("  - Height: " + _agent.height);
            DebugLog("  - Base Offset: " + _agent.baseOffset);
            DebugLog("  - Obstacle Avoidance Type: " + _agent.obstacleAvoidanceType);
            
            if (!_agent.isOnNavMesh)
            {
                Debug.LogError("Freddy AI: NavMeshAgent is NOT on a NavMesh! Make sure you've baked a NavMesh in Window > AI > Navigation.");
                Debug.LogError("Freddy's current position: " + transform.position);
            }

            // IMPORTANT: Set NavMeshAgent to control position but NOT rotation (we'll handle it manually)
            _agent.updatePosition = true;
            _agent.updateRotation = false; // We'll handle rotation manually
            
            // Set reasonable defaults if they're problematic
            if (_agent.stoppingDistance < 0.1f || _agent.stoppingDistance > 2f)
            {
                _agent.stoppingDistance = 0.5f;
                DebugLog("  - Adjusted Stopping Distance to 0.5");
            }
        }

        // Check Animator status and parameters
        if (_animator != null)
        {
            DebugLog("=== Animator Status ===");
            DebugLog("  - Has Controller: " + (_animator.runtimeAnimatorController != null));
            DebugLog("  - Enabled: " + _animator.enabled);
            DebugLog("  - Apply Root Motion: " + _animator.applyRootMotion);
            
            // CRITICAL: Disable root motion so NavMeshAgent can move the character
            if (_animator.applyRootMotion)
            {
                _animator.applyRootMotion = false;
                DebugLog("  - Disabled Apply Root Motion to allow NavMeshAgent control");
            }
            
            if (_animator.runtimeAnimatorController != null)
            {
                DebugLog("  - Available Parameters:");
                foreach (AnimatorControllerParameter param in _animator.parameters)
                {
                    DebugLog("    * " + param.name + " (Type: " + param.type + ")");
                }
            }
            else
            {
                Debug.LogError("Freddy AI: Animator has no controller assigned!");
            }
        }

        // Set initial agent speed
        if (_agent != null)
        {
            _agent.speed = _roamSpeed;
        }

        DebugLog("=== Freddy AI Initialized ===");
        DebugLog("Current state: " + _currentState);
        DebugLog("Parent (Agent) position: " + transform.position);
        DebugLog("Parent rotation: " + transform.rotation.eulerAngles);
        DebugLog("Has Child Model: " + _hasChildModel);

        // Start checking for quest activation
        StartCoroutine(CheckForQuestStart());
    }

    void Update()
    {
        // Check for debug activation key press (P key)
        if (_allowDebugActivation && !_isActive && Input.GetKeyDown(KeyCode.P))
        {
            DebugLog("Freddy AI manually activated with P key!");
            ActivateNow();
        }

        if (!_isActive || _player == null) return;



        // Periodic diagnostic output (every 2 seconds)
        _diagnosticTimer += Time.deltaTime;
        if (_diagnosticTimer >= 2f && _agent != null)
        {
            _diagnosticTimer = 0f;
            DebugLog("=== Movement Diagnostics ===");
            DebugLog("  Parent Position: " + transform.position);
            DebugLog("  Parent Rotation: " + transform.rotation.eulerAngles);
            DebugLog("  Velocity: " + _agent.velocity + " (magnitude: " + _agent.velocity.magnitude.ToString("F2") + ")");
            DebugLog("  Desired Velocity: " + _agent.desiredVelocity + " (magnitude: " + _agent.desiredVelocity.magnitude.ToString("F2") + ")");
            DebugLog("  Has Path: " + _agent.hasPath);
            DebugLog("  Path Status: " + _agent.pathStatus);
            DebugLog("  Remaining Distance: " + _agent.remainingDistance.ToString("F2"));
            DebugLog("  Is Stopped: " + _agent.isStopped);
        }

        // Update behavior based on current state
        switch (_currentState)
        {
            case FreddyAIState.Roaming:
                UpdateRoaming();
                break;
            case FreddyAIState.Charging:
                UpdateCharging();
                break;
            case FreddyAIState.Jumpscare:
                // Jumpscare is handled by coroutine
                break;
        }
    }


    // Check if player has started collecting pages (page quest started)
    private IEnumerator CheckForQuestStart()
    {
        while (!_isActive)
        {
            // Activate when player has collected at least 1 page (quest has started) but less than 7
            if (GameController.Instance != null && 
                GameController.Instance.Player != null && 
                GameController.Instance.Player._currentPageCount > 0 &&
                GameController.Instance.Player._currentPageCount < GameController.Instance.Player._maxPageCount - 1)
            {
                // Page quest has started, activate Freddy after delay
                DebugLog("Page quest started! Freddy will activate in " + _activationDelay + " seconds.");
                _activationCoroutine = StartCoroutine(ActivateAI());
                yield break;
            }
            yield return new WaitForSeconds(0.5f); // Check every half second
        }
    }

    // Activate AI after delay
    private IEnumerator ActivateAI()
    {
        yield return new WaitForSeconds(_activationDelay);
        _isActive = true;
        DebugLog("Freddy AI activated!");
        SetState(FreddyAIState.Roaming);
    }

    // Change AI state
    private void SetState(FreddyAIState newState)
    {
        if (_currentState == newState) return;

        DebugLog("State Change: " + _currentState + " -> " + newState);

        // Exit current state
        OnStateExit(_currentState);

        // Change state
        _currentState = newState;

        // Enter new state
        OnStateEnter(_currentState);
    }

    // Called when entering a new state
    private void OnStateEnter(FreddyAIState state)
    {
        DebugLog("Entering State: " + state);

        switch (state)
        {
            case FreddyAIState.Roaming:
                if (_agent != null)
                {
                    _agent.isStopped = false;
                    _agent.speed = _roamSpeed;
                    DebugLog("Roaming: Speed set to " + _roamSpeed + ", isStopped = " + _agent.isStopped);
                }
                SetRoamTarget();
                PlayAnimation(ANIM_RUNNING);
                break;

            case FreddyAIState.Charging:
                if (_agent != null)
                {
                    _agent.isStopped = false;
                    _agent.speed = _chargeSpeed;
                    DebugLog("Charging: Speed set to " + _chargeSpeed + ". Player detected!");
                }
                PlayAnimation(ANIM_RUNNING);
                break;

            case FreddyAIState.Jumpscare:
                if (_agent != null)
                {
                    _agent.isStopped = true;
                }
                DebugLog("Jumpscare: Triggering jumpscare!");
                StartCoroutine(PerformJumpscare());
                break;
        }
    }

    // Called when exiting a state
    private void OnStateExit(FreddyAIState state)
    {
        DebugLog("Exiting State: " + state);
    }

    // Update roaming behavior
    private void UpdateRoaming()
    {
        if (_agent == null || _player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

        // Check if player is close enough to start charging
        if (distanceToPlayer <= _chargeDistance)
        {
            DebugLog("Roaming: Player within charge distance (" + distanceToPlayer.ToString("F2") + "m). Switching to Charging state.");
            SetState(FreddyAIState.Charging);
            return;
        }

        // Check if reached roam destination
        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            _roamTimer += Time.deltaTime;

            // Play idle animation while waiting
            if (_animator != null && _roamTimer > 0.1f)
            {
                PlayAnimation(ANIM_IDLE);
            }

            // Wait at destination, then pick new target
            if (_roamTimer >= _roamWaitTime)
            {
                DebugLog("Roaming: Reached destination. Picking new roam target.");
                SetRoamTarget();
                _roamTimer = 0f;
                PlayAnimation(ANIM_RUNNING);
            }
        }
        else if (_agent.velocity.sqrMagnitude > 0.1f)
        {
            // Moving to destination - ensure running animation is playing
            PlayAnimation(ANIM_RUNNING);
        }

        // Rotate the PARENT towards movement direction
        if (_agent.velocity.sqrMagnitude > 0.1f)
        {
            Vector3 direction = _agent.velocity.normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 15f);
        }

        // Occasionally look towards player while roaming (stalking behavior)
        if (Random.value < 0.1f)
        {
            Vector3 directionToPlayer = (_player.position - transform.position);
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 2f);
        }
    }

    // Update charging behavior
    private void UpdateCharging()
    {
        if (_agent == null || _player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

        // Check if close enough for jumpscare
        if (distanceToPlayer <= _jumpscareDistance)
        {
            DebugLog("Charging: Player within jumpscare distance (" + distanceToPlayer.ToString("F2") + "m). Triggering jumpscare!");
            SetState(FreddyAIState.Jumpscare);
            return;
        }

        // Check if player escaped
        if (distanceToPlayer > _losePlayerDistance)
        {
            DebugLog("Charging: Player escaped (distance: " + distanceToPlayer.ToString("F2") + "m). Returning to Roaming state.");
            SetState(FreddyAIState.Roaming);
            return;
        }

        // Chase the player
        _agent.SetDestination(_player.position);

        // Always look at player while charging - rotate PARENT
        if (_agent.velocity.sqrMagnitude > 0.1f)
        {
            Vector3 directionToPlayer = (_player.position - transform.position);
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }
    }

    // Set a random roaming target
    private void SetRoamTarget()
    {
        if (_agent == null) return;

        Vector3 randomDirection = Random.insideUnitSphere * _roamRadius;
        randomDirection += transform.position;
        randomDirection.y = transform.position.y; // Keep on same Y level

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, _roamRadius, NavMesh.AllAreas))
        {
            _roamTarget = hit.position;
            bool pathSet = _agent.SetDestination(_roamTarget);
            DebugLog("Roaming: New target set at " + _roamTarget + " (Path valid: " + pathSet + ")");
            DebugLog("  Distance to target: " + Vector3.Distance(transform.position, _roamTarget).ToString("F2"));
            
            if (_agent.hasPath)
            {
                DebugLog("  Path corners: " + _agent.path.corners.Length + ", Status: " + _agent.pathStatus);
            }
            else
            {
                Debug.LogWarning("Freddy AI: Agent has no path after setting destination!");
            }
        }
        else
        {
            Debug.LogWarning("Freddy AI: Failed to find valid NavMesh position for roaming target.");
            Debug.LogWarning("  Tried position: " + randomDirection);
            Debug.LogWarning("  Freddy position: " + transform.position);
        }
    }

    // Perform jumpscare
    private IEnumerator PerformJumpscare()
    {
        DebugLog("Jumpscare: Starting jumpscare sequence.");
        PlayAnimation(ANIM_JUMPSCARE);

        // Look at player during jumpscare - rotate PARENT
        if (_player != null)
        {
            Vector3 directionToPlayer = (_player.position - transform.position);
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
        }

        // Disable player control (optional)
        if (GameController.Instance != null && GameController.Instance.Player != null)
        {
            GameController.Instance.Player.SetState(PlayerController.PlayerState.Cutscene);
            DebugLog("Jumpscare: Player control disabled.");
        }

        yield return new WaitForSeconds(_jumpscareDuration);

        DebugLog("Jumpscare: Jumpscare sequence complete. Restoring player control.");

        // After jumpscare, restore player control
        if (GameController.Instance != null && GameController.Instance.Player != null)
        {
            GameController.Instance.Player.SetState(PlayerController.PlayerState.Normal);
        }

        SetState(FreddyAIState.Roaming);
    }

    // Play animation
    private void PlayAnimation(string animationName)
    {
        if (_animator == null)
        {
            return;
        }

        // Check if parameter exists
        bool parameterExists = false;
        foreach (AnimatorControllerParameter param in _animator.parameters)
        {
            if (param.name == animationName)
            {
                parameterExists = true;
                break;
            }
        }

        if (!parameterExists)
        {
            return;
        }

        // Reset all animation triggers/bools
        _animator.SetBool(ANIM_IDLE, false);
        _animator.SetBool(ANIM_RUNNING, false);
        _animator.SetBool(ANIM_JUMPSCARE, false);

        // Set the desired animation
        _animator.SetBool(animationName, true);
    }

    // Public method to manually activate Freddy (for testing or alternative triggers)
    public void ActivateNow()
    {
        if (_activationCoroutine != null)
        {
            StopCoroutine(_activationCoroutine);
        }
        _isActive = true;
        DebugLog("Freddy AI manually activated via ActivateNow().");
        SetState(FreddyAIState.Roaming);
    }

    // Helper method for debug logging
    private void DebugLog(string message)
    {
        if (_enableDebugLogs)
        {
            Debug.Log("[Freddy AI] " + message);
        }
    }

    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        // Draw roam radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _roamRadius);

        // Draw charge distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _chargeDistance);

        // Draw jumpscare distance
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, _jumpscareDistance);

        // Draw current roam target
        if (_currentState == FreddyAIState.Roaming)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(_roamTarget, 0.5f);
            Gizmos.DrawLine(transform.position, _roamTarget);
        }

        // Draw NavMesh agent path if it exists
        if (_agent != null && _agent.hasPath)
        {
            Gizmos.color = Color.cyan;
            Vector3[] corners = _agent.path.corners;
            for (int i = 0; i < corners.Length - 1; i++)
            {
                Gizmos.DrawLine(corners[i], corners[i + 1]);
                Gizmos.DrawSphere(corners[i], 0.3f);
            }
        }

        // Draw parent forward direction
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 3f);
    }
}
