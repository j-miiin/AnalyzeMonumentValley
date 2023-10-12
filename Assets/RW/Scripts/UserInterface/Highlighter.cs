using UnityEngine;
using UnityEngine.EventSystems;

namespace RW.MonumentValley
{
    [RequireComponent(typeof(Collider))]
    public class Highlighter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        // 스피너 오브젝트의 mesh renderer
        [SerializeField] private MeshRenderer[] meshRenderers;

        // Shader Graph 프로퍼티
        [SerializeField] private string highlightProperty = "_IsHighlighted";

        private bool isEnabled;
        public bool IsEnabled { get { return isEnabled; } set { isEnabled = value; } }


        private void Start()
        {
            isEnabled = true;
            // 초기 기본 상태는 스피너의 불이 꺼져있음
            ToggleHighlight(false);
        }

        // Shader Graph 프로퍼티로 스피너의 불 on/off
        public void ToggleHighlight(bool onOff)
        {
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                if (meshRenderer != null)
                {
                    meshRenderer.material.SetFloat(highlightProperty, onOff ? 1f : 0f);
                }   
            }
        }

        // state 값에 따라 하이라이트 여부 set
        public void EnableHighlight(bool state)
        {
            isEnabled = state;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ToggleHighlight(isEnabled);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToggleHighlight(false);
        }
    }
}