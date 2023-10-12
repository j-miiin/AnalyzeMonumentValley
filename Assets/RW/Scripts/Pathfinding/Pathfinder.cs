using System.Collections.Generic;
using UnityEngine;

namespace RW.MonumentValley
{
    // Graph를 통해 경로 생성
    [RequireComponent(typeof(Graph))]
    public class Pathfinder : MonoBehaviour
    {
        // 경로 시작 노드 (현재 Player가 위치하는 노드)
        [SerializeField] private Node startNode;

        // 경로 종료 노드(목표 노드)
        [SerializeField] private Node destinationNode;
        [SerializeField] private bool searchOnStart;

        // 탐색할 다음 노드 리스트
        private List<Node> frontierNodes;

        // 이미 탐색한 노드 리스트
        private List<Node> exploredNodes;

        // goal까지의 경로를 이루는 노드 리스트
        private List<Node> pathNodes;

        // 탐색이 완료 되었는가
        private bool isSearchComplete;

        // 목표 노드를 찾았는가
        private bool isPathComplete;

        // 모든 노드를 가지고 있음
        private Graph graph;

        // properties
        public Node StartNode { get { return startNode; } set { startNode = value; } }
        public Node DestinationNode { get { return destinationNode; } set { destinationNode = value; } }
        public List<Node> PathNodes => pathNodes;
        public bool IsPathComplete => isPathComplete;
        public bool SearchOnStart => searchOnStart;

        private void Awake()
        {
            graph = GetComponent<Graph>();
        }

        private void Start()
        {
            if (searchOnStart)
            {
                pathNodes = FindPath();
            }
        }

        // 모든 노드와 리스트를 init
        private void InitGraph()
        {
            // 필요한 컴포넌트들이 유효한지 확인
            if (graph == null || startNode == null || destinationNode == null)
            {
                return;
            }

            frontierNodes = new List<Node>();
            exploredNodes = new List<Node>();
            pathNodes = new List<Node>();

            isSearchComplete = false;
            isPathComplete = false;

            // 이전 탐색의 결과 삭제
            graph.ResetNodes();

            // 시작 노드를 탐색할 노드 리스트에 추가
            frontierNodes.Add(startNode);
        }

        // BFS 로직
        private void ExpandFrontier(Node node)
        {
            // 유효한 노드인가
            if (node == null)
            {
                return;
            }

            // 모든 Edge를 순회
            for (int i = 0; i < node.Edges.Count; i++)
            {
                // 이웃한 노드를 이미 탐색했거나 유효하지 않으면 건너뛰기
                if (node.Edges[i] == null ||
                    node.Edges.Count == 0 ||
                    exploredNodes.Contains(node.Edges[i].neighbor) ||
                    frontierNodes.Contains(node.Edges[i].neighbor))
                {
                    continue;
                }

                // Edge가 활성화 되어 있다면 trail 연결
                if (node.Edges[i].isActive && node.Edges[i].neighbor != null)
                {
                    node.Edges[i].neighbor.PreviousNode = node;

                    // 탐색할 노드 리스트에 이웃 노드 추가
                    frontierNodes.Add(node.Edges[i].neighbor);
                }
            }
        }

        // 시작 노드에서 목표 노드까지의 경로를 set
        public List<Node> FindPath()
        {
            List<Node> newPath = new List<Node>();

            if (startNode == null || destinationNode == null || startNode == destinationNode)
            {
                return newPath;
            }

            // 무한 루프 방지
            const int maxIterations = 100;
            int iterations = 0;

            // 모든 노드 init
            InitGraph();

            // goal 노드를 찾거나, 모든 노드를 탐색했거나, max 값을 넘길 때까지 그래프 탐색
            while (!isSearchComplete && frontierNodes != null && iterations < maxIterations)
            {
                iterations++;

                // 더 탐색할 노드가 있다면
                if (frontierNodes.Count > 0)
                {
                    // 첫 번째 노드 삭제
                    Node currentNode = frontierNodes[0];
                    frontierNodes.RemoveAt(0);

                    // 탐색할 노드에 추가
                    if (!exploredNodes.Contains(currentNode))
                    {
                        exploredNodes.Add(currentNode);
                    }

                    // 탐색하지 않은 이웃 노드를 탐색할 노드 리스트에 추가
                    ExpandFrontier(currentNode);

                    // 목표 노드를 찾았다면
                    if (frontierNodes.Contains(destinationNode))
                    {
                        // goal까지의 경로 생성
                        newPath = GetPathNodes();
                        isSearchComplete = true;
                        isPathComplete = true;
                    }
                }
                // 모든 Graph를 탐색했지만 경로를 찾지 못했다면
                else
                {
                    isSearchComplete = true;
                    isPathComplete = false;
                }
            }
            return newPath;
        }

        public List<Node> FindPath(Node start, Node destination)
        {
            this.destinationNode = destination;
            this.startNode = start;
            return FindPath();
        }

        // 최적의 경로를 찾아 가능한 노드 리스트를 반환
        public List<Node> FindBestPath(Node start, Node[] possibleDestinations)
        {
            List<Node> bestPath = new List<Node>();
            foreach (Node n in possibleDestinations)
            {
                List<Node> possiblePath = FindPath(start, n);

                if (!isPathComplete && isSearchComplete)
                {
                    continue;
                }

                if (bestPath.Count == 0 && possiblePath.Count > 0)
                {
                    bestPath = possiblePath;
                }

                if (bestPath.Count > 0 && possiblePath.Count < bestPath.Count)
                {
                    bestPath = possiblePath;
                }
            }

            if (bestPath.Count <= 1)
            {
                ClearPath();
                return new List<Node>();
            }

            destinationNode = bestPath[bestPath.Count - 1];
            pathNodes = bestPath;
            return bestPath;
        }

        public void ClearPath()
        {
            startNode = null;
            destinationNode = null;
            pathNodes = new List<Node>();
        }

        // given a goal node, follow PreviousNode breadcrumbs to create a path
        public List<Node> GetPathNodes()
        {
            // create a new list of Nodes
            List<Node> path = new List<Node>();

            // start with the goal Node
            if (destinationNode == null)
            {
                return path;
            }
            path.Add(destinationNode);

            // follow the breadcrumb trail, creating a path until it ends
            Node currentNode = destinationNode.PreviousNode;

            while (currentNode != null)
            {
                path.Insert(0, currentNode);
                currentNode = currentNode.PreviousNode;
            }
            return path;
        }

        private void OnDrawGizmos()
        {
            if (isSearchComplete)
            {
                foreach (Node node in pathNodes)
                {

                    if (node == startNode)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawCube(node.transform.position, new Vector3(0.25f, 0.25f, 0.25f));
                    }
                    else if (node == destinationNode)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawCube(node.transform.position, new Vector3(0.25f, 0.25f, 0.25f));
                    }
                    else
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawSphere(node.transform.position, 0.15f);
                    }

                    Gizmos.color = Color.yellow;
                    if (node.PreviousNode != null)
                    {
                        Gizmos.DrawLine(node.transform.position, node.PreviousNode.transform.position);
                    }
                }
            }
        }

        public void SetStartNode(Vector3 position)
        {
            StartNode = graph.FindClosestNode(position);
        }
    }
}