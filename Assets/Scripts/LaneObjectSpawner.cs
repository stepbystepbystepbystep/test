using System.Collections.Generic;
using UnityEngine;

public class LaneObjectSpawner : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Transform player;

    [Header("Lane Setup")]
    [SerializeField] private float laneDistance = 2.5f;

    [Header("Prefabs")]
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private GameObject jetpackPrefab;

    [Header("Spawn Distances")]
    [SerializeField] private float initialSpawnAhead = 25f;
    [SerializeField] private float keepSpawnAhead = 60f;
    [SerializeField] private float destroyBehind = 20f;

    [Header("Rates")]
    [SerializeField] private Vector2 obstacleStepRange = new Vector2(4f, 8f);
    [SerializeField] private Vector2 jetpackStepRange = new Vector2(25f, 38f);

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();
    private float nextObstacleY;
    private float nextJetpackY;

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("LaneObjectSpawner: player reference is not set.");
            enabled = false;
            return;
        }

        float startY = player.position.y + initialSpawnAhead;
        nextObstacleY = startY;
        nextJetpackY = startY + 12f;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            return;
        }

        float frontY = player.position.y + keepSpawnAhead;

        while (nextObstacleY < frontY)
        {
            SpawnObstacleRow(nextObstacleY);
            nextObstacleY += Random.Range(obstacleStepRange.x, obstacleStepRange.y);
        }

        while (nextJetpackY < frontY)
        {
            SpawnJetpack(nextJetpackY);
            nextJetpackY += Random.Range(jetpackStepRange.x, jetpackStepRange.y);
        }

        CleanupBehindPlayer();
    }

    private void SpawnObstacleRow(float y)
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
        {
            return;
        }

        int safeLane = Random.Range(-1, 2);

        for (int lane = -1; lane <= 1; lane++)
        {
            if (lane == safeLane)
            {
                continue;
            }

            var prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
            var instance = Instantiate(prefab, new Vector3(lane * laneDistance, y, 0f), Quaternion.identity);
            instance.tag = "Obstacle";
            spawnedObjects.Add(instance);
        }
    }

    private void SpawnJetpack(float y)
    {
        if (jetpackPrefab == null)
        {
            return;
        }

        int lane = Random.Range(-1, 2);
        var instance = Instantiate(jetpackPrefab, new Vector3(lane * laneDistance, y, 0f), Quaternion.identity);
        instance.tag = "Jetpack";
        spawnedObjects.Add(instance);
    }

    private void CleanupBehindPlayer()
    {
        float minY = player.position.y - destroyBehind;

        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            GameObject current = spawnedObjects[i];

            if (current == null)
            {
                spawnedObjects.RemoveAt(i);
                continue;
            }

            if (current.transform.position.y < minY)
            {
                Destroy(current);
                spawnedObjects.RemoveAt(i);
            }
        }
    }
}
