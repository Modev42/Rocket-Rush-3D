using UnityEngine;

public class BallAutoDestroy : MonoBehaviour
{
    public Transform player;
    public float maxBehindDistance = 40f; // roket bu kadar ilerleyince top silinir

    void Update()
    {
        if (!player) return;

        // Roket topun önüne geçmiş mi?
        float dz = player.position.z - transform.position.z;

        if (dz > maxBehindDistance)
        {
            Destroy(gameObject);
        }
    }
}
