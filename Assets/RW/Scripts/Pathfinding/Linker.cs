using UnityEngine;

namespace RW.MonumentValley
{
    // 회전 값에 따라서 노드 사이의 특정 edge를 활성화/비활성화
    [System.Serializable]
    public class RotationLink
    {
        // 회전을 체크할 transform
        public Transform linkedTransform;

        // 링크 활성화를 위한 오일러 각도
        public Vector3 activeEulerAngle;
        [Header("Nodes to activate")]
        public Node nodeA;
        public Node nodeB;
    }

    // 노드 사이 edge를 활성화/비활성화
    public class Linker : MonoBehaviour
    {
        [SerializeField] public RotationLink[] rotationLinks;

        // 이웃하는 노드 사이 edge의 active 상태를 toggle
        public void EnableLink(Node nodeA, Node nodeB, bool state)
        {
            if (nodeA == null || nodeB == null)
                return;

            nodeA.EnableEdge(nodeB, state);
            nodeB.EnableEdge(nodeA, state);
        }

        // 오일러 각도에 따라 enable/disable
        public void UpdateRotationLinks()
        {
            foreach (RotationLink l in rotationLinks)
            {
                if (l.linkedTransform == null || l.nodeA == null || l.nodeB == null)
                    continue;

                // 원하는 각도와 현재 각도의 차이를 구함
                Quaternion targetAngle = Quaternion.Euler(l.activeEulerAngle);
                float angleDiff = Quaternion.Angle(l.linkedTransform.rotation, targetAngle);

                // 각도가 일치하면 링크를 활성화, 아니라면 비활성화
                if (Mathf.Abs(angleDiff) < 0.01f)
                {
                    EnableLink(l.nodeA, l.nodeB, true);
                }
                else
                {
                    EnableLink(l.nodeA, l.nodeB, false);
                }
            }
        }

        // update links when we begin
        private void Start()
        {
            UpdateRotationLinks();
        }
    }
}