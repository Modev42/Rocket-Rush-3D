// CameraFollow.cs
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public void ResetCamera(Transform newTarget = null) { SnapNow(); }

    public Transform target;
    public Vector3 offset = new Vector3(0f, 4f, -5f);

    [Header("Smoothing")]
    public float smoothXY = 10f;   // X,Y için yumuşatma hızı
    public bool lockZ = true;      // Z'yi kilitle

    Vector3 vel;

    void Awake()
    {
        if (!target) return;
        transform.position = target.position + offset; // başlangıçta direkt yerleştir
        vel = Vector3.zero;
    }

    void LateUpdate()
    {
        if (!target) return;

        Vector3 desired = target.position + offset;

        // X-Y yumuşak takip
        Vector3 pos = transform.position;
        pos.x = Mathf.SmoothDamp(transform.position.x, desired.x, ref vel.x, 1f / Mathf.Max(0.01f, smoothXY));
        pos.y = Mathf.SmoothDamp(transform.position.y, desired.y, ref vel.y, 1f / Mathf.Max(0.01f, smoothXY));

        // Z kilitli: roketin z + offset.z (anında)
        if (lockZ) pos.z = desired.z;
        else       pos.z = Mathf.SmoothDamp(transform.position.z, desired.z, ref vel.z, 1f / Mathf.Max(0.01f, smoothXY));

        transform.position = pos;
    }

    public void SnapNow()
    {
        if (!target) return;
        transform.position = target.position + offset;
        vel = Vector3.zero;
    }
}
