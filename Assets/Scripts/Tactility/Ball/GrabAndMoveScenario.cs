// Copyright (C) 2024 Peter Leth

#region
using System.Collections.Generic;
using Tactility.Modulation;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
#endregion

namespace Tactility.Ball
{
    [RequireComponent(typeof(ITactilityDataProvider))]
    [RequireComponent(typeof(TrialManager))]
    public class GrabAndMoveScenario : MonoBehaviour, IScenario
    {
        public Vector3 targetPosition;
        public Vector3 originPosition;
        [FormerlySerializedAs("ug")]
        [Tooltip("The UniformGrabbable component of the object that is being grabbed and moved. This component is used to track the object's position and movement.")]
        public UniformGrabbable grabbable; // The sphere's UniformGrabbable component
        // public List<Vector3> targetPositions;
        public FreeFloatable floatable; // The sphere's FreeFloatable component
        [Tooltip("The radius around the target position that is considered safe. Pressure will only start to be evaluated when the object is moved outside the radius.")]
        public float safeRadius = 0.1f;

        public Text textBox;
        
        private bool _wasGrabbed;
        private ITactilityDataProvider _dataProvider;
        private TrialManager _trialManager;

        private string _pressureString;
        private string _failureString;

        private void Start()
        {
            // targetPositions = new List<Vector3>
            // {
            //     new Vector3(-0.1f, 0.9f, 0.5f),
            //     new Vector3(-0.1f, 1.1f, 0.5f),
            //     new Vector3(0.1f, 0.9f, 0.5f),
            //     new Vector3(0.1f, 1.1f, 0.5f),
            //     new Vector3(-0.1f, 0.9f, 0.7f),
            //     new Vector3(-0.1f, 1.1f, 0.7f),
            //     new Vector3(0.1f, 0.9f, 0.7f),
            //     new Vector3(0.1f, 1.1f, 0.7f)
            // };
            
            _dataProvider = GetComponent<ITactilityDataProvider>();
            _trialManager = GetComponent<TrialManager>();
            
            floatable = grabbable!.gameObject.GetComponent<FreeFloatable>();
            // UpdateTargetPosition();

            WhenOnSuccess += () => floatable.ResetPosition();
            WhenOnFailure += (CauseOfFailure cause) => floatable.ResetPosition();
            // WhenOnSuccess += UpdateTargetPosition;
            // WhenOnFailure += UpdateTargetPosition;

#if DEBUG
            WhenOnSuccess += () => Debug.Log($"{this} target reached!");
            WhenOnFailure += (CauseOfFailure cause) => Debug.Log($"{this} target failed!");
#endif
        }

        private void Update()
        {
            textBox.text = $"Pressure: {_pressureString}\nFailure: {_failureString}";
            
            if (grabbable && grabbable.isGrabbed)
            {
                var origin = originPosition;
                var progress = (Vector3.Distance(origin, targetPosition) - Vector3.Distance(floatable.transform.position, targetPosition)) / Vector3.Distance(origin, targetPosition);
                // Debug.Log($"1: {Vector3.Distance(origin, targetPosition)}");
                // Debug.Log($"2: {Vector3.Distance(floatable.transform.position, targetPosition)}");
                // Debug.Log($"3: {progress}");
                Progress = Mathf.Clamp01(progress); // Clamp between 0 and 1
                
                if (Progress >= 0.95f)
                {
                    WhenOnSuccess?.Invoke();
                }
                
                // Check for pressure
                ref var modulationData = ref _dataProvider.GetTactilityData();
                
                // Update stimuli for each touching finger bone of interest
                var valueBatch = new float[2];
                for (var i = 0; i < modulationData.BoneIds.Count; i++)
                {
                    var pressure = modulationData.Values[i];
                    // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                    switch (modulationData.BoneIds[i])
                    {
                        case OVRSkeleton.BoneId.Hand_Thumb3:
                            valueBatch[0] = pressure;
                            break;
                        case OVRSkeleton.BoneId.Hand_Index3:
                            valueBatch[1] = pressure;
                            break;
                    }
                }
                
                // Get the largest pressure value
                var maxPressure = Mathf.Max(valueBatch);
                
                // Debug.Log("max pressure: " + maxPressure);
                
                var pressureValue = maxPressure switch
                {
                    > 0.85f => 6,
                    > 0.6f => 5,
                    > 0.45f => 4,
                    > 0.3f => 3,
                    > 0.15f => 2,
                    _ => 1
                };
                
                // Debug.Log(pressureValue);
                _pressureString = pressureValue.ToString();
                
                // Debug.Log("Target force level: " + _trialManager.targetForceLevel);
                
                // Invoke failure if pressure level is different from current force level
                if (Progress <= safeRadius)
                {
                    return;
                }
                if (pressureValue > _trialManager.targetForceLevel)
                {
                    WhenOnFailure?.Invoke(CauseOfFailure.TooMuchPressure);
                    _failureString = "Too much pressure";
                }
                else if (pressureValue < _trialManager.targetForceLevel)
                {
                    WhenOnFailure?.Invoke(CauseOfFailure.TooLittlePressure);
                    _failureString = "Too little pressure";
                }
            }
            else
            {
                if (Progress > 0.1f)
                {
                    WhenOnFailure?.Invoke(CauseOfFailure.LossOfGrab);
                    _failureString = "Loss of grab";
                }
                Progress = 0f;
                
            }
        }

        public float Progress { get; private set; }
        
        // private void UpdateTargetPosition()
        // {
        //     var index = Random.Range(0, targetPositions.Count);
        //     targetPosition = targetPositions[index];
        //     originPosition = index < targetPositions.Count / 2 ? targetPositions[Random.Range(targetPositions.Count / 2, targetPositions.Count)] : targetPositions[Random.Range(0, targetPositions.Count / 2)];
        //     floatable.OriginPoint = originPosition;
        //     
        //     // Print positions
        //     Debug.Log($"Target position: {targetPosition}");
        //     Debug.Log($"Origin position: {originPosition}");
        // }

#pragma warning disable CS0067 // The event is never used
        public event ScenarioOnSuccess WhenOnSuccess;
        public event ScenarioOnFailure WhenOnFailure;
#pragma warning restore CS0067 // The event is never used
    }
}
