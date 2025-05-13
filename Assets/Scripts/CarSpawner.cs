using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    [System.Serializable]
    public class Lane
    {
        public string name; // For identification in the Inspector
        public Transform spawnPoint;
        public bool spawnMovingRight = true;
        public float carSpeed = 5f; // Fixed speed for all cars in this lane
        public float minSpawnInterval = 2f;
        public float maxSpawnInterval = 5f;
        public List<GameObject> carPrefabs = new List<GameObject>(); // Using List instead of array for easier management
        [Range(0f, 1f)]
        public float spawnChance = 1f;
    }
    
    public Lane[] lanes = new Lane[4]; // Preset to 4 lanes
    
    void Start()
    {
        // Debug all car prefabs to check for issues
        ValidateSetup();
        
        // Start the spawning coroutine for each lane
        for (int i = 0; i < lanes.Length; i++)
        {
            StartCoroutine(SpawnCarsInLane(i));
        }
    }
    
    void ValidateSetup()
    {
        for (int i = 0; i < lanes.Length; i++)
        {
            Lane lane = lanes[i];
            Debug.Log($"Validating Lane {i}: {lane.name}");
            
            if (lane.spawnPoint == null)
            {
                Debug.LogError($"Lane {i} ({lane.name}) has no spawn point assigned!");
                continue;
            }
            
            if (lane.carPrefabs.Count == 0)
            {
                Debug.LogError($"Lane {i} ({lane.name}) has no car prefabs assigned!");
                continue;
            }
            
            // Check each prefab
            for (int j = 0; j < lane.carPrefabs.Count; j++)
            {
                if (lane.carPrefabs[j] == null)
                {
                    Debug.LogError($"Null car prefab found at index {j} in lane {lane.name}");
                    lane.carPrefabs.RemoveAt(j);
                    j--; // Adjust index after removal
                }
                else
                {
                    // Check if the prefab has the Car component
                    if (lane.carPrefabs[j].GetComponent<Car>() == null)
                    {
                        Debug.LogWarning($"Car prefab '{lane.carPrefabs[j].name}' in lane {lane.name} is missing the Car script component!");
                    }
                    else
                    {
                        Debug.Log($"  Valid car prefab at index {j}: {lane.carPrefabs[j].name}");
                    }
                }
            }
        }
    }
    
    IEnumerator SpawnCarsInLane(int laneIndex)
    {
        Lane lane = lanes[laneIndex];
        
        // Skip if this lane has no valid setup
        if (lane.spawnPoint == null || lane.carPrefabs.Count == 0)
        {
            Debug.LogWarning($"Skipping spawn coroutine for lane {lane.name} due to invalid setup");
            yield break;
        }
        
        while (true)
        {
            // Wait for random interval
            float spawnInterval = Random.Range(lane.minSpawnInterval, lane.maxSpawnInterval);
            yield return new WaitForSeconds(spawnInterval);
            
            // Check if we should spawn based on spawn chance
            if (Random.value <= lane.spawnChance)
            {
                SpawnCar(lane);
            }
        }
    }
    
    void SpawnCar(Lane lane)
    {
        // Skip if lane has no valid prefabs or spawn point
        if (lane.carPrefabs.Count == 0 || lane.spawnPoint == null)
        {
            return;
        }
        
        try
        {
            // Choose a random car prefab
            int prefabIndex = Random.Range(0, lane.carPrefabs.Count);
            GameObject carPrefab = lane.carPrefabs[prefabIndex];
            
            // Skip if the prefab is null
            if (carPrefab == null)
            {
                Debug.LogError($"Attempted to spawn null car prefab at index {prefabIndex} in lane {lane.name}");
                // Remove the null prefab
                lane.carPrefabs.RemoveAt(prefabIndex);
                return;
            }
            
            // Spawn the car
            GameObject carInstance = Instantiate(carPrefab, lane.spawnPoint.position, lane.spawnPoint.rotation);
            
            // Configure the car
            Car carScript = carInstance.GetComponent<Car>();
            if (carScript != null)
            {
                carScript.movingRight = lane.spawnMovingRight;
                carScript.speed = lane.carSpeed; // Use the lane's fixed speed
            }
            else
            {
                Debug.LogWarning($"Car prefab {carPrefab.name} is missing the Car script");
                
                // Add the Car script component if it's missing
                carScript = carInstance.AddComponent<Car>();
                carScript.movingRight = lane.spawnMovingRight;
                carScript.speed = lane.carSpeed;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error spawning car in lane {lane.name}: {e.Message}");
        }
    }
}