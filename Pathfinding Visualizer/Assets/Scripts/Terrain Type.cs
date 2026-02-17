using UnityEngine;

[System.Serializable]
public class TerrainType
{
    public string name;
    public float cost;
    public Color color;

    public TerrainType(string name, float cost, Color color)
    {
        this.name = name;
        this.cost = cost;
        this.color = color;
    }
}