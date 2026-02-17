using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("Algorithm UI")]
    public TMP_Dropdown algorithmDropdown;
    public Pathfinder pathfinder;

    [Header("Obstacle/Terrain Dropdown")]
    public TMP_Dropdown obstacleTypeDropdown;

    [Header("Add Terrain Panel")]
    public GameObject addTerrainPanel;
    public TMP_InputField terrainNameInput;
    public TMP_InputField terrainCostInput;
    public Image colorPreview;
    public Button addTerrainButton;
    public Button saveTerrainButton;
    public Button cancelTerrainButton;

    [Header("Terrain Definitions")]
    public Dictionary<string, TerrainType> terrainTypes = new();

    public FlexibleColorPicker colorPicker;

    void Start()
    {
        InitializeDefaultTerrains();

        // UI button events
        addTerrainButton.onClick.AddListener(() => addTerrainPanel.SetActive(true));
        cancelTerrainButton.onClick.AddListener(() => addTerrainPanel.SetActive(false));
        saveTerrainButton.onClick.AddListener(OnAddTerrainClicked);

        // Hide panel on startup
        addTerrainPanel.SetActive(false);
    }

    private void Update()
    {
        // Update color preview based on color picker
        if (colorPicker != null && colorPreview != null)
        {
            colorPreview.color = colorPicker.color;
        }
    }
    void InitializeDefaultTerrains()
    {
        AddTerrainType(new TerrainType("Normal", 1f,new Color(0.4415f,0.1990f,0.3892f,1f)));
        AddTerrainType(new TerrainType("Wall", Mathf.Infinity, Color.white));
        AddTerrainType(new TerrainType("Water", 2f, Color.blue));
        AddTerrainType(new TerrainType("Mud", 3f, new Color(0.5f, 0.25f, 0f))); // Brown
        AddTerrainType(new TerrainType("Lava", 4f, Color.red));
        AddTerrainType(new TerrainType("Sand", 5f, Color.gray));
    }

    public void AddTerrainType(TerrainType type)
    {
        if (!terrainTypes.ContainsKey(type.name))
        {
            terrainTypes[type.name] = type;
            obstacleTypeDropdown.options.Add(new TMP_Dropdown.OptionData(type.name));
            obstacleTypeDropdown.RefreshShownValue();
        }
    }

    public void OnAddTerrainClicked()
    {
        string name = terrainNameInput.text.Trim();

        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("Terrain name cannot be empty.");
            return;
        }

        if (terrainTypes.ContainsKey(name))
        {
            Debug.LogWarning("Terrain with this name already exists.");
            return;
        }

        if (!float.TryParse(terrainCostInput.text, out float cost))
        {
            Debug.LogWarning("Invalid weight/cost input.");
            return;
        }

        Color color = colorPicker.color;

        AddTerrainType(new TerrainType(name, cost, color));

        // Clear fields and close panel
        terrainNameInput.text = "";
        terrainCostInput.text = "";
        addTerrainPanel.SetActive(false);
    }

    public string GetSelectedObstacleType()
    {
        return obstacleTypeDropdown.options[obstacleTypeDropdown.value].text;
    }

    public TerrainType GetSelectedTerrain()
    {
        string name = GetSelectedObstacleType();
        return terrainTypes.ContainsKey(name) ? terrainTypes[name] : terrainTypes["Normal"];
    }

    public void VisualizeAlgorithm()
    {
        switch (algorithmDropdown.value)
        {
            case 0: pathfinder.RunBFS(); break;
            case 1: pathfinder.RunDFS(); break;
            case 2: pathfinder.RunDijkstra(); break;
            case 3: pathfinder.RunAStar(); break;
            default: Debug.LogWarning("Unknown algorithm selected"); break;
        }
    }
}
