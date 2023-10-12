using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

namespace RW.MonumentValley
{

    // win 조건과 필요에 따라 게임 정지를 모니터링
    public class GameManager : MonoBehaviour
    {
        // reference to player's controller component
        private PlayerController playerController;

        // win 조건을 달성했는지
        private bool isGameOver;
        public bool IsGameOver => isGameOver;

        // 재시작까지 딜레이 타임
        public float delayTime = 2f;

        // awake 됐을 때 실행할 이벤트
        public UnityEvent awakeEvent;

        // level을 시작할 때 실행할 이벤트
        public UnityEvent initEvent;

        // ending level 전에 실행할 이벤트
        public UnityEvent endLevelEvent;


        private void Awake()
        {
            awakeEvent.Invoke();

            // get a reference to the player
            playerController = FindObjectOfType<PlayerController>();
        }

        // 게임 시작시의 이벤트 발생
        private void Start()
        {
            initEvent.Invoke();
        }

        // 매 프레임 win 조건 확인
        private void Update()
        {
            if (playerController != null && playerController.HasReachedGoal())
            {
                Win();
            }
        }

        // 이기고 레벨 종료
        private void Win()
        {
            // win trigger를 한 번 발생시키기 위한 flag
            if (isGameOver || playerController == null)
            {
                return;
            }
            isGameOver = true;

            // player controller를 disable
            playerController.EnableControls(false);

            // 이겼을 때 애니메이션
            StartCoroutine(WinRoutine());
        }

        // 레벨 종료 이벤트 발생
        private IEnumerator WinRoutine()
        {
            if (endLevelEvent != null)
                endLevelEvent.Invoke();

            // yield Animation time
            yield return new WaitForSeconds(delayTime);
        }

        // 씬 재시작
        public void Restart(float delay)
        {
            StartCoroutine(RestartRoutine(delay));
        }

        // 딜레이를 기다렸다가 씬 재시작
        private IEnumerator RestartRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }
    }
}
