using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlendTwoMaterialsTest : MonoBehaviour
{
    public GameObject blendGameObject;
    public Material materialToBlend1;
    public Material materialToBlend2;

    private Material materialOfBlendGameObject;
    private void Start()
    {
        //textureOfMaterial1 = materialToBlend1.GetTexture("_MainTex") as Texture2D;
        //textureOfMaterial2 = materialToBlend1.GetTexture("_MainTex") as Texture2D;

        // Get the renderer component of the GameObject
        Renderer renderer = blendGameObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Get the first material of the renderer
            materialOfBlendGameObject = renderer.material;
            // Do something with the material
        }

        // Create a new Texture2D with the same width and height as the material
        Texture2D texture1 = new Texture2D(materialToBlend1.mainTexture.width, materialToBlend1.mainTexture.height, TextureFormat.ARGB32, false);
        // Copy the pixels from the material's main texture to the new Texture2D
        texture1.SetPixels(((Texture2D)materialToBlend1.mainTexture).GetPixels());
        // Apply the changes to the Texture2D
        texture1.Apply();
        // Do something with the texture, such as assigning it to a renderer's 

        // Create a new Texture2D with the same width and height as the material
        Texture2D texture2 = new Texture2D(materialToBlend2.mainTexture.width, materialToBlend2.mainTexture.height, TextureFormat.ARGB32, false);
        // Copy the pixels from the material's main texture to the new Texture2D
        texture2.SetPixels(((Texture2D)materialToBlend2.mainTexture).GetPixels());
        // Apply the changes to the Texture2D
        texture2.Apply();
        // Do something with the texture, such as assigning it to a renderer's 

        renderer.material.SetTexture("_MainTex1_ST2", texture1);
        renderer.material.SetTexture("_MainTex2_ST2", texture2);
    }

    void Update()
    {
        //float blendValue = 0.5f; // Example value, you can change it to whatever you want
        //blendMaterial.SetFloat("_Blend", blendValue);
        //materialOfBlendGameObject.color = Color.red;
    }
}
