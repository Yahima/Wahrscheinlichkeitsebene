using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Tile
{
    private bool collapsed;
    private string type;
    private List<string> validTypes;
    private readonly List<TileData> tileData;
    private Vector2Int gridPosition;
    private readonly GameObject tileObject;

    private readonly int probability;
    private readonly int cellValue;

    public Tile(RectTransform container, Vector2 position, Vector2 size, List<string> types, List<TileData> data, int x, int y, int probability, int cellValue)
    {
        collapsed = false;
        validTypes = types;
        tileData = data;
        gridPosition = new Vector2Int(x, y);

        tileObject = new GameObject(x + ":" + y);

        RectTransform trans = tileObject.AddComponent<RectTransform>();
        trans.transform.SetParent(container);
        trans.localScale = Vector3.one;
        trans.anchoredPosition = position;
        trans.sizeDelta = size;

        tileObject.AddComponent<Image>();

        this.probability = probability;
        this.cellValue = cellValue;
    }

    public void CollapseToType(string type)
    {
        ResetTile();
        this.type = type;
        collapsed = true;
    }

    public void WeightedCollapse()
    {
        List<TileData> filteredTileList = tileData.Where(tile => validTypes.Contains(tile.name)).ToList();
        type = GetTypeByProbability(filteredTileList);
        collapsed = true;
    }

    public void ResetTile()
    {
        collapsed = false;
        tileObject.GetComponent<Image>().sprite = null;
    }

    public string GetTypeByProbability(List<TileData> tiles)
    {
        List<string> weightedTypeList = new();

        foreach (var tile in tiles)
            if (tile.value == cellValue)
                for (int i = 0; i < probability; i++)
                    weightedTypeList.Add(tile.name);

            else
                weightedTypeList.Add(tile.name);

        System.Random random = new();
        return weightedTypeList[random.Next(0, weightedTypeList.Count)];
    }

    public void RemoveType(string type)
    {
        validTypes.Remove(type);
    }

    public float GetEntropy()
    {
        float sumWeight = 0;
        float sumWeightLogWeight = 0;
        List<TileData> filteredTileList = tileData.Where(tile => validTypes.Contains(tile.name)).ToList();

        if (filteredTileList.Count == 0)
            return 0f;

        foreach (TileData tile in filteredTileList)
        {
            int weight = tile.weight;
            if (tile.value == cellValue)
            {
                weight += probability;
            }
            sumWeight += weight;
            sumWeightLogWeight += weight * Mathf.Log(weight);
        }

        float entropy = Mathf.Log(sumWeight) - (sumWeightLogWeight / sumWeight);

        return entropy;
    }

    public bool IsCollapsed()
    {
        return collapsed;
    }

    public string GetTileType()
    {
        return type;
    }

    public List<string> GetValidTypes()
    {
        return validTypes;
    }

    public void SetValidTypes(List<string> valids)
    {
        validTypes = valids;
    }

    public Vector2Int GetGridPosition()
    {
        return gridPosition;
    }

    public GameObject GetTileObject()
    {
        return tileObject;
    }

    public void SetObjectImage(Sprite sprite)
    {
        tileObject.GetComponent<Image>().sprite = sprite;
    }

    public bool ObjectImageIsEmpty()
    {
        return tileObject.GetComponent<Image>().sprite == null;
    }
}

