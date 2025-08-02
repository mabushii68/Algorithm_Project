# Algorithm Pathfinding Unity Prototype

## 프로젝트 개요
- **개발 기간**: 2025년 1학기 알고리즘 기말 프로젝트
- **개발 도구**: Unity (URP), C#
- **목적**: 다양한 길 찾기 알고리즘의 시각적 비교 및 학습

## 프로토타입 설명
- 이 프로토타입은 Unity 3D 환경에서 그리드(Grid) 기반의 3차원 길 찾기 알고리즘을 구현하고 시각화하는 시스템입니다. 플레이어가 맵상의 특정 지점을 클릭하면, 해당 지점까지 최단 또는 최적의 경로를 계산하여 이동하며, 그 과정을 시각적으로 보여줍니다. 3가지 주요 길 찾기 알고리즘(BFS, Dijkstra, A*)을 선택하여 비교하고 테스트할 수 있습니다.
- 가천대학교 재학생 및 신입생 그리고 외부 방문객들을 위해 만들었습니다.
- 모바일 환경 (+AR/VR) 혹은 PC 환경에서 필요할 때 언제든지 사용할 수 있습니다.
- 가천대학교 캠퍼스는 넓고 복잡하기 때문에, 건물 위치나 동선을 쉽게 이해할 수 있는 가상 안내 서비스가 필요하다고 생각했습니다. 또한 이 서비스는 이동에 어려움을 겪는 분들 (휠체어 사용자, 노약자, 임산부)의 불편함을 해소할 수 있습니다. 일반적인 이동 경로는 계단이나 경사로가 포함될 수 있어 안전하지 않으므로 엘리베이터와 경사로 중심의 접근 가능한 경로가 필요하다고 생각했습니다.

## 사용한 자료구조 
- 3차원 배열 (Node[,,] grid)
  - PathGrid.cs 스크립트에서 전체 3D 공간을 이산적인 노드들로 표현하기 위해 사용했습니다. grid[x, y, z] 형태로 특정 좌표의 노드에 직접 접근할 수 있습니다.
 
- 우선순위 큐 (PriorityQueue<T>)
  - PathfindingManager.cs의 다익스트라(Dijkstra) 및 A* 알고리즘에서 openSet으로 사용됩니다. 다음에 탐색할 노드 중 비용이 가장 낮은 노드를 효율적으로 선택하기 위해 사용됩니다.
 
- 해시셋 (HashSet<Node> closedSet)
  - athfindingManager.cs의 모든 길 찾기 알고리즘(BFS, Dijkstra, A*)에서 이미 방문하여 탐색을 완료한 노드들을 저장하는 데 사용됩니다.
 
## 사용한 알고리즘
- 너비 우선 탐색 (BFS: Breadth-First Search)
  - athfindingManager.cs의 FindPathBFS 함수에서 구현됩니다. 최단 거리(간선 수 기준) 경로를 찾기 위해 큐(Queue)를 사용하여 시작 노드에서부터 인접 노드를 층별로 탐색해 나갑니다.
 
- 다익스트라 알고리즘 (Dijkstra's Algorithm)
  - PathfindingManager.cs의 FindPathDijkstra 함수에서 구현됩니다. 우선순위 큐(PriorityQueue)를 사용하여 시작 노드로부터 모든 다른 노드까지의 최단 경로를 찾습니다. 각 노드 간의 거리를 비용으로 사용하여 최단 "거리" 경로를 보장합니다.
 
- A* 알고리즘 (A* Search Algorithm)
  - PathfindingManager.cs의 FindPathAStar 함수에서 구현됩니다. 우선순위 큐(PriorityQueue)를 사용하며, 각 노드에 대해 시작점으로부터의 실제 비용(gCost)과 목표점까지의 예상 비용(hCost, 휴리스틱)을 합산한 총 비용(fCost = gCost + hCost)을 기준으로 탐색합니다.

## 설계 / 구현 방법
- 사용한 툴 : Blender, Substance 3D Painter, Unity6
