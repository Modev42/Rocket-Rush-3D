using UnityEngine;

public class GapScoreTrigger : MonoBehaviour
{
    public int points = 3;
    public string playerTag = "Player";
    bool awarded = false;

    void OnTriggerEnter(Collider other)
    {
        if (awarded) return;
        if (!other.CompareTag(playerTag)) return;

        if (GameManager.instance != null)
            GameManager.instance.AddDodgeScore(); // +3

        awarded = true;
        Destroy(gameObject, 0.05f);
    }
}
