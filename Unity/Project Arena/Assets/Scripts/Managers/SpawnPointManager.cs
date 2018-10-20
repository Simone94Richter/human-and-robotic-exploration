using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// SpawnPointManager menages the spawn points, keeping track of the last time each one of them was
/// used.
/// </summary>
public class SpawnPointManager : CoreComponent {

    [SerializeField] private float spawnCooldown = 5f;

    // List of all the spawn points.
    private List<SpawnPoint> spawnPoints;
    private List<SpawnPoint> targetPoints;
    // Last used spawn point.
    private SpawnPoint lastUsed;

    // Use this for initialization.
    private void Start() {
        spawnPoints = new List<SpawnPoint>();
        targetPoints = new List<SpawnPoint>();
        SetReady(true);
    }

    // Sets the spawn points.
    public void SetSpawnPoints(List<GameObject> SPs) {
        if (SPs != null && SPs.Count > 0) {
            foreach (GameObject s in SPs) {
                spawnPoints.Add(new SpawnPoint(s.transform.position, -1 * Mathf.Infinity));
            }
        } else {
            ManageError(Error.HARD_ERROR, "Error while setting the spawn points, no spawn point " +
                "was found.");
        }
    }

    public void SetTargetPoint(List<GameObject> TPs)
    {
        if (TPs != null && TPs.Count > 0)
        {
            foreach (GameObject s in TPs)
            {
                targetPoints.Add(new SpawnPoint(s.transform.position, -1 * Mathf.Infinity));
            }
        }
        else
        {
            ManageError(Error.HARD_ERROR, "Error while setting the spawn points for target, no target point " +
                "was found.");
        }
    }

    // Updates the last used field of all the spawn points that have already been used.
    public void UpdateLastUsed() {
        foreach (SpawnPoint s in spawnPoints)
            if (s.lastUsed > -1 * Mathf.Infinity) {
                s.lastUsed = Time.time;
            }
    }

    // Returns an available spawn position.
    public Vector3 GetSpawnPosition() {
        List<SpawnPoint> availableSpawnPoints = spawnPoints.Where(spawnPoint =>
            Time.time - spawnPoint.lastUsed >= spawnCooldown && spawnPoint != lastUsed).ToList();

        if (availableSpawnPoints.Count == 0) {
            return GetRandomSpawnPoint(spawnPoints).spawnPosition;
        } else {
            return GetRandomSpawnPoint(availableSpawnPoints).spawnPosition;
        }
    }

    public Vector3 GetTargetPosition()
    {
        List<SpawnPoint> availableSpawnPoints = targetPoints;

        if (availableSpawnPoints.Count == 0)
        {
            return GetRandomSpawnPoint(targetPoints).spawnPosition;
        }
        else
        {
            return GetRandomSpawnPoint(availableSpawnPoints).spawnPosition;
        }
    }

    public Vector3 GetTargetPosition(int index)
    {
        List<SpawnPoint> spawnPoints = targetPoints;
        return spawnPoints[index].spawnPosition;
    }

    // Returns a random spawn point from a list.
    private SpawnPoint GetRandomSpawnPoint(List<SpawnPoint> SPs) {
        int index = UnityEngine.Random.Range(0, SPs.Count);
        SPs[index].lastUsed = Time.time;
        lastUsed = SPs[index];
        return SPs[index];
    }

    private class SpawnPoint {
        public Vector3 spawnPosition;
        public float lastUsed;

        public SpawnPoint(Vector3 sp, float lu) {
            spawnPosition = sp;
            lastUsed = lu;
        }
    }

}