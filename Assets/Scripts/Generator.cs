using UnityEngine;

public class Generator : MonoBehaviour
{
    [Header("Generator Settings")]
    [SerializeField] float noiseScale;

    [Header("Quality Settings")]
    [SerializeField] int previewResolution;
    [SerializeField] int exportResolution;

    SpriteRenderer spriteRenderer;
    float[,] heightmap;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        Generate(previewResolution);
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
                float sampleX = (float)x / resolution;
                float sampleY = (float)y / resolution;
                heightmap[x, y] = Sample(sampleX, sampleY);
            }
        }
        GenerateOutput(resolution);
    }

    void GenerateOutput(int resolution)
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
        spriteRenderer.sprite = outSprite;
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
