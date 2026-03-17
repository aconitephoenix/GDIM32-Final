using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.PostProcessing;

public class freddyAI : MonoBehaviour
{
    // AI States
    public enum FreddyAIState
    {
        Inactive,       // Before activation
        Charging,       // Chasing the player
        Jumpscare,      // Jumpscare animation
    }

    [Header("AI Activation")]
    [SerializeField] private float _activationDelay = 5f; // Time before Freddy activates after quest starts
    [SerializeField] private bool _allowDebugActivation = true; // Allow pressing P to activate
    private bool _isActive = false;
    private Coroutine _activationCoroutine;

    [Header("AI Components")]
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Animator _animator;
    [SerializeField] private Transform _modelTransform; // Child transform for adjusting jumpscare Y position
    [SerializeField] private AudioSource _audioSource; // Audio source for running sounds
    [SerializeField] private AudioSource _breathingAudioSource; // Separate audio source for breathing sound
    private FreddyAIState _currentState = FreddyAIState.Inactive;

    [Header("Charging Settings")]
    [SerializeField] private float _chargeSpeed = 6f;
    [SerializeField] private float _losePlayerDistance = 25f; // Distance at which Freddy loses the player

    [Header("Audio Settings")]
    [SerializeField] private AudioClip _runningClip; // Running sound effect
    [SerializeField] private AudioClip _jumpscareClip; // Jumpscare sound effect
    [SerializeField] private float _maxAudioDistance = 20f; // Distance at which audio is at full volume
    [SerializeField] private float _minAudioDistance = 2f; // Distance at which audio starts fading

    [Header("Jumpscare Settings")]
    [SerializeField] private float _jumpscareDistance = 2f; // Distance to trigger jumpscare
    [SerializeField] private float _jumpscareDuration = 3f; // How long the jumpscare lasts
    [SerializeField] private float _jumpscareFollowDistance = 5f; // Distance from camera to position Freddy
    [SerializeField] private float _jumpscareFollowHeight = -1f; // Height offset for child model during jumpscare
    [SerializeField] private float _escapeRunDuration = 2f; // How long Freddy runs away
    [SerializeField] private float _escapeRunSpeed = 8f; // Speed while escaping
    [SerializeField] private float _vignetteEffectDuration = 3f; // Duration of vignette ping-pong effect
    [SerializeField] private PostProcessVolume _postProcessVolume; // Reference to post-processing volume
    [SerializeField] private AudioClip _breathingSound; // Breathing sound effect during vignette ping-pong

    [Header("Debug Settings")]
    [SerializeField] private bool _enableDebugLogs = true; // Toggle debug logs on/off

    // Animation parameter names (adjust these to match your animator)
    private readonly string ANIM_IDLE = "idle";
    private readonly string ANIM_RUNNING = "running";
    private readonly string ANIM_JUMPSCARE = "jumpscare";

    private Transform _player;
    private Camera _playerCamera;
    private float _diagnosticTimer = 0f;
    private Vector3 _initialModelLocalPosition; // Store initial local position of model
    private bool _hasChildModel = false; // Track if we have a separate child model
    private bool _hasBeenDeactivated = false; // Track if Freddy has been deactivated due to quest completion
    private bool _isPlayingRunningAudio = false; // Track if running audio is currently playing

    void Start()
    {
        // Get player reference
        if (GameController.Instance != null && GameController.Instance.Player != null)
        {
            _player = GameController.Instance.Player.transform;
            _playerCamera = _player.GetComponentInChildren<Camera>();
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

        // Get AudioSource if not assigned
        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource != null)
            {
                DebugLog("AudioSource component found on parent.");
            }
            else
            {
                Debug.LogWarning("Freddy AI: AudioSource component missing! Running sounds will not play.");
            }
        }

        // Get model transform if not assigned
        if (_modelTransform == null && transform.childCount > 0)
        {
            _modelTransform = transform.GetChild(0);
            _hasChildModel = true;
            _initialModelLocalPosition = _modelTransform.localPosition;
            DebugLog("Model transform found on child: " + _modelTransform.gameObject.name);
            DebugLog("  Initial local position: " + _initialModelLocalPosition);
        }
        else if (_modelTransform != null)
        {
            _hasChildModel = true;
            _initialModelLocalPosition = _modelTransform.localPosition;
            DebugLog("Model transform assigned: " + _modelTransform.gameObject.name);
            DebugLog("  Initial local position: " + _initialModelLocalPosition);
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
                    DebugLog("    * " + param.name + " (Type: " + param.type + ")"
                    );
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
            _agent.speed = _chargeSpeed;
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

        // If player has collected all pages, deactivate Freddy
        if (!_hasBeenDeactivated && GameController.Instance.Player._currentPageCount >= GameController.Instance.Player._maxPageCount - 1)
        {
            DebugLog("Player collected all pages! Deactivating Freddy.");
            DeactivateFreddy();
            return;
        }

        // Update behavior based on current state
        switch (_currentState)
        {
            case FreddyAIState.Charging:
                UpdateCharging();
                break;
            case FreddyAIState.Jumpscare:
                // Jumpscare is handled by coroutine
                break;
        }
    }

    // Deactivate Freddy and make him invisible
    private void DeactivateFreddy()
    {
        _hasBeenDeactivated = true;
        _isActive = false;
        _currentState = FreddyAIState.Inactive;

        // Stop the NavMeshAgent
        if (_agent != null)
        {
            _agent.isStopped = true;
            _agent.enabled = false;
        }

        // Stop running audio
        StopRunningAudio();

        // Disable all renderers to make Freddy invisible
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }

        DebugLog("Freddy AI deactivated and hidden.");
    }

    // Check if player has started collecting pages (page quest started)
    private IEnumerator CheckForQuestStart()
    {
        while (!_isActive)
        {
            // Activate when player has collected at least 1 page (quest has started) but less than 7
            if (GameController.Instance.Player != null && 
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
        SetState(FreddyAIState.Charging);
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
            case FreddyAIState.Charging:
                if (_agent != null)
                {
                    _agent.isStopped = false;
                    _agent.speed = _chargeSpeed;
                    DebugLog("Charging: Speed set to " + _chargeSpeed + ". Player detected!");
                }
                PlayAnimation(ANIM_RUNNING);
                StartRunningAudio();
                break;

            case FreddyAIState.Jumpscare:
                if (_agent != null)
                {
                    _agent.isStopped = true;
                }
                StopRunningAudio();
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

        // Chase the player
        _agent.SetDestination(_player.position);

        // Update running audio volume based on distance
        UpdateRunningAudioVolume(distanceToPlayer);

        // Always look at player while charging - rotate PARENT
        if (_agent.velocity.sqrMagnitude > 0.1f)
        {
            Vector3 directionToPlayer = (_player.position - transform.position);
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }
    }

    // Start playing running audio
    private void StartRunningAudio()
    {
        if (_audioSource == null || _runningClip == null)
        {
            return;
        }

        if (!_isPlayingRunningAudio)
        {
            _audioSource.clip = _runningClip;
            _audioSource.loop = true;
            _audioSource.Play();
            _isPlayingRunningAudio = true;
            DebugLog("Running audio started.");
        }
    }

    // Stop playing running audio
    private void StopRunningAudio()
    {
        if (_audioSource == null)
        {
            return;
        }

        if (_isPlayingRunningAudio)
        {
            _audioSource.Stop();
            _isPlayingRunningAudio = false;
            DebugLog("Running audio stopped.");
        }
    }

    // Update running audio volume based on distance to player
    private void UpdateRunningAudioVolume(float distanceToPlayer)
    {
        if (_audioSource == null || !_isPlayingRunningAudio)
        {
            return;
        }

        // Calculate volume based on distance
        // Full volume at _minAudioDistance, zero volume at _maxAudioDistance
        float volume = 1f - Mathf.Clamp01((distanceToPlayer - _minAudioDistance) / (_maxAudioDistance - _minAudioDistance));
        _audioSource.volume = volume;
    }

    // Play jumpscare sound once
    private void PlayJumpscareSound()
    {
        if (_audioSource == null || _jumpscareClip == null)
        {
            return;
        }

        _audioSource.clip = _jumpscareClip;
        _audioSource.loop = false;
        _audioSource.PlayOneShot(_jumpscareClip, 0.5f);
        DebugLog("Jumpscare sound played.");
    }

    // Perform jumpscare
    private IEnumerator PerformJumpscare()
    {
        DebugLog("Jumpscare: Starting jumpscare sequence.");

        // Disable player control during jumpscare
        if (GameController.Instance != null && GameController.Instance.Player != null)
        {
            GameController.Instance.Player.SetState(PlayerController.PlayerState.Cutscene);
            DebugLog("Jumpscare: Player control disabled.");
        }

        // Stop NavMeshAgent movement during jumpscare
        if (_agent != null)
        {
            _agent.isStopped = true;
        }

        // Store original child Y position if we have a model transform
        float originalChildY = 0f;
        if (_modelTransform != null)
        {
            originalChildY = _modelTransform.localPosition.y;
            DebugLog("Jumpscare: Stored original child Y position: " + originalChildY);
        }

        // Teleport Freddy in front of player camera
        if (_player != null && _playerCamera != null)
        {
            Vector3 cameraPos = _playerCamera.transform.position;
            Vector3 cameraForward = _playerCamera.transform.forward;
            Vector3 jumpscarePos = cameraPos + (cameraForward * _jumpscareFollowDistance);
            jumpscarePos.y = transform.position.y; // Keep parent at same height

            transform.position = jumpscarePos;
            
            // Look directly at player camera
            Vector3 directionToPlayer = (_playerCamera.transform.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(directionToPlayer);

            // Adjust child model Y position for jumpscare
            if (_modelTransform != null)
            {
                Vector3 newChildPos = _modelTransform.localPosition;
                newChildPos.y = _jumpscareFollowHeight;
                _modelTransform.localPosition = newChildPos;
                DebugLog("Jumpscare: Adjusted child Y to " + _jumpscareFollowHeight);
            }

            DebugLog("Jumpscare: Freddy teleported to " + jumpscarePos);
        }

        // Play jumpscare animation and sound
        PlayAnimation(ANIM_JUMPSCARE);
        PlayJumpscareSound();
        yield return new WaitForSeconds(_jumpscareDuration);

        DebugLog("Jumpscare: Animation complete. Restoring player control and running away.");

        // Restore player control immediately
        if (GameController.Instance != null && GameController.Instance.Player != null)
        {
            GameController.Instance.Player.SetState(PlayerController.PlayerState.Normal);
            DebugLog("Jumpscare: Player control restored.");
        }

        // Start vignette ping-pong effect
        StartCoroutine(PingPongVignette());

        // Restore child model Y position
        if (_modelTransform != null)
        {
            Vector3 restoredChildPos = _modelTransform.localPosition;
            restoredChildPos.y = originalChildY;
            _modelTransform.localPosition = restoredChildPos;
            DebugLog("Jumpscare: Restored child Y to " + originalChildY);
        }

        // Resume normal AI behavior
        if (_agent != null)
        {
            _agent.isStopped = false;
            _agent.speed = _escapeRunSpeed;

            // Set escape destination away from player
            Vector3 escapeDirection = (transform.position - _player.position).normalized;
            Vector3 escapeTarget = transform.position + (escapeDirection * 50f);
            escapeTarget.y = transform.position.y;

            _agent.SetDestination(escapeTarget);
            PlayAnimation(ANIM_RUNNING);

            DebugLog("Jumpscare: Freddy running away from " + transform.position + " at speed " + _escapeRunSpeed);
        }

        // Wait for escape run duration
        yield return new WaitForSeconds(_escapeRunDuration);

        StartRunningAudio();
        SetState(FreddyAIState.Charging);
    }

    // Ping-pong vignette effect
    private IEnumerator PingPongVignette()
    {
        DebugLog("Jumpscare: Starting vignette ping-pong effect.");
        
        Vignette vignetteEffect = null;
        if (_postProcessVolume != null && _postProcessVolume.profile != null)
        {
            _postProcessVolume.profile.TryGetSettings<Vignette>(out vignetteEffect);
        }

        if (vignetteEffect == null)
        {
            DebugLog("Jumpscare: Vignette effect not found in post-processing volume.");
            yield break;
        }

        // Play breathing sound effect as a one-shot on separate audio source
        if (_breathingAudioSource != null && _breathingSound != null)
        {
            // Ensure the breathing audio source is not muted or affected by other settings
            _breathingAudioSource.mute = false;
            _breathingAudioSource.PlayOneShot(_breathingSound, 0.7f);
            DebugLog("Jumpscare: Breathing sound started on separate audio source.");
        }

        float elapsedTime = 0f;
        float originalSmoothness = vignetteEffect.smoothness.value;
        float originalIntensity = vignetteEffect.intensity.value;

        // Ping-pong phase
        while (elapsedTime < _vignetteEffectDuration)
        {
            elapsedTime += Time.deltaTime;
            
            // Ping-pong the smoothness value between 0.155 and 0.7
            float pingPongValue = Mathf.PingPong(elapsedTime * 1.3f, 1f);
            vignetteEffect.smoothness.value = Mathf.Lerp(0.155f, 0.7f, pingPongValue);
            vignetteEffect.intensity.value = Mathf.Lerp(0.5f, 0.75f, pingPongValue);

            yield return null;
        }

        // Smooth return to original value
        float returnTime = 0f;
        float returnDuration = 0.5f; // Duration of smooth transition back
        while (returnTime < returnDuration)
        {
            returnTime += Time.deltaTime;
            float t = returnTime / returnDuration;
            vignetteEffect.smoothness.value = Mathf.Lerp(vignetteEffect.smoothness.value, originalSmoothness, t);
            vignetteEffect.intensity.value = Mathf.Lerp(vignetteEffect.intensity.value, originalIntensity, t);

            yield return null;
        }

        // Ensure it's set to exactly the original value
        vignetteEffect.smoothness.value = originalIntensity;
        DebugLog("Jumpscare: Vignette ping-pong effect complete and restored to original value.");
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
        SetState(FreddyAIState.Charging);
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

        // Draw charge distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _chargeSpeed);

        // Draw jumpscare distance
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, _jumpscareDistance);

        // Draw audio distance visualization
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f); // Yellow with transparency
        Gizmos.DrawWireSphere(transform.position, _minAudioDistance);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f); // Orange with transparency
        Gizmos.DrawWireSphere(transform.position, _maxAudioDistance);

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
