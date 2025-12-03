using UnityEngine;
using System.Collections.Generic;

public class RoadSpawner : MonoBehaviour
{
    public GameObject roadPrefab;
    public int initialSegments = 10;
    public float segmentLength = 10f;
    public Transform player;
    public float spawnAheadDistance = 120f;
    public float despawnBehindDistance = 50f; // oyuncunun gerisindeki yolu silme mesafesi
    
    private List<GameObject> activeSegments = new List<GameObject>();
    private float nextSpawnZ = 0f;
    private int lastRocketColor = -1; // BU CLASS İÇİNDE
    
    void Start()
    {
        ResetSpawner();
    }
    
    void Update()
    {
        if (player == null) return;

        // Roket rengi değiştiyse mevcut segmentleri boya
        RocketController rocket = player.GetComponent<RocketController>();
        if (rocket != null)
        {
            int currentRocketColor = rocket.GetCurrentColor();
            if (currentRocketColor != lastRocketColor)
            {
                UpdateExistingSegments(currentRocketColor);
                lastRocketColor = currentRocketColor;
            }
        }

        // Öne doğru yeterince yol yoksa spawn et
        while (nextSpawnZ < player.position.z + spawnAheadDistance)
            SpawnRoadSegment();

        // Arkayı mesafeye göre temizle (count’a göre değil)
        while (activeSegments.Count > 0)
        {
            GameObject first = activeSegments[0];
            if (first == null) { activeSegments.RemoveAt(0); continue; }

            float dz = player.position.z - first.transform.position.z;
            if (dz > despawnBehindDistance)
            {
                activeSegments.RemoveAt(0);
                Destroy(first);
            }
            else break;
        }
    }

    
    void SpawnRoadSegment()
    {
        Vector3 spawnPosition = new Vector3(0, 0, nextSpawnZ);
        GameObject newSegment = Instantiate(roadPrefab, spawnPosition, Quaternion.identity);
        newSegment.transform.parent = transform;
        activeSegments.Add(newSegment);
        
        if (player != null)
        {
            RocketController rocket = player.GetComponent<RocketController>();
            if (rocket != null)
            {
                RoadSegment roadSeg = newSegment.GetComponent<RoadSegment>();
                if (roadSeg != null)
                {
                    roadSeg.SetStripeColor(rocket.GetCurrentColor());
                }
            }
        }
        
        nextSpawnZ += segmentLength;
    }
    
    void UpdateExistingSegments(int rocketColor)
    {
        foreach (GameObject segment in activeSegments)
        {
            if (segment != null)
            {
                RoadSegment roadSeg = segment.GetComponent<RoadSegment>();
                if (roadSeg != null)
                {
                    roadSeg.SetStripeColor(rocketColor);
                }
            }
        }
    }
    
    public void ResetSpawner()
    {
        foreach (GameObject segment in activeSegments)
        {
            if (segment != null)
                Destroy(segment);
        }
        activeSegments.Clear();
        
        nextSpawnZ = 0f;
        
        for (int i = 0; i < initialSegments; i++)
        {
            SpawnRoadSegment();
        }
    }
}
