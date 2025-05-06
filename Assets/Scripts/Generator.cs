using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Generator : MonoBehaviour
{
    [Header("Generator Settings")]
    public float noiseScale;
    public int octaves;
    public float lacunarity;
    public float persistance;
    public float falloff;
    public Vector2 seed = Vector2.zero;

    [Header("Quality Settings")]
    public int previewResolution;
    public int exportResolution;

    SpriteRenderer spriteRenderer;
    Exporter exporter;
    float[,] heightmap;

    string exportText = "";
    bool highResExporting = false;
    struct NoiseGenSettings
    {
        public float noiseScale;
        public int octaves;
        public float lacunarity;
        public float persistance;
        public float falloff;
        public Vector2 seed;

        public NoiseGenSettings(float noiseScale, int octaves, float lacunarity, float persistance, float falloff, Vector2 seed)
        {
            this.noiseScale = noiseScale;
            this.octaves = octaves;
            this.lacunarity = lacunarity;
            this.persistance = persistance;
            this.falloff = falloff;
            this.seed = seed;
        }
    }

    private void Awake()
    {
        exporter = GetComponent<Exporter>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void RandomiseSeed()
    {
        float x = Random.Range(-9999f, 9999f);
        float y = Random.Range(-9999f, 9999f);
        seed = new Vector2(x, y);
    }

    private void Update()
    {
        heightmap = Generate(previewResolution, new NoiseGenSettings(noiseScale, octaves, lacunarity, persistance, falloff, seed));
        Texture2D outTex = GenerateTexture(heightmap);
        //Converts texture to a sprite so it can be displayed via a sprite renderer
        outTex.filterMode = FilterMode.Point; //Makes sure its not blurry
        Sprite outSprite = Sprite.Create(outTex, new Rect(0f, 0f, heightmap.GetLength(0), heightmap.GetLength(1)), new Vector2(0.5f, 0.5f), previewResolution);
        spriteRenderer.sprite = outSprite;

        if (highResExporting) exporter.outputLog.text = exportText;
    }


    public async void GenerateHighResOutputAsync()
    {
        if (highResExporting) return; //make sure multiple exports are not going on at once
        heightmap = await GenerateAsync(exportResolution);
        Texture2D outTex = GenerateTexture(heightmap);
        exporter.Export(outTex);
    }


    // Creates the noisemap and calls the generate output function
    float[,] Generate(int resolution, NoiseGenSettings settings, bool log = false)
    {
        float[,] generatedHeightmap = new float[resolution, resolution];

        int pixelsToRun = resolution * resolution;
        int currentPixel = 0;

        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                // Makes sample pos between 0 and 1 so that resoltion doesnt affect results
                float sampleX = (float)x / resolution;
                float sampleY = (float)y / resolution;
                generatedHeightmap[x, y] = Sample(sampleX, sampleY, settings);
                currentPixel++;

                if (log)
                {
                    //Gets the completion progress
                    int progressPercentage = Mathf.RoundToInt((float)currentPixel / pixelsToRun * 100);
                    exportText = $"Export Progress: {progressPercentage}%";
                }
            }
        }
        return generatedHeightmap;
    }

    async Awaitable<float[,]> GenerateAsync(int resolution)
    {
        highResExporting = true;

        //Switches to the background thread
        await Awaitable.BackgroundThreadAsync();

        float[,] output = Generate(resolution, new NoiseGenSettings(noiseScale, octaves, lacunarity, persistance, falloff, seed), true);

        //Returns to the main thread
        await Awaitable.MainThreadAsync();
        highResExporting = false;
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
                //Writes the pixels from the heightmap to the texture
                outTex.SetPixel(x, y, new Color(texHeightmap[x, y], texHeightmap[x, y], texHeightmap[x, y], 1f));
            }
        }
        outTex.Apply();
        return outTex;
        
    }

    //Samples a point between x 0 - 1 and y 0 - 1 and returns a noise value between 0 - 1
    float Sample(float x, float y, NoiseGenSettings noiseGenSettings)
    {
        float value = 0f;

        float amplitude = 1f;
        float frequency = 1f;
        
        for (int octave = 0; octave < noiseGenSettings.octaves; octave++)
        {
            //for each octave add to the perlin noise value and change the amplitude and frequency for the next octave
            float sampleX = (x + noiseGenSettings.seed.x) / noiseGenSettings.noiseScale * frequency;
            float sampleY = (y + noiseGenSettings.seed.y) / noiseGenSettings.noiseScale * frequency;

            float perlin = (Mathf.PerlinNoise(sampleX, sampleY) * 2) - 1; //Makes the noise from 0 - 1 to -1 to 1
            value += perlin * amplitude;
            amplitude *= noiseGenSettings.persistance;
            frequency *= noiseGenSettings.lacunarity;
        }
        value = Mathf.Clamp(value, -1f, 1f);
        return ((value + 1f) / 2f) * CalculateFalloff(x, y, noiseGenSettings); // Puts the noise back to 0 - 1 from -1 to 1
    }

    //Makes points further from the centre lower
    float CalculateFalloff(float x, float y, NoiseGenSettings noiseGenSettings)
    {
        float distanceToCenter = Vector2.Distance(new Vector2(0.5f, 0.5f), new Vector2(x, y));
        float maxDistanceToCenter = Mathf.Sqrt(0.5f);
        float distanceScalar = distanceToCenter / maxDistanceToCenter;
        distanceScalar *= noiseGenSettings.falloff;
        distanceScalar *= -1;
        distanceScalar += 1;
        distanceScalar = Mathf.Clamp(distanceScalar, 0f, 1f);
        return distanceScalar;
    }
}
