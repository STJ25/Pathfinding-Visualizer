using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridManager : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public float spacing = 5f;

    public GameObject nodePrefab;
    public Transform gridParent;

    private Node[,] grid;

    public Node currentStartNode;
    public Node currentEndNode;

    private bool isDraggingStart = false;
    private bool isDraggingEnd = false;
    private RectTransform draggingIcon;

    public bool IsDragging => isDraggingStart || isDraggingEnd;

    private Node previousNode; // To track previous node before dragging

    void Start()
    {
        GenerateGrid();
        SetInitialStartAndEnd();
    }

    void Update()
    {
        if (draggingIcon != null)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                gridParent as RectTransform,
                Input.mousePosition,
                null,
                out pos
            );
            draggingIcon.anchoredPosition = pos;
        }

    }

    void GenerateGrid()
    {

        grid = new Node[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject nodeObj = Instantiate(nodePrefab, gridParent);
                RectTransform rt = nodeObj.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(x * (rt.sizeDelta.x + spacing), -y * (rt.sizeDelta.y + spacing));

                Node node = nodeObj.GetComponent<Node>();
                node.gridManager = this;
                node.uiManager = FindFirstObjectByType<UIManager>(); // Simple solution
                node.gridPosition = new Vector2Int(x, y);

                node.startIcon = nodeObj.transform.Find("StartIcon")?.GetComponent<RectTransform>().gameObject;
                node.endIcon = nodeObj.transform.Find("EndIcon")?.GetComponent<RectTransform>().gameObject;

                grid[x, y] = node;
                

            }
        }
    }

    void SetInitialStartAndEnd()
    {
        currentStartNode = grid[0, 0];
        currentEndNode = grid[1, 0];

        currentStartNode.SetAsStart(true);
        currentEndNode.SetAsEnd(true);

        AddDragEvents(currentStartNode.startIcon, true);
        AddDragEvents(currentEndNode.endIcon, false);
    }

    void AddDragEvents(GameObject icon, bool isStartIcon)
    {
        if (icon == null) return;

        EventTrigger trigger = icon.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = icon.AddComponent<EventTrigger>();

        trigger.triggers.Clear();

        var entryDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        entryDown.callback.AddListener((data) => {
            BeginDrag(icon.GetComponent<RectTransform>(), isStartIcon);
        });
        trigger.triggers.Add(entryDown);

        var entryUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        entryUp.callback.AddListener((data) => {
            EndDrag(isStartIcon);
        });
        trigger.triggers.Add(entryUp);
    }

    void BeginDrag(RectTransform icon, bool isStart)
    {
        draggingIcon = icon;
        isDraggingStart = isStart;
        isDraggingEnd = !isStart;
        icon.SetAsLastSibling();

        previousNode = isStart ? currentStartNode : currentEndNode;
    }

    void EndDrag(bool isStart)
    {
        Node targetNode = GetNodeUnderMouse();
        if (targetNode == null || targetNode == (isStart ? currentEndNode : currentStartNode))
        {
            // Invalid drop (overlapping with the other icon), snap back to previous
            draggingIcon.SetParent(previousNode.transform, false);
            draggingIcon.anchoredPosition = Vector2.zero;
            draggingIcon = null;
            isDraggingStart = false;
            isDraggingEnd = false;
            return;
        }

        if (isStart)
        {
            if (currentStartNode != null) currentStartNode.SetAsStart(false);
            currentStartNode = targetNode;
            currentStartNode.SetAsStart(true);

            draggingIcon.SetParent(targetNode.transform, false);
            draggingIcon.anchoredPosition = Vector2.zero;

            AddDragEvents(currentStartNode.startIcon, true);
        }
        else
        {
            if (currentEndNode != null) currentEndNode.SetAsEnd(false);
            currentEndNode = targetNode;
            currentEndNode.SetAsEnd(true);

            draggingIcon.SetParent(targetNode.transform, false);
            draggingIcon.anchoredPosition = Vector2.zero;

            AddDragEvents(currentEndNode.endIcon, false);
        }

        draggingIcon = null;
        isDraggingStart = false;
        isDraggingEnd = false;
    }

    public Node GetNodeAtPosition(Vector2Int pos)
    {
        return grid[pos.x, pos.y];
    }

    Node GetNodeUnderMouse()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> rayResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, rayResults);

        foreach (var result in rayResults)
        {
            Node node = result.gameObject.GetComponent<Node>();
            if (node != null)
                return node;
        }

        return null;
    }

    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        int x = node.gridPosition.x;
        int y = node.gridPosition.y;

        Vector2Int[] directions = new Vector2Int[]
        {
        new Vector2Int(0, 1),   // Up
        new Vector2Int(1, 0),   // Right
        new Vector2Int(0, -1),  // Down
        new Vector2Int(-1, 0),  // Left
        new Vector2Int(1, 1),   // Up-Right
        new Vector2Int(1, -1),  // Down-Right
        new Vector2Int(-1, -1), // Down-Left
        new Vector2Int(-1, 1),  // Up-Left
        };

        foreach (Vector2Int dir in directions)
        {
            int newX = x + dir.x;
            int newY = y + dir.y;

            if (newX >= 0 && newX < width && newY >= 0 && newY < height)
            {
                Node neighbor = grid[newX, newY];
                if (neighbor != null)
                {
                    neighbors.Add(neighbor);
                }
            }
        }

        return neighbors;
    }


    public List<Node> GetAllNodes()
    {
        List<Node> allNodes = new List<Node>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                allNodes.Add(grid[x, y]);
            }
        }

        return allNodes;
    }


}
