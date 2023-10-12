using UnityEngine;

namespace RW.MonumentValley
{
    [RequireComponent(typeof(Animator))]
    // 마우스 클릭을 표시하는 마커
    public class Cursor : MonoBehaviour
    {
        // extra distance offset toward camera
        [SerializeField] private float offsetDistance = 1f;

        private Camera cam;

        // cursor AnimationController
        private Animator animController;

        private void Awake()
        {
            if (cam == null)
            {
                cam = Camera.main;
            }
            animController = GetComponent<Animator>();
        }

        // 항상 카메라를 바라봄
        void LateUpdate()
        {
            if (cam != null)
            {
                Vector3 cameraForward = cam.transform.rotation * Vector3.forward;
                Vector3 cameraUp = cam.transform.rotation * Vector3.up;

                transform.LookAt(transform.position + cameraForward, cameraUp);
            }
        }

        // 추가 offset을 적용한 position에 커서를 보여줌
        public void ShowCursor(Vector3 position)
        {
            if (cam != null && animController != null)
            {
                Vector3 cameraForwardOffset = cam.transform.rotation * new Vector3(0f, 0f, offsetDistance);
                transform.position = position - cameraForwardOffset;

                animController.SetTrigger("ClickTrigger");
            }
        }
    }

}