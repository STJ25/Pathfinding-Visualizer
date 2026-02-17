using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    public GridManager gridManager;
    public float visualDelay = 0.02f;

    public void RunBFS()
    {
        StopAllCoroutines();
        StartCoroutine(BFS());
    }

    public void RunDFS()
    {
        StopAllCoroutines();
        StartCoroutine(DFS());
    }

    public void RunDijkstra()
    {
        StopAllCoroutines();
        StartCoroutine(Dijkstra());
    }

    public void RunAStar()
    {
        StopAllCoroutines();
        StartCoroutine(AStar());
    }

    public void ClearBoard()
    {
        foreach (Node node in gridManager.GetAllNodes())
        {
            if (node != gridManager.currentStartNode && node != gridManager.currentEndNode)
            {
                node.ResetNode();
            }
        }
    }

    IEnumerator BFS()
    {
        Node startNode = gridManager.currentStartNode;
        Node endNode = gridManager.currentEndNode;

        Queue<Node> queue = new Queue<Node>();
        HashSet<Node> visited = new HashSet<Node>();
        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();

        queue.Enqueue(startNode);
        visited.Add(startNode);

        while (queue.Count > 0)
        {
            Node current = queue.Dequeue();

            if (current != startNode && current != endNode)
                current.SetColor(Color.cyan);

            if (current == endNode)
            {
                yield return StartCoroutine(ReconstructPath(cameFrom, startNode, endNode));
                yield break;
            }

            foreach (Node neighbor in gridManager.GetNeighbors(current))
            {
                if (!visited.Contains(neighbor) && !neighbor.IsBlocked())
                {
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                }
            }

            yield return new WaitForSeconds(visualDelay);
        }

        Debug.Log("No path found (BFS).");
    }

    IEnumerator DFS()
    {
        Node startNode = gridManager.currentStartNode;
        Node endNode = gridManager.currentEndNode;

        Stack<Node> stack = new Stack<Node>();
        HashSet<Node> visited = new HashSet<Node>();
        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();

        stack.Push(startNode);
        visited.Add(startNode);

        while (stack.Count > 0)
        {
            Node current = stack.Pop();

            if (current != startNode && current != endNode)
                current.SetColor(Color.magenta);

            if (current == endNode)
            {
                yield return StartCoroutine(ReconstructPath(cameFrom, startNode, endNode));
                yield break;
            }

            foreach (Node neighbor in gridManager.GetNeighbors(current))
            {
                if (!visited.Contains(neighbor) && !neighbor.IsBlocked())
                {
                    stack.Push(neighbor);
                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                }
            }

            yield return new WaitForSeconds(visualDelay);
        }

        Debug.Log("No path found (DFS).");
    }

    IEnumerator Dijkstra()
    {
        Node startNode = gridManager.currentStartNode;
        Node endNode = gridManager.currentEndNode;

        Dictionary<Node, float> distance = new Dictionary<Node, float>();
        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
        List<Node> unvisited = gridManager.GetAllNodes();

        foreach (Node node in unvisited)
        {
            distance[node] = float.MaxValue;
        }

        distance[startNode] = 0;

        while (unvisited.Count > 0)
        {
            unvisited.Sort((a, b) => distance[a].CompareTo(distance[b]));
            Node current = unvisited[0];
            unvisited.RemoveAt(0);

            if (current == endNode)
            {
                yield return StartCoroutine(ReconstructPath(cameFrom, startNode, endNode));
                yield break;
            }

            if (current != startNode && current != endNode)
                current.SetColor(Color.cyan);

            foreach (Node neighbor in gridManager.GetNeighbors(current))
            {
                if (neighbor.IsBlocked() || !unvisited.Contains(neighbor))
                    continue;

                float cost = neighbor.GetTraversalCost();
                if (IsDiagonal(current, neighbor)) cost *= 1.41f;

                float tentativeDist = distance[current] + cost;
                if (tentativeDist < distance[neighbor])
                {
                    distance[neighbor] = tentativeDist;
                    cameFrom[neighbor] = current;
                }
            }

            yield return new WaitForSeconds(visualDelay);
        }

        Debug.Log("No path found (Dijkstra).");
    }

    IEnumerator AStar()
    {
        Node startNode = gridManager.currentStartNode;
        Node endNode = gridManager.currentEndNode;

        List<Node> openSet = new List<Node> { startNode };
        HashSet<Node> closedSet = new HashSet<Node>();
        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();

        Dictionary<Node, float> gScore = new Dictionary<Node, float>();
        Dictionary<Node, float> fScore = new Dictionary<Node, float>();

        foreach (Node node in gridManager.GetAllNodes())
        {
            gScore[node] = float.MaxValue;
            fScore[node] = float.MaxValue;
        }

        gScore[startNode] = 0;
        fScore[startNode] = Heuristic(startNode, endNode);

        while (openSet.Count > 0)
        {
            openSet.Sort((a, b) => fScore[a].CompareTo(fScore[b]));
            Node current = openSet[0];

            if (current == endNode)
            {
                yield return StartCoroutine(ReconstructPath(cameFrom, startNode, endNode));
                yield break;
            }

            openSet.Remove(current);
            closedSet.Add(current);

            if (current != startNode && current != endNode)
                current.SetColor(Color.cyan);

            foreach (Node neighbor in gridManager.GetNeighbors(current))
            {
                if (neighbor.IsBlocked() || closedSet.Contains(neighbor))
                    continue;

                float cost = neighbor.GetTraversalCost();
                if (IsDiagonal(current, neighbor)) cost *= 1.41f;

                float tentativeGScore = gScore[current] + cost;

                if (!openSet.Contains(neighbor))
                    openSet.Add(neighbor);
                else if (tentativeGScore >= gScore[neighbor])
                    continue;

                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, endNode);
            }

            yield return new WaitForSeconds(visualDelay);
        }

        Debug.Log("No path found (A*).");
    }

    float Heuristic(Node a, Node b)
    {
        return Mathf.Abs(a.gridPosition.x - b.gridPosition.x) + Mathf.Abs(a.gridPosition.y - b.gridPosition.y);
    }

    bool IsDiagonal(Node a, Node b)
    {
        return a.gridPosition.x != b.gridPosition.x && a.gridPosition.y != b.gridPosition.y;
    }

    IEnumerator ReconstructPath(Dictionary<Node, Node> cameFrom, Node start, Node end)
    {
        List<Node> path = new List<Node>();
        Node current = end;

        while (current != start)
        {
            path.Add(current);
            current = cameFrom[current];
        }

        path.Reverse();

        foreach (Node node in path)
        {
            if (node != start && node != end)
                node.SetColor(Color.yellow);
            yield return new WaitForSeconds(visualDelay);
        }
    }
}
