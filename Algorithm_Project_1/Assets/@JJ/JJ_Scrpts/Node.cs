// Node.cs
using UnityEngine;
using System.Collections.Generic; // List<Node> neighbors를 위한 네임스페이스 추가

public class Node
{
    public bool canWalk;
    public Vector3 worldPosition; // 노드의 3D 월드 좌표
    public int gridX; // 그리드 상의 X 좌표
    public int gridY; // 그리드 상의 Y 좌표 (3D에서는 Z를 Y로 사용할 수 있음)
    public int gridZ; // 그리드 상의 Z 좌표 (선택 사항, 층 개념이 있다면 사용)

    public float gCost; // 시작 노드부터 이 노드까지의 실제 비용
    public float hCost; // 이 노드부터 목표 노드까지의 추정 비용 (휴리스틱)
    public Node parent; // 경로 재구성을 위한 부모 노드

    public List<Node> neighbors; // 인접 노드 리스트

    public Node(bool walk, Vector3 pos, int x, int y, int z = 0) // 3D를 고려하여 Z 추가
    {
        canWalk = walk;
        worldPosition = pos;
        gridX = x;
        gridY = y;
        gridZ = z; // 3D 그리드를 사용할 경우
        neighbors = new List<Node>(); // 인접 노드 리스트 초기화
    }

    public float fCost // 총 비용 (A* 알고리즘에 사용)
    {
        get { return gCost + hCost; }
    }
}