using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Node : MonoBehaviour, IPointerClickHandler
{
    public Vector2Int gridPosition;
    public string terrainType = "Normal";

    public GameObject startIcon;
    public GameObject endIcon;
    public GridManager gridManager;
    public UIManager uiManager;

    private Image image;
    private Color originalColor;

    void Awake()
    {
        image = GetComponent<Image>();
        originalColor = image.color;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (gridManager != null && gridManager.IsDragging) return;
        if (startIcon != null && startIcon.activeSelf) return;
        if (endIcon != null && endIcon.activeSelf) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            TerrainType selected = uiManager.GetSelectedTerrain();
            ApplyTerrainType(selected);
        }
    }

    public void ApplyTerrainType(TerrainType type)
    {
        terrainType = type.name;
        SetColor(type.color);
    }

    public void SetAsStart(bool isStart)
    {
        if (startIcon != null)
            startIcon.SetActive(isStart);
    }

    public void SetAsEnd(bool isEnd)
    {
        if (endIcon != null)
            endIcon.SetActive(isEnd);
    }

    public void SetColor(Color color)
    {
        image.color = color;
    }

    public void ResetColor()
    {
        image.color = originalColor;
    }

    public float GetTraversalCost()
    {
        return uiManager.terrainTypes.ContainsKey(terrainType)
            ? uiManager.terrainTypes[terrainType].cost
            : 1f;
    }

    public bool IsBlocked()
    {
        return uiManager.terrainTypes.ContainsKey(terrainType) && float.IsInfinity(uiManager.terrainTypes[terrainType].cost);
    }

    public void ResetNode()
    {
        terrainType = "Normal";
        ResetColor();
    }
}
