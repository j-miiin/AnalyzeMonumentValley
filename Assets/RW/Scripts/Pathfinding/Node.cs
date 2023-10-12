using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RW.MonumentValley
{
    public class Node : MonoBehaviour
    {
        // gizmo colors
        [SerializeField] private float gizmoRadius = 0.1f;
        [SerializeField] private Color defaultGizmoColor = Color.black;
        [SerializeField] private Color selectedGizmoColor = Color.blue;
        [SerializeField] private Color inactiveGizmoColor = Color.gray;

        // 이웃 노드와 active 상태
        [SerializeField] private List<Edge> edges = new List<Edge>();

        // edge에서 제외되는 노드
        [SerializeField] private List<Node> excludedNodes;

        // reference to the graph
        private Graph graph;

        // 시작까지의 이동 경로를 구성하는 이전 노드
        // previous Node that forms a "breadcrumb" trail back to the start
        private Node previousNode;

        // Player가 이 노드에 진입했을 때 발생할 이벤트
        public UnityEvent gameEvent;

        // properties
        public Node PreviousNode { get { return previousNode; } set { previousNode = value; } }
        public List<Edge> Edges => edges;

        // 동서남북에 있는 이웃 노드를 자동으로 체크하기 위한 3d 방향
        // 3d compass directions to check for horizontal neighbors automatically(east/west/north/south)
        public static Vector3[] neighborDirections =
        {
            new Vector3(1f, 0f, 0f),
            new Vector3(-1f, 0f, 0f),
            new Vector3(0f, 0f, 1f),
            new Vector3(0f, 0f, -1f),
        };
         
        private void Start()
        {
            // 수평의 노드들과 자동으로 edge 연결
            // automatic connect Edges with horizontal Nodes
            if (graph != null)
            {
                FindNeighbors();
            }
        }

        // draws a sphere gizmo
        private void OnDrawGizmos()
        {
            Gizmos.color = defaultGizmoColor;
            Gizmos.DrawSphere(transform.position, gizmoRadius);
        }

        // draws a sphere gizmo in a different color when selected
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = selectedGizmoColor;
            Gizmos.DrawSphere(transform.position, gizmoRadius);

            // draws a line to each neighbor
            foreach (Edge e in edges)
            {
                if (e.neighbor != null)
                {
                    Gizmos.color = (e.isActive) ? selectedGizmoColor : inactiveGizmoColor;
                    Gizmos.DrawLine(transform.position, e.neighbor.transform.position);
                }
            }
        }

        // 이웃 노드까지의 edge 연결을 자동으로 채움
        public void FindNeighbors()
        {
            // 가능한 이웃 노드들의 offset을 순회
            // search through possible neighbor offsets
            foreach (Vector3 direction in neighborDirections)
            {
                Node newNode = graph?.FindNodeAt(transform.position + direction);

                // 이미 edge 리스트에 존재하거나 특별히 배제된 노드가 아니라면 추가
                if (newNode != null && !HasNeighbor(newNode) && !excludedNodes.Contains(newNode))
                {
                    Edge newEdge = new Edge { neighbor = newNode, isActive = true };
                    edges.Add(newEdge);
                }
            }
        }

        // edge 리스트에 노드가 이미 있는지
        private bool HasNeighbor(Node node)
        {
            foreach (Edge e in edges)
            {
                if (e.neighbor != null && e.neighbor.Equals(node))
                {
                    return true;
                }
            }
            return false;
        }

        // 특정 이웃을 주고 active 상태를 set
        public void EnableEdge(Node neighborNode, bool state)
        {
            foreach (Edge e in edges)
            {
                if (e.neighbor.Equals(neighborNode))
                {
                    e.isActive = state;
                }
            }
        }

        public void InitGraph(Graph graphToInit)
        {
            this.graph = graphToInit;
        }
    }
}