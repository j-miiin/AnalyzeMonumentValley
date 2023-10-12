using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace RW.MonumentValley
{
    // 타겟 transform을 마우스 클릭과 드래그로 회전
    [RequireComponent(typeof(Collider))]
    public class DragSpinner : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public enum SpinAxis
        {
            X,
            Y,
            Z
        }

        // 회전할 target transform
        [SerializeField] private Transform targetToSpin;

        // 회전축
        [SerializeField] private SpinAxis spinAxis = SpinAxis.X;

        // 마우스 포인터 각도를 계산할 pivot
        [SerializeField] private Transform pivot;

        // 최소 드래그 거리 값
        [SerializeField] private int minDragDist = 10;

        //[SerializeField] private Linker linker;

        // pivot에서 마우스 포인터까지의 벡터
        private Vector2 directionToMouse;

        // 회전 중인지 여부 체크
        private bool isSpinning;

        private bool isActive;

        // 화면을 클릭한 지점으로부터의 각도
        private float angleToMouse;

        // 이전 프레임에서 마우스 각도
        private float previousAngleToMouse;

        // 회전축 벡터
        private Vector3 axisDirection;

        public UnityEvent snapEvent;

        private float timeCount;

        void Start()
        {
            switch (spinAxis)
            {
                case (SpinAxis.X):
                    axisDirection = Vector3.right;
                    break;
                case (SpinAxis.Y):
                    axisDirection = Vector3.up;
                    break;
                case (SpinAxis.Z):
                    axisDirection = Vector3.forward;
                    break;
            }
            EnableSpinner(true);
        }

        // 회전 드래그 시작
        public void OnBeginDrag(PointerEventData data)
        {
            if (!isActive)
            {
                return;
            }

            isSpinning = true;

            // 클릭한 마우스 포지션까지의 벡터 
            Vector3 inputPosition = new Vector3(data.position.x, data.position.y, 0f);
            directionToMouse = inputPosition - Camera.main.WorldToScreenPoint(pivot.position);

            // 마우스 포인터 각도 저장
            previousAngleToMouse = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg;
        }

        // 드래그 동작이 끝났을 때
        public void OnEndDrag(PointerEventData data)
        {
            if (isActive)
            {
                SnapSpinner();
            }
        }

        public void OnDrag(PointerEventData data)
        {
            if (isSpinning && Camera.main != null && pivot != null && isActive)
            {
                // 현재 마우스 포지션 각도
                Vector3 inputPosition = new Vector3(data.position.x, data.position.y, 0f);
                directionToMouse = inputPosition - Camera.main.WorldToScreenPoint(pivot.position);
                angleToMouse = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg;

                // 최소 드래그 값보다 더 드래그했을 경우
                if (directionToMouse.magnitude > minDragDist)
                {
                    Vector3 newRotationVector = (previousAngleToMouse - angleToMouse) * axisDirection;
                    targetToSpin.Rotate(newRotationVector); // 타겟을 회전시킴
                    previousAngleToMouse = angleToMouse;
                }
            }
        }

        // 90도 간격으로 snap 가능
        private void SnapSpinner()
        {
            isSpinning = false;

            // 가장 가까운 90도 간격으로 회전
            RoundToRightAngles(targetToSpin);

            // snap 이벤트 invoke
            if (snapEvent != null)
            {
                snapEvent.Invoke();
            }
        }

        // 가장 가까운 90도 반환
        private void RoundToRightAngles(Transform xform)
        {
            float roundedXAngle = Mathf.Round(xform.eulerAngles.x / 90f) * 90f;
            float roundedYAngle = Mathf.Round(xform.eulerAngles.y / 90f) * 90f;
            float roundedZAngle = Mathf.Round(xform.eulerAngles.z / 90f) * 90f;

            xform.eulerAngles = new Vector3(roundedXAngle, roundedYAngle, roundedZAngle);
        }

        // 스피너 enable/disable
        public void EnableSpinner(bool state)
        {
            isActive = state;

            // force snap the spinner on disable
            if (!isActive)
            {
                SnapSpinner();
            }
        }
    }
}