using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// --- 열거형(Enum) ---
// 어떤 알고리즘을 사용할지 선택할 수 있게 해주는 열거형
public enum PathfindingAlgorithm
{
    Dijkstra,  // 최단 경로 탐색 (가중치 O)
    AStar,     // 휴리스틱 기반 최단 경로 탐색 (가중치 O, 휴리스틱 O)
    BFS        // 너비 우선 탐색 (가중치 X, 최단거리만 보장)
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
            Debug.LogWarning("BFS: 시작 또는 도착 노드가 이동 불가합니다.");
            return null;
        }

        // --- 자료구조 ---
        Queue<Node> openSet = new Queue<Node>();                     // BFS 탐색용 큐
        HashSet<Node> closedSet = new HashSet<Node>();              // 방문한 노드를 기록 (중복 방지)
        Dictionary<Node, Node> parentMap = new Dictionary<Node, Node>(); // 경로 추적용 부모 맵

        openSet.Enqueue(startNode);
        closedSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.Dequeue(); // FIFO 방식으로 노드를 탐색

            if (currentNode == targetNode)
                return ReconstructPathBFS(parentMap, startNode, targetNode);

            foreach (Node neighbor in currentNode.neighbors)
            {
                if (!closedSet.Contains(neighbor) && neighbor.canWalk)
                {
                    openSet.Enqueue(neighbor);
                    closedSet.Add(neighbor);
                    parentMap[neighbor] = currentNode;  // 부모 설정
                }
            }
        }

        return null;
    }

    // ==========================
    // 다익스트라 알고리즘
    // ==========================
    public List<Node> FindPathDijkstra(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = PathGrid.Instance.GetNodeFromWorldPoint(startPos);
        Node targetNode = PathGrid.Instance.GetNodeFromWorldPoint(targetPos);

        if (!startNode.canWalk || !targetNode.canWalk)
        {
            Debug.LogWarning("Dijkstra: 시작 또는 도착 노드가 이동 불가합니다.");
            return null;
        }

        // --- 자료구조 ---
        PriorityQueue<Node> openSet = new PriorityQueue<Node>(); // 우선순위 큐 (가중치 기준 최소값 pop)
        HashSet<Node> closedSet = new HashSet<Node>();           // 방문한 노드 집합

        // --- 알고리즘 초기화 ---
        foreach (Node n in PathGrid.Instance.GetAllNodes())
        {
            if (n != null)
            {
                n.gCost = float.MaxValue; // 초기 비용 무한대
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

                float newGCost = currentNode.gCost + GetDistance(currentNode, neighbor); // 거리 누적

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
    // A* 알고리즘 (A-Star)
    // ==========================
    public List<Node> FindPathAStar(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = PathGrid.Instance.GetNodeFromWorldPoint(startPos);
        Node targetNode = PathGrid.Instance.GetNodeFromWorldPoint(targetPos);

        if (!startNode.canWalk || !targetNode.canWalk)
        {
            Debug.LogWarning("A*: 시작 또는 도착 노드가 이동 불가합니다.");
            return null;
        }

        // --- 자료구조 ---
        PriorityQueue<Node> openSet = new PriorityQueue<Node>(); // fCost 기준 정렬
        HashSet<Node> closedSet = new HashSet<Node>();

        // --- 알고리즘 초기화 ---
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
        startNode.hCost = GetDistance(startNode, targetNode); // 휴리스틱 계산
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
                    neighbor.hCost = GetDistance(neighbor, targetNode); // hCost 업데이트
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
    // BFS 경로 복원용 함수
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
                Debug.LogError("BFS 경로 복원 중 오류: 부모 정보 없음");
                return null;
            }
        }

        path.Add(startNode);
        path.Reverse(); // 역방향이므로 뒤집어야 올바른 순서
        return path;
    }

    // ==========================
    // A*, Dijkstra 경로 복원용
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
    // 거리 계산 (유클리디안)
    // ==========================
    private float GetDistance(Node nodeA, Node nodeB)
    {
        return Vector3.Distance(nodeA.worldPosition, nodeB.worldPosition); // 실제 거리
    }
}
