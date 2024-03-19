// Copyright (C) 2024 Peter Leth

#region
using UnityEngine;
using UnityEngine.Serialization;
#endregion

namespace Tactility.Ball
{
    public class GrabAndMoveScenario : MonoBehaviour, IScenario
    {
        [Tooltip("The target height that the grabbed object needs to reach. This height is used as a goal for the progress calculation and visual indicators.")]
        public float targetHeight;
        [FormerlySerializedAs("ug")]
        [Tooltip("The UniformGrabbable component of the object that is being grabbed and moved. This component is used to track the object's position and movement.")]
        public UniformGrabbable grabbable; // The sphere's UniformGrabbable component

        private FreeFloatable _floatable; // The sphere's FreeFloatable component

        private void Start()
        {
            _floatable = grabbable!.gameObject.GetComponent<FreeFloatable>();

            WhenOnSuccess += () => _floatable.ResetPosition();
            WhenOnFailure += () => _floatable.ResetPosition();

#if DEBUG
            WhenOnSuccess += () => Debug.Log($"{this} target reached!");
            WhenOnFailure += () => Debug.Log($"{this} target failed!");
#endif
        }

        private void Update()
        {
            if (grabbable && grabbable.isGrabbed)
            {
                UpdateProgress(grabbable.gameObject.transform.position.y);
            }
            else
            {
                Progress = 0f;
            }
        }

        public float Progress { get; private set; }

        private void UpdateProgress(float currentY)
        {
            var originY = _floatable.OriginPoint.y;
            Progress = (currentY - originY) / (targetHeight - originY);
            Progress = Mathf.Clamp01(Progress); // Clamp between 0 and 1

            if (Progress >= 1f)
            {
                WhenOnSuccess?.Invoke();
            }
        }

#pragma warning disable CS0067 // The event is never used
        public event ScenarioOnSuccess WhenOnSuccess;
        public event ScenarioOnFailure WhenOnFailure;
#pragma warning restore CS0067 // The event is never used
    }
}
