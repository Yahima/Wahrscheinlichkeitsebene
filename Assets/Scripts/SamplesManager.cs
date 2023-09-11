using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SamplesManager
{
    private readonly Dictionary<string, Sprite> sprites;
    private Dictionary<string, Dictionary<Dir, List<string>>> rules;
    private readonly List<string> types;
    private readonly List<TileData> data;

    private readonly string samplesPath;
    private readonly string xmlFilePath;
    private readonly int tileSize;
    private readonly FilterMode filterMode;

    private string tileHash;
    private string adjacentTileHash;

    public SamplesManager(string samplesPath, string xmlFilePath, int tileSize, FilterMode filterMode)
    {
        this.samplesPath = samplesPath;
        this.xmlFilePath = xmlFilePath;
        this.tileSize = tileSize;
        this.filterMode = filterMode;

        sprites = new Dictionary<string, Sprite>();
        rules = new Dictionary<string, Dictionary<Dir, List<string>>>();
        types = new List<string>();
        data = new List<TileData>();

        ExtractTilesFromSamples(true);
    }

    private void ExtractTilesFromSamples(bool generateRules)
    {
        foreach (Object obj in Resources.LoadAll(samplesPath, typeof(Texture2D)))
        {
            Texture2D sampleTexture = (Texture2D)obj;

            for (int i = 0; i < sampleTexture.width; i += tileSize)
            {
                for (int j = 0; j < sampleTexture.height; j += tileSize)
                {
                    Texture2D tileTexture = new(tileSize, tileSize);
                    tileTexture.SetPixels(sampleTexture.GetPixels(i, j, tileSize, tileSize));
                    tileTexture.filterMode = filterMode;
                    tileTexture.Apply();
                    tileHash = GenerateTextureHash(tileTexture);

                    if (!rules.Keys.Contains(tileHash))
                    {
                        Sprite sprite = Sprite.Create(tileTexture, new Rect(0.0f, 0.0f, tileTexture.width, tileTexture.height), new Vector2(0.5f, 0.5f));
                        sprites[tileHash] = sprite;

                        types.Add(tileHash);

                        string filename = Path.GetFileNameWithoutExtension(obj.name);
                        int value = int.Parse(filename[^3..]);
                        int weight = 1;

                        TileData tileData = new(tileHash, value, weight);
                        data.Add(tileData);

                        if (generateRules)
                        {
                            Dictionary<Dir, List<string>> validsForDirection = new()
                            {
                                { Dir.Up, new List<string>() },
                                { Dir.Right, new List<string>() },
                                { Dir.Down, new List<string>() },
                                { Dir.Left, new List<string>() }
                            };

                            rules.Add(tileHash, validsForDirection);
                            GetAdjacentTilesFromSample(sampleTexture, tileHash);
                        }

                    }

                    else
                    {
                        TileData existingTile = data.FirstOrDefault(tile => tile.name == tileHash);
                        existingTile.weight += 1;

                        if (generateRules)
                            GetAdjacentTilesFromSample(sampleTexture, tileHash);
                    }
                }
            }
        }
    }

    private void GetAdjacentTilesFromSample(Texture2D sampleTexture, string tileHash)
    {
        for (int i = 0; i < sampleTexture.width; i += tileSize)
        {
            for (int j = 0; j < sampleTexture.height; j += tileSize)
            {
                Texture2D tileTexture = new(tileSize, tileSize);
                tileTexture.SetPixels(sampleTexture.GetPixels(i, j, tileSize, tileSize));
                string tileTextureHash = GenerateTextureHash(tileTexture);

                if (tileTextureHash == tileHash)
                {
                    if (j < sampleTexture.height - tileSize)
                    {
                        Texture2D adjacentTileTexture = new(tileSize, tileSize);
                        adjacentTileTexture.SetPixels(sampleTexture.GetPixels(i, j + tileSize, tileSize, tileSize));
                        adjacentTileHash = GenerateTextureHash(adjacentTileTexture);

                        AddAdjacent(Dir.Up, tileHash, adjacentTileHash);
                    }

                    if (i < sampleTexture.width - tileSize)
                    {
                        Texture2D adjacentTileTexture = new(tileSize, tileSize);
                        adjacentTileTexture.SetPixels(sampleTexture.GetPixels(i + tileSize, j, tileSize, tileSize));
                        adjacentTileHash = GenerateTextureHash(adjacentTileTexture);

                        AddAdjacent(Dir.Right, tileHash, adjacentTileHash);
                    }

                    if (j > 0)
                    {
                        Texture2D adjacentTileTexture = new(tileSize, tileSize);
                        adjacentTileTexture.SetPixels(sampleTexture.GetPixels(i, j - tileSize, tileSize, tileSize));
                        adjacentTileHash = GenerateTextureHash(adjacentTileTexture);

                        AddAdjacent(Dir.Down, tileHash, adjacentTileHash);
                    }

                    if (i > 0)
                    {
                        Texture2D adjacentTileTexture = new(tileSize, tileSize);
                        adjacentTileTexture.SetPixels(sampleTexture.GetPixels(i - tileSize, j, tileSize, tileSize));
                        adjacentTileHash = GenerateTextureHash(adjacentTileTexture);

                        AddAdjacent(Dir.Left, tileHash, adjacentTileHash);
                    }
                }
            }
        }
    }

    private void AddAdjacent(Dir dir, string tileHash, string adjacentTileHash)
    {
        if (!rules[tileHash][dir].Contains(adjacentTileHash))
            rules[tileHash][dir].Add(adjacentTileHash);
    }

    private string GenerateTextureHash(Texture2D texture)
    {
        byte[] bytes = texture.EncodeToPNG();
        string hash = System.Convert.ToBase64String(bytes);
        return hash;
    }

    public Dictionary<string, Sprite> GetSprites()
    {
        return sprites;
    }

    public Dictionary<string, Dictionary<Dir, List<string>>> GetRules()
    {
        return rules;
    }

    public List<string> GetTypes()
    {
        return types;
    }

    public List<TileData> GetTileData()
    {
        return data;
    }
}