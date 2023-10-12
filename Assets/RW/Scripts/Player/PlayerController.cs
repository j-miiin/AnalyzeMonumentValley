using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RW.MonumentValley
{
    // Player의 Input과 움직임을 처리
    [RequireComponent(typeof(PlayerAnimation))]
    public class PlayerController : MonoBehaviour
    {
        // 한 unit을 이동하는 시간
        [Range(0.25f, 2f)]
        [SerializeField] private float moveTime = 0.5f;

        // 클릭 indicator
        [SerializeField] Cursor cursor;

        // 커서 애니메이션 컨트롤러
        private Animator cursorAnimController;

        // pathfinding fields
        private Clickable[] clickables;
        private Pathfinder pathfinder;
        private Graph graph;
        private Node currentNode;
        private Node nextNode;

        // flags
        private bool isMoving;
        private bool isControlEnabled;
        private PlayerAnimation playerAnimation;

        private void Awake()
        {
            //  initialize fields
            clickables = FindObjectsOfType<Clickable>();
            pathfinder = FindObjectOfType<Pathfinder>();
            playerAnimation = GetComponent<PlayerAnimation>();

            if (pathfinder != null)
            {
                graph = pathfinder.GetComponent<Graph>();
            }

            isMoving = false;
            isControlEnabled = true;
        }

        private void Start()
        {
            // 항상 노드에서 시작
            SnapToNearestNode();

            // Graph의 시작 노드 set
            if (pathfinder != null && !pathfinder.SearchOnStart)
            {
                pathfinder.SetStartNode(transform.position);
            }

            // 모든 클릭 이벤트 listen
            foreach (Clickable c in clickables)
            {
                c.clickAction += OnClick;
            }
        }

        private void OnDisable()
        {
            // unsubscribe from clickEvents when disabled
            foreach (Clickable c in clickables)
            {
                c.clickAction -= OnClick;
            }
        }

        private void OnClick(Clickable clickable, Vector3 position)
        {
            if (!isControlEnabled || clickable == null || pathfinder == null)
            {
                return;
            }

            // Clickable 노드 중 가장 최적의 경로를 찾음
            List<Node> newPath = pathfinder.FindBestPath(currentNode, clickable.ChildNodes);

            // 이미 움직이는 도중 클릭하면 이전의 애니메이션과 모션을 모두 멈춤
            if (isMoving)
            {
                StopAllCoroutines();
            }

            // 마우스 클릭에 마커를 보여줌
            if (cursor != null)
            {
                cursor.ShowCursor(position);
            }

            // 유효한 경로가 있다면 따라감
            if (newPath.Count > 1)
            {
                StartCoroutine(FollowPathRoutine(newPath));
            }
            else
            {
                // 유효한 경로가 없다면 움직임을 멈춤
                isMoving = false;
                UpdateAnimation();
            }
        }

        private IEnumerator FollowPathRoutine(List<Node> path)
        {
            // start moving
            isMoving = true;

            if (path == null || path.Count <= 1)
            {
                Debug.Log("PLAYERCONTROLLER FollowPathRoutine: invalid path");
            }
            else
            {
                UpdateAnimation();

                // 모든 노드를 순회
                for (int i = 0; i < path.Count; i++)
                {
                    // 다음에 갈 포인트로 현재 노드를 사용
                    nextNode = path[i];

                    // flip 최소화를 위해 다음 노드를 바라봄
                    int nextAimIndex = Mathf.Clamp(i + 1, 0, path.Count - 1);
                    Node aimNode = path[nextAimIndex];
                    FaceNextPosition(transform.position, aimNode.transform.position);

                    // 다음 노드로 이동
                    yield return StartCoroutine(MoveToNodeRoutine(transform.position, nextNode));
                }
            }

            isMoving = false;
            UpdateAnimation();
        }

        // 현재 노드로부터 다른 노드까지의 lerp
        private IEnumerator MoveToNodeRoutine(Vector3 startPosition, Node targetNode)
        {
            float elapsedTime = 0;

            // 유효한 이동 시간
            moveTime = Mathf.Clamp(moveTime, 0.1f, 5f);

            while (elapsedTime < moveTime && targetNode != null && !HasReachedNode(targetNode))
            {
                elapsedTime += Time.deltaTime;
                float lerpValue = Mathf.Clamp(elapsedTime / moveTime, 0f, 1f);

                Vector3 targetPos = targetNode.transform.position;
                transform.position = Vector3.Lerp(startPosition, targetPos, lerpValue);

                // 반 이상 오면 다음 노드로 parent를 변경
                if (lerpValue > 0.51f)
                {
                    transform.parent = targetNode.transform;
                    currentNode = targetNode;

                    // 다음 노드에 연결된 Event 실행
                    targetNode.gameEvent.Invoke();
                    //Debug.Log("invoked GameEvent from targetNode: " + targetNode.name);
                }

                // wait one frame
                yield return null;
            }
        }

        // 플레이어를 가장 가까운 노드에 위치시킴
        public void SnapToNearestNode()
        {
            Node nearestNode = graph?.FindClosestNode(transform.position);
            if (nearestNode != null)
            {
                currentNode = nearestNode;
                transform.position = nearestNode.transform.position;
            }
        }

        // turn face the next Node, always projected on a plane at the Player's feet
        public void FaceNextPosition(Vector3 startPosition, Vector3 nextPosition)
        {
            if (Camera.main == null)
            {
                return;
            }

            // convert next Node world space to screen space
            Vector3 nextPositionScreen = Camera.main.WorldToScreenPoint(nextPosition);

            // convert next Node screen point to Ray
            Ray rayToNextPosition = Camera.main.ScreenPointToRay(nextPositionScreen);

            // plane at player's feet
            Plane plane = new Plane(Vector3.up, startPosition);

            // distance from camera (used for projecting point onto plane)
            float cameraDistance = 0f;

            // project the nextNode onto the plane and face toward projected point
            if (plane.Raycast(rayToNextPosition, out cameraDistance))
            {
                Vector3 nextPositionOnPlane = rayToNextPosition.GetPoint(cameraDistance);
                Vector3 directionToNextNode = nextPositionOnPlane - startPosition;
                if (directionToNextNode != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(directionToNextNode);
                }
            }
        }

        // toggle between Idle and Walk animations
        private void UpdateAnimation()
        {
            if (playerAnimation != null)
            {
                playerAnimation.ToggleAnimation(isMoving);
            }
        }

        // 특정 노드에 도착했는지
        public bool HasReachedNode(Node node)
        {
            if (pathfinder == null || graph == null || node == null)
            {
                return false;
            }

            float distanceSqr = (node.transform.position - transform.position).sqrMagnitude;

            return (distanceSqr < 0.01f);
        }

        // have we reached the end of the graph?
        public bool HasReachedGoal()
        {
            if (graph == null)
            {
                return false;
            }
            return HasReachedNode(graph.GoalNode);
        }

        //  enable/disable controls
        public void EnableControls(bool state)
        {
            isControlEnabled = state;
        }
    }
}