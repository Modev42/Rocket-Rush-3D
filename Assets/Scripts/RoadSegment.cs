using UnityEngine;

public class RoadSegment : MonoBehaviour
{
    public Renderer leftStripe;
    public Renderer rightStripe;
    
    public Material redMaterial;
    public Material blueMaterial;
    public Material yellowMaterial;
    
    void Start()
    {
        // Her şerit için materyal instance oluştur
        if (leftStripe != null)
        {
            leftStripe.material = new Material(leftStripe.material);
        }
        if (rightStripe != null)
        {
            rightStripe.material = new Material(rightStripe.material);
        }
    }
    
    public void SetStripeColor(int colorIndex)
    {
        Material mat = null;
        
        switch (colorIndex)
        {
            case 0: mat = redMaterial; break;
            case 1: mat = blueMaterial; break;
            case 2: mat = yellowMaterial; break;
        }
        
        if (mat != null)
        {
            // Yeni materyal instance kullan
            if (leftStripe != null)
            {
                leftStripe.material = new Material(mat);
            }
            if (rightStripe != null)
            {
                rightStripe.material = new Material(mat);
            }
        }
    }
}