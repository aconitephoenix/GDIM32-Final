using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fogOptimization : MonoBehaviour
{
    [Header("Particle System Settings")]
    [SerializeField] private GameObject particleSystemPrefab; // The fog particle system prefab
    [SerializeField] private Transform player; // Reference to player transform
    
    [Header("Chunk Settings")]
    [SerializeField] private float chunkSize = 50f; // Size of each chunk
    [SerializeField] [Range(1, 3)] private int renderDistance = 1; // Quality setting: 1 = minimum, 3 = maximum
    [SerializeField] private float updateInterval = 0.5f; // How often to check player position
    [SerializeField] [Range(30f, 120f)] private float viewConeAngle = 90f; // Field of view cone angle
    [SerializeField] private bool alwaysUpdateForRotation = true; // Update chunks when player rotates
    
    // Dictionary to store active chunks
    private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();
    
    // Current player chunk position
    private Vector2Int currentPlayerChunk;
    
    // Timer for update interval
    private float updateTimer;
    
    // Previous player rotation to detect changes
    private float previousPlayerRotation;

    void Start()
    {
        // Get player reference if not assigned
        if (player == null)
        {
            if (GameController.Instance != null && GameController.Instance.Player != null)
            {
                player = GameController.Instance.Player.transform;
            }
            else
            {
                Debug.LogError("Player reference not found! Please assign player in inspector or ensure GameController exists.");
                enabled = false;
                return;
            }
        }

        // Validate particle system prefab
        if (particleSystemPrefab == null)
        {
            Debug.LogError("Particle System Prefab not assigned! Please assign a fog particle system prefab in the inspector.");
            enabled = false;
            return;
        }

        // Initialize chunks around player
        currentPlayerChunk = GetChunkPosition(player.position);
        previousPlayerRotation = player.eulerAngles.y;
        UpdateChunks();
    }

    void Update()
    {
        // Update timer
        updateTimer += Time.deltaTime;
        
        // Check if it's time to update chunks
        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            
            Vector2Int newPlayerChunk = GetChunkPosition(player.position);
            float currentRotation = player.eulerAngles.y;
            
            // Check if significant rotation occurred (more than 15 degrees)
            bool rotationChanged = alwaysUpdateForRotation && Mathf.Abs(Mathf.DeltaAngle(previousPlayerRotation, currentRotation)) > 15f;
            
            // Update if player moved to a different chunk OR rotated significantly
            if (newPlayerChunk != currentPlayerChunk || rotationChanged)
            {
                currentPlayerChunk = newPlayerChunk;
                previousPlayerRotation = currentRotation;
                UpdateChunks();
            }
        }
    }

    // Convert world position to chunk coordinates
    private Vector2Int GetChunkPosition(Vector3 worldPosition)
    {
        int chunkX = Mathf.FloorToInt(worldPosition.x / chunkSize);
        int chunkZ = Mathf.FloorToInt(worldPosition.z / chunkSize);
        return new Vector2Int(chunkX, chunkZ);
    }

    // Get world position from chunk coordinates (center of chunk)
    private Vector3 GetWorldPosition(Vector2Int chunkCoord)
    {
        float worldX = (chunkCoord.x * chunkSize) + (chunkSize / 2f);
        float worldZ = (chunkCoord.y * chunkSize) + (chunkSize / 2f);
        return new Vector3(worldX, 0f, worldZ);
    }

    // Check if a chunk is within the view cone
    private bool IsChunkInViewCone(Vector2Int chunkCoord)
    {
        Vector3 chunkWorldPos = GetWorldPosition(chunkCoord);
        Vector3 playerPos = player.position;
        
        // Get direction from player to chunk (2D, ignore Y)
        Vector2 toChunk = new Vector2(
            chunkWorldPos.x - playerPos.x,
            chunkWorldPos.z - playerPos.z
        );
        
        // Always include the chunk the player is standing in
        if (toChunk.magnitude < chunkSize * 0.5f)
        {
            return true;
        }
        
        // Get player's forward direction (2D)
        Vector3 playerForward3D = player.forward;
        Vector2 playerForward = new Vector2(playerForward3D.x, playerForward3D.z).normalized;
        
        // Calculate angle between player forward and direction to chunk
        float angleToChunk = Vector2.Angle(playerForward, toChunk);
        
        // Calculate distance-based render limits based on quality setting
        float forwardDistance = GetForwardDistance();
        float sideDistance = GetSideDistance();
        float backDistance = GetBackDistance();
        
        float distanceToChunk = toChunk.magnitude;
        
        // Determine if chunk is in front, side, or back cone
        if (angleToChunk <= viewConeAngle * 0.5f) // Front cone
        {
            return distanceToChunk <= forwardDistance;
        }
        else if (angleToChunk <= 90f) // Side areas
        {
            // Interpolate between front and side distance
            float t = (angleToChunk - viewConeAngle * 0.5f) / (90f - viewConeAngle * 0.5f);
            float effectiveDistance = Mathf.Lerp(forwardDistance, sideDistance, t);
            return distanceToChunk <= effectiveDistance;
        }
        else if (angleToChunk <= 135f) // Back-side areas
        {
            // Interpolate between side and back distance
            float t = (angleToChunk - 90f) / 45f;
            float effectiveDistance = Mathf.Lerp(sideDistance, backDistance, t);
            return distanceToChunk <= effectiveDistance;
        }
        else // Behind player
        {
            return distanceToChunk <= backDistance;
        }
    }

    // Get forward distance based on render quality
    private float GetForwardDistance()
    {
        // Render distance 1: 2 chunks, 2: 3.5 chunks, 3: 5 chunks forward
        return chunkSize * Mathf.Lerp(2f, 5f, (renderDistance - 1) / 2f);
    }

    // Get side distance based on render quality
    private float GetSideDistance()
    {
        // Render distance 1: 1.5 chunks, 2: 2.5 chunks, 3: 3.5 chunks to sides
        return chunkSize * Mathf.Lerp(1.5f, 3.5f, (renderDistance - 1) / 2f);
    }

    // Get back distance based on render quality
    private float GetBackDistance()
    {
        // Render distance 1: 1 chunk, 2: 1.5 chunks, 3: 2 chunks behind
        return chunkSize * Mathf.Lerp(1f, 2f, (renderDistance - 1) / 2f);
    }

    // Update chunks based on player position and view direction
    private void UpdateChunks()
    {
        // List to store chunks that should be active
        HashSet<Vector2Int> chunksToKeep = new HashSet<Vector2Int>();
        
        // Calculate search radius based on max possible distance (forward distance)
        int maxChunkRadius = Mathf.CeilToInt(GetForwardDistance() / chunkSize) + 1;
        
        // Generate chunks in a grid around the player, but filter by view cone
        for (int x = -maxChunkRadius; x <= maxChunkRadius; x++)
        {
            for (int z = -maxChunkRadius; z <= maxChunkRadius; z++)
            {
                Vector2Int chunkCoord = new Vector2Int(
                    currentPlayerChunk.x + x,
                    currentPlayerChunk.y + z
                );
                
                // Only add chunk if it's within the view cone
                if (IsChunkInViewCone(chunkCoord))
                {
                    chunksToKeep.Add(chunkCoord);
                    
                    // Create chunk if it doesn't exist
                    if (!activeChunks.ContainsKey(chunkCoord))
                    {
                        CreateChunk(chunkCoord);
                    }
                }
            }
        }
        
        // Remove chunks that are too far away or outside view cone
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        
        foreach (var chunk in activeChunks)
        {
            if (!chunksToKeep.Contains(chunk.Key))
            {
                chunksToRemove.Add(chunk.Key);
            }
        }
        
        // Destroy far chunks
        foreach (var chunkCoord in chunksToRemove)
        {
            DestroyChunk(chunkCoord);
        }
    }

    // Create a new chunk at the specified coordinates
    private void CreateChunk(Vector2Int chunkCoord)
    {
        Vector3 worldPos = GetWorldPosition(chunkCoord);
        GameObject chunk = Instantiate(particleSystemPrefab, worldPos, Quaternion.identity, transform);
        chunk.name = $"FogChunk_{chunkCoord.x}_{chunkCoord.y}";
        
        activeChunks.Add(chunkCoord, chunk);
    }

    // Destroy a chunk at the specified coordinates
    private void DestroyChunk(Vector2Int chunkCoord)
    {
        if (activeChunks.ContainsKey(chunkCoord))
        {
            GameObject chunk = activeChunks[chunkCoord];
            activeChunks.Remove(chunkCoord);
            Destroy(chunk);
        }
    }

    // Cleanup when script is disabled or destroyed
    private void OnDisable()
    {
        // Destroy all active chunks
        foreach (var chunk in activeChunks.Values)
        {
            if (chunk != null)
            {
                Destroy(chunk);
            }
        }
        activeChunks.Clear();
    }

    // Debug visualization
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || player == null) return;
        
        // Draw current player chunk
        Gizmos.color = Color.green;
        Vector3 playerChunkPos = GetWorldPosition(currentPlayerChunk);
        Gizmos.DrawWireCube(playerChunkPos, new Vector3(chunkSize, 5f, chunkSize));
        
        // Draw view cone
        Vector3 playerPos = player.position;
        Vector3 forward = player.forward;
        
        // Draw forward direction
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(playerPos, playerPos + forward * GetForwardDistance());
        
        // Draw view cone edges
        Gizmos.color = Color.yellow;
        float halfAngle = viewConeAngle * 0.5f;
        Vector3 rightEdge = Quaternion.Euler(0, halfAngle, 0) * forward * GetForwardDistance();
        Vector3 leftEdge = Quaternion.Euler(0, -halfAngle, 0) * forward * GetForwardDistance();
        
        Gizmos.DrawLine(playerPos, playerPos + rightEdge);
        Gizmos.DrawLine(playerPos, playerPos + leftEdge);
        
        // Draw cone arc
        DrawConeArc(playerPos, forward, GetForwardDistance(), halfAngle, 32);
        
        // Draw all active chunks
        Gizmos.color = Color.cyan;
        foreach (var chunk in activeChunks)
        {
            Vector3 chunkWorldPos = GetWorldPosition(chunk.Key);
            Gizmos.DrawWireCube(chunkWorldPos, new Vector3(chunkSize, 2f, chunkSize));
        }
    }

    // Helper method to draw cone arc
    private void DrawConeArc(Vector3 center, Vector3 forward, float radius, float halfAngle, int segments)
    {
        Gizmos.color = Color.yellow;
        Vector3 prevPoint = center + Quaternion.Euler(0, -halfAngle, 0) * forward * radius;
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = Mathf.Lerp(-halfAngle, halfAngle, i / (float)segments);
            Vector3 direction = Quaternion.Euler(0, angle, 0) * forward;
            Vector3 point = center + direction * radius;
            
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }
}
