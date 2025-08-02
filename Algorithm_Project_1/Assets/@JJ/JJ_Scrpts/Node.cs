// Node.cs
using UnityEngine;
using System.Collections.Generic; // List<Node> neighbors�� ���� ���ӽ����̽� �߰�

public class Node
{
    public bool canWalk;
    public Vector3 worldPosition; // ����� 3D ���� ��ǥ
    public int gridX; // �׸��� ���� X ��ǥ
    public int gridY; // �׸��� ���� Y ��ǥ (3D������ Z�� Y�� ����� �� ����)
    public int gridZ; // �׸��� ���� Z ��ǥ (���� ����, �� ������ �ִٸ� ���)

    public float gCost; // ���� ������ �� �������� ���� ���
    public float hCost; // �� ������ ��ǥ �������� ���� ��� (�޸���ƽ)
    public Node parent; // ��� �籸���� ���� �θ� ���

    public List<Node> neighbors; // ���� ��� ����Ʈ

    public Node(bool walk, Vector3 pos, int x, int y, int z = 0) // 3D�� ����Ͽ� Z �߰�
    {
        canWalk = walk;
        worldPosition = pos;
        gridX = x;
        gridY = y;
        gridZ = z; // 3D �׸��带 ����� ���
        neighbors = new List<Node>(); // ���� ��� ����Ʈ �ʱ�ȭ
    }

    public float fCost // �� ��� (A* �˰��� ���)
    {
        get { return gCost + hCost; }
    }
}