using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.UI;

public class Generator : MonoBehaviour
{
    [SerializeField] SpriteRenderer output;
    [SerializeField] float noiseScale;
    [SerializeField] Button regenerateButton;

    float[,] heightmap;
    Vector2Int size = new Vector2Int(1080, 1080);

    private void Awake()
    {
        regenerateButton.onClick.AddListener(Gen);
    }

    void Gen()
    {
        Generate(size.x, size.y);
    }

    private void Start()
    {
        Generate(size.x, size.y);
    }

    void Generate(int xSize, int ySize)
    {
        heightmap = new float[xSize, ySize];
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                heightmap[x, y] = Sample(x, y);
            }
        }
        GenerateOutput();
    }

    void GenerateOutput()
    {
        Texture2D outTex = new Texture2D(heightmap.GetLength(0), heightmap.GetLength(1));
        for (int x = 0; x < heightmap.GetLength(0); x++)
        {
            for (int y = 0; y < heightmap.GetLength(1); y++)
            {
                outTex.SetPixel(x, y, new Color(heightmap[x, y], heightmap[x, y], heightmap[x, y], 1f));
            }
        }
        outTex.Apply();
        outTex.filterMode = FilterMode.Point;
        Sprite outSprite = Sprite.Create(outTex, new Rect(0f, 0f, heightmap.GetLength(0), heightmap.GetLength(1)), new Vector2(0.5f, 0.5f), 1080f);
        
        output.sprite = outSprite;
    }

    float Sample(int x, int y)
    {
        float value = 0f;

        value = noise.snoise(new float2((float)x / noiseScale, (float)y / noiseScale));
        value += 1;
        value /= 2;
        
        return value;
    }
}
