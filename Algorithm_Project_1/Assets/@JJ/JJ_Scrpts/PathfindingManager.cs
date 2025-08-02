using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// --- ������(Enum) ---
// � �˰����� ������� ������ �� �ְ� ���ִ� ������
public enum PathfindingAlgorithm
{
    Dijkstra,  // �ִ� ��� Ž�� (����ġ O)
    AStar,     // �޸���ƽ ��� �ִ� ��� Ž�� (����ġ O, �޸���ƽ O)
    BFS        // �ʺ� �켱 Ž�� (����ġ X, �ִܰŸ��� ����)
}

public class PathfindingManager : MonoBehaviour
{
    public static PathfindingManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ==========================
    // BFS (Breadth-First Search)
    // ==========================
    public List<Node> FindPathBFS(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = PathGrid.Instance.GetNodeFromWorldPoint(startPos);
        Node targetNode = PathGrid.Instance.GetNodeFromWorldPoint(targetPos);

        if (!startNode.canWalk || !targetNode.canWalk)
        {
            Debug.LogWarning("BFS: ���� �Ǵ� ���� ��尡 �̵� �Ұ��մϴ�.");
            return null;
        }

        // --- �ڷᱸ�� ---
        Queue<Node> openSet = new Queue<Node>();                     // BFS Ž���� ť
        HashSet<Node> closedSet = new HashSet<Node>();              // �湮�� ��带 ��� (�ߺ� ����)
        Dictionary<Node, Node> parentMap = new Dictionary<Node, Node>(); // ��� ������ �θ� ��

        openSet.Enqueue(startNode);
        closedSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.Dequeue(); // FIFO ������� ��带 Ž��

            if (currentNode == targetNode)
                return ReconstructPathBFS(parentMap, startNode, targetNode);

            foreach (Node neighbor in currentNode.neighbors)
            {
                if (!closedSet.Contains(neighbor) && neighbor.canWalk)
                {
                    openSet.Enqueue(neighbor);
                    closedSet.Add(neighbor);
                    parentMap[neighbor] = currentNode;  // �θ� ����
                }
            }
        }

        return null;
    }

    // ==========================
    // ���ͽ�Ʈ�� �˰���
    // ==========================
    public List<Node> FindPathDijkstra(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = PathGrid.Instance.GetNodeFromWorldPoint(startPos);
        Node targetNode = PathGrid.Instance.GetNodeFromWorldPoint(targetPos);

        if (!startNode.canWalk || !targetNode.canWalk)
        {
            Debug.LogWarning("Dijkstra: ���� �Ǵ� ���� ��尡 �̵� �Ұ��մϴ�.");
            return null;
        }

        // --- �ڷᱸ�� ---
        PriorityQueue<Node> openSet = new PriorityQueue<Node>(); // �켱���� ť (����ġ ���� �ּҰ� pop)
        HashSet<Node> closedSet = new HashSet<Node>();           // �湮�� ��� ����

        // --- �˰��� �ʱ�ȭ ---
        foreach (Node n in PathGrid.Instance.GetAllNodes())
        {
            if (n != null)
            {
                n.gCost = float.MaxValue; // �ʱ� ��� ���Ѵ�
                n.parent = null;
            }
        }

        startNode.gCost = 0;
        openSet.Enqueue(startNode, startNode.gCost);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.Dequeue();
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
                return ReconstructPath(startNode, targetNode);

            foreach (Node neighbor in currentNode.neighbors)
            {
                if (closedSet.Contains(neighbor) || !neighbor.canWalk) continue;

                float newGCost = currentNode.gCost + GetDistance(currentNode, neighbor); // �Ÿ� ����

                if (newGCost < neighbor.gCost)
                {
                    neighbor.gCost = newGCost;
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Enqueue(neighbor, neighbor.gCost);
                    else
                        openSet.UpdatePriority(neighbor, neighbor.gCost);
                }
            }
        }

        return null;
    }

    // ==========================
    // A* �˰��� (A-Star)
    // ==========================
    public List<Node> FindPathAStar(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = PathGrid.Instance.GetNodeFromWorldPoint(startPos);
        Node targetNode = PathGrid.Instance.GetNodeFromWorldPoint(targetPos);

        if (!startNode.canWalk || !targetNode.canWalk)
        {
            Debug.LogWarning("A*: ���� �Ǵ� ���� ��尡 �̵� �Ұ��մϴ�.");
            return null;
        }

        // --- �ڷᱸ�� ---
        PriorityQueue<Node> openSet = new PriorityQueue<Node>(); // fCost ���� ����
        HashSet<Node> closedSet = new HashSet<Node>();

        // --- �˰��� �ʱ�ȭ ---
        foreach (Node n in PathGrid.Instance.GetAllNodes())
        {
            if (n != null)
            {
                n.gCost = float.MaxValue;
                n.hCost = 0;
                n.parent = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = GetDistance(startNode, targetNode); // �޸���ƽ ���
        openSet.Enqueue(startNode, startNode.fCost); // fCost = g + h

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.Dequeue();
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
                return ReconstructPath(startNode, targetNode);

            foreach (Node neighbor in currentNode.neighbors)
            {
                if (closedSet.Contains(neighbor) || !neighbor.canWalk) continue;

                float newGCost = currentNode.gCost + GetDistance(currentNode, neighbor);

                if (newGCost < neighbor.gCost)
                {
                    neighbor.gCost = newGCost;
                    neighbor.hCost = GetDistance(neighbor, targetNode); // hCost ������Ʈ
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Enqueue(neighbor, neighbor.fCost);
                    else
                        openSet.UpdatePriority(neighbor, neighbor.fCost);
                }
            }
        }

        return null;
    }

    // ==========================
    // BFS ��� ������ �Լ�
    // ==========================
    private List<Node> ReconstructPathBFS(Dictionary<Node, Node> parentMap, Node startNode, Node targetNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = targetNode;

        while (currentNode != null && currentNode != startNode)
        {
            path.Add(currentNode);

            if (!parentMap.TryGetValue(currentNode, out currentNode))
            {
                Debug.LogError("BFS ��� ���� �� ����: �θ� ���� ����");
                return null;
            }
        }

        path.Add(startNode);
        path.Reverse(); // �������̹Ƿ� ������� �ùٸ� ����
        return path;
    }

    // ==========================
    // A*, Dijkstra ��� ������
    // ==========================
    private List<Node> ReconstructPath(Node startNode, Node targetNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = targetNode;

        while (currentNode != null && currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Add(startNode);
        path.Reverse();
        return path;
    }

    // ==========================
    // �Ÿ� ��� (��Ŭ�����)
    // ==========================
    private float GetDistance(Node nodeA, Node nodeB)
    {
        return Vector3.Distance(nodeA.worldPosition, nodeB.worldPosition); // ���� �Ÿ�
    }
}
