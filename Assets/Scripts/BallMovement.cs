using UnityEngine;

[DisallowMultipleComponent]
public class BallMovement : MonoBehaviour
{
    public float speed = 4f;
    public float range = 1f;
    public float startX;

    void Update()
    {
        float offset = Mathf.PingPong(Time.time * speed, range * 2f) - range;
        var p = transform.position;
        p.x = startX + offset;
        transform.position = p;
    }
}
