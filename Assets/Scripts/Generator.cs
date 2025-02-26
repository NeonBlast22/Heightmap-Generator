using UnityEngine;

public class Generator : MonoBehaviour
{
    [SerializeField] float noiseScale;

    float[,] heightmap;
    int resolution = 256;

    void Gen()
    {
        Generate(resolution);
    }

    private void Start()
    {
        Generate(resolution);
    }

    // Creates the noisemap and calls the generate output function
    void Generate(int resolution)
    {
        heightmap = new float[resolution, resolution];
        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                // Makes sample pos between 0 and 1
                float sampleX = x / resolution;
                float sampleY = y / resolution;
                heightmap[x, y] = Sample(sampleX, sampleY);
            }
        }
        GenerateOutput();
    }

    void GenerateOutput()
    {
        //Generates texture from noise
        Texture2D outTex = new Texture2D(heightmap.GetLength(0), heightmap.GetLength(1));
        for (int x = 0; x < heightmap.GetLength(0); x++)
        {
            for (int y = 0; y < heightmap.GetLength(1); y++)
            {
                outTex.SetPixel(x, y, new Color(heightmap[x, y], heightmap[x, y], heightmap[x, y], 1f));
            }
        }
        outTex.Apply();

        //Converts texture to a sprite so it can be displayed via a sprite renderer
        outTex.filterMode = FilterMode.Point;
        Sprite outSprite = Sprite.Create(outTex, new Rect(0f, 0f, heightmap.GetLength(0), heightmap.GetLength(1)), new Vector2(0.5f, 0.5f), resolution);
    }

    float Sample(float x, float y)
    {
        float value = 0f;

        value = Mathf.PerlinNoise(x * noiseScale, y * noiseScale);
        value += 1;
        value /= 2;
        
        return value;
    }
}
