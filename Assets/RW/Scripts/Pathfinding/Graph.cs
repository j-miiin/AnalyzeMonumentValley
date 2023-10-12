using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace RW.MonumentValley
{
    // 모든 Node를 관리
    public class Graph : MonoBehaviour
    {
        // 현재 level에 있는 모든 Node 리스트
        private List<Node> allNodes = new List<Node>();

        // 마지막 목표 노드
        [SerializeField] private Node goalNode;
        public Node GoalNode => goalNode;

        private void Awake()
        {
            allNodes = FindObjectsOfType<Node>().ToList();
            InitNodes();
        }

        private void Start()
        {
            InitNeighbors();
        }

        // 반올림 오차 범위 내에서 타겟 pos에 있는 노드를 찾음
        public Node FindNodeAt(Vector3 pos)
        {
            foreach (Node n in allNodes)
            {
                Vector3 diff = n.transform.position - pos;

                if (diff.sqrMagnitude < 0.01f)
                {
                    return n;
                }
            }
            return null;
        }

        // Node 배열이 주어질 때 가장 가까운 Node를 찾음
        public Node FindClosestNode(Node[] nodes, Vector3 pos)
        {
            Node closestNode = null;
            float closestDistanceSqr = Mathf.Infinity;

            foreach (Node n in nodes)
            {
                Vector3 diff = n.transform.position - pos;

                Vector3 nodeScreenPosition = Camera.main.WorldToScreenPoint(n.transform.position);
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(pos);
                diff = nodeScreenPosition - screenPosition;

                if (diff.sqrMagnitude < closestDistanceSqr)
                {
                    closestNode = n;
                    closestDistanceSqr = diff.sqrMagnitude;
                }
            }
            return closestNode;
        }

        // 전체 Graph에서 가장 가까운 Node를 찾음
        public Node FindClosestNode(Vector3 pos)
        {
            return FindClosestNode(allNodes.ToArray(), pos);
        }

        // 추적 경로 clear
        public void ResetNodes()
        {
            foreach (Node node in allNodes)
            {
                node.PreviousNode = null;
            }
        }

        // 각 Node에 대해 Graph 설정
        private void InitNodes()
        {
            foreach (Node n in allNodes)
            {
                if (n != null)
                {
                    n.InitGraph(this);
                }
            }
        }

        // 모든 Node가 생성된 후에 각 Node의 이웃을 set
        private void InitNeighbors()
        {
            foreach (Node n in allNodes)
            {
                if (n != null)
                {
                    n.FindNeighbors();
                }
            }
        }
    }
}