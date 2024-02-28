using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tactility.Modulation
{
    public class DebugGameObjectCycler : MonoBehaviour
    {
        [Tooltip("List of GameObjects to cycle through.")]
        public List<GameObject> gameObjects;
        [Tooltip("Time in seconds each GameObject is active.")]
        public float cycleDelay = 5f;

        private int _currentIndex = -1;         // Track the current index. Starts at -1 to indicate no GameObject is active initially.
        private bool _isCyclingEnabled = true;  // Control cycling on/off.
        private Coroutine _cyclingCoroutine;    // To hold the coroutine for controlling its execution.

        private void Start()
        {
            if (gameObjects.Count > 0)
            {
                _cyclingCoroutine = StartCoroutine(CycleGameObjects());
            }
        }

        private void Update()
        {
            // Check for number keys 1-8 to select specific GameObject and disable cycling.
            for (var i = 1; i <= 8; i++)
            {
                if (!Input.GetKeyDown(i.ToString()) || gameObjects.Count < i) continue;
                
                ToggleCycling(false); // Disable cycling.
                ActivateGameObject(i - 1); // Activate the selected GameObject.
            }

            // Check for number key 9 to re-enable cycling.
            if (Input.GetKeyDown("9")) ToggleCycling(true);
        }

        private IEnumerator CycleGameObjects()
        {
            while (_isCyclingEnabled)
            {
                // Increment the currentIndex and wrap it around if it exceeds the list count.
                _currentIndex = (_currentIndex + 1) % gameObjects.Count;

                // Enable the current GameObject.
                gameObjects[_currentIndex].SetActive(true);

                // Wait for the specified delay.
                yield return new WaitForSeconds(cycleDelay);

                // Check if cycling is still enabled before disabling the current GameObject.
                if (_isCyclingEnabled)
                {
                    gameObjects[_currentIndex].SetActive(false);
                }
            }
        }

        private void ToggleCycling(bool enable)
        {
            _isCyclingEnabled = enable;

            if (enable)
            {
                // Ensure all GameObjects are initially disabled.
                foreach (var go in gameObjects)
                {
                    go.SetActive(false);
                }

                // Restart cycling coroutine if not already running.
                _cyclingCoroutine ??= StartCoroutine(CycleGameObjects());
            }
            else
            {
                // Stop the cycling coroutine if it's running.
                if (_cyclingCoroutine == null) return;
                
                StopCoroutine(_cyclingCoroutine);
                _cyclingCoroutine = null;
            }
        }

        private void ActivateGameObject(int index)
        {
            // Disable all GameObjects before enabling the selected one.
            foreach (var go in gameObjects)
            {
                go.SetActive(false);
            }

            gameObjects[index].SetActive(true);
        }
    }
}
