using UnityEngine;

public class Generator : MonoBehaviour
{
    [Header("Generator Settings")]
    public float noiseScale;
    public int octaves;
    public float lacunarity;
    public float persistance;
    public float falloff;

    [Header("Quality Settings")]
    public int previewResolution;
    public int exportResolution;

    SpriteRenderer spriteRenderer;
    Exporter exporter;
    float[,] heightmap;

    struct NoiseGenSettings
    {
        public float noiseScale;
        public int octaves;
        public float lacunarity;
        public float persistance;
        public float falloff;

        public NoiseGenSettings(float noiseScale, int octaves, float lacunarity, float persistance, float falloff)
        {
            this.noiseScale = noiseScale;
            this.octaves = octaves;
            this.lacunarity = lacunarity;
            this.persistance = persistance;
            this.falloff = falloff;
        }
    }

    private void Awake()
    {
        exporter = GetComponent<Exporter>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        heightmap = Generate(previewResolution, new NoiseGenSettings(noiseScale, octaves, lacunarity, persistance, falloff));
        Texture2D outTex = GenerateTexture(heightmap);
        //Converts texture to a sprite so it can be displayed via a sprite renderer
        outTex.filterMode = FilterMode.Point;
        Sprite outSprite = Sprite.Create(outTex, new Rect(0f, 0f, heightmap.GetLength(0), heightmap.GetLength(1)), new Vector2(0.5f, 0.5f), previewResolution);
        spriteRenderer.sprite = outSprite;
    }


    public async void GenerateHighResOutputAsync()
    {
        heightmap = await GenerateAsync(exportResolution);
        Texture2D outTex = GenerateTexture(heightmap);
        exporter.Export(outTex);
    }

    // Creates the noisemap and calls the generate output function
    float[,] Generate(int resolution, NoiseGenSettings settings)
    {
        float[,] generatedHeightmap = new float[resolution, resolution];
        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                // Makes sample pos between 0 and 1
                float sampleX = (float)x / resolution;
                float sampleY = (float)y / resolution;
                generatedHeightmap[x, y] = Sample(sampleX, sampleY);
            }
        }
        return generatedHeightmap;
    }

    async Awaitable<float[,]> GenerateAsync(int resolution)
    {
        await Awaitable.BackgroundThreadAsync();
        float[,] output = Generate(resolution, new NoiseGenSettings(noiseScale, octaves, lacunarity, persistance, falloff));
        await Awaitable.MainThreadAsync();
        return output;
    }

    Texture2D GenerateTexture(float[,] texHeightmap)
    {
        //Generates texture from the heightmap
        Texture2D outTex = new Texture2D(texHeightmap.GetLength(0), texHeightmap.GetLength(1));
        for (int x = 0; x < texHeightmap.GetLength(0); x++)
        {
            for (int y = 0; y < texHeightmap.GetLength(1); y++)
            {
                outTex.SetPixel(x, y, new Color(texHeightmap[x, y], texHeightmap[x, y], texHeightmap[x, y], 1f));
            }
        }
        outTex.Apply();
        return outTex;
        
    }

    float Sample(float x, float y)
    {
        float value = 0f;

        float amplitude = 1f;
        float frequency = 1f;
        
        for (int octave = 0; octave < octaves; octave++)
        {
            //for each octave add to the perlin noise value and change the amplitude and frequency for the next octave
            float sampleX = x / noiseScale * frequency;
            float sampleY = y / noiseScale * frequency;

            float perlin = (Mathf.PerlinNoise(sampleX, sampleY) * 2) - 1; //Makes the noise from 0 - 1 to -1 to 1
            value += perlin * amplitude;
            amplitude *= persistance;
            frequency *= lacunarity;
        }
        value = Mathf.Clamp(value, -1f, 1f);
        return ((value + 1f) / 2f) * CalculateFalloff(x, y); // Puts the noise back to 0 - 1 from -1 to 1
    }

    float CalculateFalloff(float x, float y)
    {
        float distanceToCenter = Vector2.Distance(new Vector2(0.5f, 0.5f), new Vector2(x, y));
        float maxDistanceToCenter = Mathf.Sqrt(0.5f);
        float distanceScalar = distanceToCenter / maxDistanceToCenter;
        distanceScalar *= falloff;
        distanceScalar *= -1;
        distanceScalar += 1;
        distanceScalar = Mathf.Clamp(distanceScalar, 0f, 1f);
        return distanceScalar;
    }
}
