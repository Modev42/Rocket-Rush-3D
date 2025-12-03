using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    public Material redBackground;
    public Material blueBackground;
    public Material yellowBackground;
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend == null)
            Debug.LogWarning("BackgroundManager: Renderer bulunamadı!");
    }

    public enum BgType { Red, Blue, Yellow }

    public void ChangeBackground(BgType type)
    {
        if (!rend) return;

        switch (type)
        {
            case BgType.Red: rend.material = redBackground; break;
            case BgType.Blue: rend.material = blueBackground; break;
            case BgType.Yellow: rend.material = yellowBackground; break;
        }
    }
}
