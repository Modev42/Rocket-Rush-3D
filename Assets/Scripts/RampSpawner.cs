using UnityEngine;
using System.Collections.Generic;

public class RampSpawner : MonoBehaviour
{
    public GameObject rampPrefab;
    public Transform player;
    public RocketController rocket;
    
    [Header("Spawn Settings")]
    public float spawnDistance = 100f;
    public float rampInterval = 600f;  // BallSpawner'daki ile AYNI olmalı (450'den 300'e)
    
    [Header("Ramp Materials")]
    public Material redMaterial;
    public Material blueMaterial;
    public Material yellowMaterial;
    
    private List<GameObject> activeRamps = new List<GameObject>();
    private float nextRampZ = 200f;
    
    void Start()
    {
        if (player == null) player = FindObjectOfType<RocketController>().transform;
        if (rocket == null) rocket = FindObjectOfType<RocketController>();
    }
    
    void Update()
    {
        if (player == null) return;
        
        while (nextRampZ < player.position.z + spawnDistance)
        {
            SpawnRamp();
            
            if (activeRamps.Count > 5)
            {
                GameObject oldRamp = activeRamps[0];
                activeRamps.RemoveAt(0);
                Destroy(oldRamp);
            }
        }
    }
    
    void SpawnRamp()
    {
        Vector3 spawnPos = new Vector3(0, 0.25f, nextRampZ);
        GameObject ramp = Instantiate(rampPrefab, spawnPos, Quaternion.Euler(-15, 0, 0));
        ramp.transform.parent = transform;
        
        int rocketColor = rocket.GetCurrentColor();
        int rampColor = GetRandomColorExcluding(rocketColor);
        
        Renderer rend = ramp.GetComponent<Renderer>();
        if (rend != null)
        {
            switch (rampColor)
            {
                case 0: rend.material = redMaterial; break;
                case 1: rend.material = blueMaterial; break;
                case 2: rend.material = yellowMaterial; break;
            }
        }
        
        ramp.name = "ColorRamp_" + rampColor;
        activeRamps.Add(ramp);
        nextRampZ += rampInterval;
        
        // PauseSpawning kaldırıldı - matematiksel kontrol kullanıyoruz
    }
    
    int GetRandomColorExcluding(int excludeColor)
    {
        List<int> availableColors = new List<int>();
        for (int i = 0; i < 3; i++)
        {
            if (i != excludeColor) availableColors.Add(i);
        }
        return availableColors[Random.Range(0, availableColors.Count)];
    }
    
    public List<GameObject> GetActiveRamps()
    {
        return activeRamps;
    }
    
    public void ResetSpawner()
    {
        foreach (GameObject ramp in activeRamps)
        {
            if (ramp != null) Destroy(ramp);
        }
        activeRamps.Clear();
        nextRampZ = 200f;  // 50'den 200'e (başlangıçla aynı)
    }
}