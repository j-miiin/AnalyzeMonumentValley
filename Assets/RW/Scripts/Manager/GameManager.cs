using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

namespace RW.MonumentValley
{

    // monitors win condition and stops/pauses gameplay as needed
    public class GameManager : MonoBehaviour
    {
        // reference to player's controller component
        private PlayerController playerController;

        // have we completed the win condition (i.e. reached the goal)?
        private bool isGameOver;
        public bool IsGameOver => isGameOver;

        // delay before restarting, etc.
        public float delayTime = 2f;

        // invoked on awake
        public UnityEvent awakeEvent;

        // invoked when starting the level
        public UnityEvent initEvent;

        // invoked before ending the level
        public UnityEvent endLevelEvent;


        private void Awake()
        {
            awakeEvent.Invoke();

            // get a reference to the player
            playerController = FindObjectOfType<PlayerController>();
        }

        // invoke any events at the start of gameplay
        private void Start()
        {
            initEvent.Invoke();
        }

        // check for win condition every frame
        private void Update()
        {
            if (playerController != null && playerController.HasReachedGoal())
            {
                Win();
            }
        }

        // win and end the level
        private void Win()
        {
            // flag to ensure Win only triggers once
            if (isGameOver || playerController == null)
            {
                return;
            }
            isGameOver = true;

            // disable player controls
            playerController.EnableControls(false);

            // play win animation
            StartCoroutine(WinRoutine());
        }

        // invoke end level event and wait
        private IEnumerator WinRoutine()
        {
            if (endLevelEvent != null)
                endLevelEvent.Invoke();

            // yield Animation time
            yield return new WaitForSeconds(delayTime);
        }

        // restart the scene
        public void Restart(float delay)
        {
            StartCoroutine(RestartRoutine(delay));
        }

        // wait for a delay and restart the scene
        private IEnumerator RestartRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }
    }
}
