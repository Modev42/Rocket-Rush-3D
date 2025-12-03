using UnityEngine;

public class RocketSkin2D : MonoBehaviour
{
    public SpriteRenderer body;
    public SpriteRenderer jet;

    // 0=Kırmızı, 1=Mavi, 2=Sarı
    public void ApplyColor(int colorIndex)
    {
        Color c = Color.white;
        switch (colorIndex)
        {
            case 0: c = new Color(1f, 0.2f, 0.2f); break;
            case 1: c = new Color(0.2f, 0.55f, 1f); break;
            case 2: c = new Color(1f, 0.85f, 0.2f); break;
        }
        if (body) body.color = c;
        if (jet)  jet.color  = c;
    }
}