using UnityEngine;

public class JetFlicker : MonoBehaviour
{
    public float baseScaleY = 1f, amp = 0.15f, speed = 18f;
    Vector3 baseScale;
    void Awake(){ baseScale = transform.localScale; }
    void Update(){
        float s = baseScaleY + Mathf.Sin(Time.time * speed) * amp;
        transform.localScale = new Vector3(baseScale.x, s, baseScale.z);
    }
}