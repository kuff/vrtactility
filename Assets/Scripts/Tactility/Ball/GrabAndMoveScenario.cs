// Copyright (C) 2024 Peter Leth

#region
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
#endregion

namespace Tactility.Ball
{
    public class GrabAndMoveScenario : MonoBehaviour, IScenario
    {
        public Vector3 targetPosition;
        public Vector3 originPosition;
        [FormerlySerializedAs("ug")]
        [Tooltip("The UniformGrabbable component of the object that is being grabbed and moved. This component is used to track the object's position and movement.")]
        public UniformGrabbable grabbable; // The sphere's UniformGrabbable component
        public List<Vector3> targetPositions;
        public FreeFloatable floatable; // The sphere's FreeFloatable component

        private void Start()
        {
            targetPositions = new List<Vector3>
            {
                new Vector3(-0.1f, 0.9f, 0.5f),
                new Vector3(-0.1f, 1.1f, 0.5f),
                new Vector3(0.1f, 0.9f, 0.5f),
                new Vector3(0.1f, 1.1f, 0.5f),
                new Vector3(-0.1f, 0.9f, 0.7f),
                new Vector3(-0.1f, 1.1f, 0.7f),
                new Vector3(0.1f, 0.9f, 0.7f),
                new Vector3(0.1f, 1.1f, 0.7f)
            };
            
            floatable = grabbable!.gameObject.GetComponent<FreeFloatable>();
            // UpdateTargetPosition();

            WhenOnSuccess += () => floatable.ResetPosition();
            WhenOnFailure += () => floatable.ResetPosition();
            // WhenOnSuccess += UpdateTargetPosition;
            // WhenOnFailure += UpdateTargetPosition;

#if DEBUG
            WhenOnSuccess += () => Debug.Log($"{this} target reached!");
            WhenOnFailure += () => Debug.Log($"{this} target failed!");
#endif
        }

        private void Update()
        {
            if (grabbable && grabbable.isGrabbed)
            {
                UpdateProgress();
            }
            else
            {
                Progress = 0f;
            }
        }

        public float Progress { get; private set; }

        private void UpdateProgress()
        {
            var origin = originPosition;
            var progress = (Vector3.Distance(origin, targetPosition) - Vector3.Distance(floatable.transform.position, targetPosition)) / Vector3.Distance(origin, targetPosition);
            Debug.Log($"1: {Vector3.Distance(origin, targetPosition)}");
            Debug.Log($"2: {Vector3.Distance(floatable.transform.position, targetPosition)}");
            Debug.Log($"3: {progress}");
            Progress = Mathf.Clamp01(progress); // Clamp between 0 and 1

            if (Progress >= 0.98f)
            {
                WhenOnSuccess?.Invoke();
            }
        }
        
        private void UpdateTargetPosition()
        {
            var index = Random.Range(0, targetPositions.Count);
            targetPosition = targetPositions[index];
            originPosition = index < targetPositions.Count / 2 ? targetPositions[Random.Range(targetPositions.Count / 2, targetPositions.Count)] : targetPositions[Random.Range(0, targetPositions.Count / 2)];
            floatable.OriginPoint = originPosition;
            
            // Print positions
            Debug.Log($"Target position: {targetPosition}");
            Debug.Log($"Origin position: {originPosition}");
        }

#pragma warning disable CS0067 // The event is never used
        public event ScenarioOnSuccess WhenOnSuccess;
        public event ScenarioOnFailure WhenOnFailure;
#pragma warning restore CS0067 // The event is never used
    }
}
