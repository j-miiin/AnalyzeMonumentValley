﻿using System.Collections.Generic;
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

        // neighboring nodes + active state
        [SerializeField] private List<Edge> edges = new List<Edge>();

        // Nodes specifically excluded from Edges
        [SerializeField] private List<Node> excludedNodes;

        // reference to the graph
        private Graph graph;

        // previous Node that forms a "breadcrumb" trail back to the start
        private Node previousNode;

        // invoked when Player enters this node
        public UnityEvent gameEvent;

        // properties
        
        public Node PreviousNode { get { return previousNode; } set { previousNode = value; } }
        public List<Edge> Edges => edges;

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

        // fill out edge connections to neighboring nodes automatically
        public void FindNeighbors()
        {
            // search through possible neighbor offsets
            foreach (Vector3 direction in neighborDirections)
            {
                Node newNode = graph?.FindNodeAt(transform.position + direction);

                // add to edges list if not already included and not excluded specifically
                if (newNode != null && !HasNeighbor(newNode) && !excludedNodes.Contains(newNode))
                {
                    Edge newEdge = new Edge { neighbor = newNode, isActive = true };
                    edges.Add(newEdge);
                }
            }
        }

        // is a Node already in the Edges List?
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

        // given a specific neighbor, sets active state
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