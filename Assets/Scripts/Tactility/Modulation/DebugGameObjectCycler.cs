// Copyright (C) 2024 Peter Leth

#region
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#endregion

namespace Tactility.Modulation
{
    public class DebugGameObjectCycler : MonoBehaviour
    {
        [Tooltip("List of GameObjects to cycle through.")]
        public List<GameObject> gameObjects;
        [Tooltip("Time in seconds each GameObject is active.")]
        public float cycleDelay = 8f;

        private int _currentIndex = -1;        // Track the current index. Starts at -1 to indicate no GameObject is active initially.
        private Coroutine _cyclingCoroutine;   // To hold the coroutine for controlling its execution.
        private bool _isCyclingEnabled = true; // Control cycling on/off.

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
                if (!Input.GetKeyDown(i.ToString()) || gameObjects.Count < i)
                {
                    continue;
                }

                ToggleCycling(false);      // Disable cycling.
                ActivateGameObject(i - 1); // Activate the selected GameObject.
            }

            // Check for number key 9 to re-enable cycling.
            if (Input.GetKeyDown("9"))
            {
                CycleToNextGameObjectAndContinue(); // Cycle to next GameObject immediately and continue cycling.
            }
        }

        private IEnumerator CycleGameObjects()
        {
            while (_isCyclingEnabled)
            {
                CycleToNextGameObject(); // Move to the next GameObject and enable it.

                // Wait for the specified delay.
                yield return new WaitForSeconds(cycleDelay);

                // Disable the current GameObject before the next cycle begins, if cycling is still enabled.
                if (_isCyclingEnabled)
                {
                    gameObjects[_currentIndex].SetActive(false);
                }
            }
        }

        private void CycleToNextGameObjectAndContinue()
        {
            ToggleCycling(true);     // Ensure cycling is enabled.
            CycleToNextGameObject(); // Move to the next GameObject immediately.

            // If not already running, restart the coroutine to continue cycling from the current position.
            _cyclingCoroutine ??= StartCoroutine(CycleGameObjects());
        }

        private void ToggleCycling(bool enable)
        {
            _isCyclingEnabled = enable;

            if (enable || _cyclingCoroutine == null)
            {
                return;
            }

            // Stop the cycling coroutine if it's running.
            StopCoroutine(_cyclingCoroutine);
            _cyclingCoroutine = null;
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

        private void CycleToNextGameObject()
        {
            // Increment the currentIndex and wrap it around if it exceeds the list count.
            _currentIndex = (_currentIndex + 1) % gameObjects.Count;

            // Disable all GameObjects before enabling the next one.
            foreach (var go in gameObjects)
            {
                go.SetActive(false);
            }

            // Enable the next GameObject.
            gameObjects[_currentIndex].SetActive(true);
        }
    }
}
