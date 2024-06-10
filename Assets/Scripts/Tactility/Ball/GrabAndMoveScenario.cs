// Copyright (C) 2024 Peter Leth

#region
using System;
using System.Collections.Generic;
using Tactility.Modulation;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        public int currentForceLevel;

        public Text textBox;

        public LoadingIndicator loadingIndicator;
        
        private bool _wasGrabbed;
        private ITactilityDataProvider _dataProvider;
        private TrialManager _trialManager;

        private string _pressureString;
        private string _triggerString;
        
        private float _pressureOutsideTime;
        private const float AllowedOutsideTime = 1.0f;  // 1 second

        private float dwellTime = 1f; // Time to stay close to the target before succeeding

        private bool isDwellTimeCounting = false; // Flag to check if dwell time is being counted
        private float dwellTimer = 0f; // Timer for dwell time

        //dynamic ranges to stabilize the force level

        private float _level6Threshold = 0.75f;
        private float _level5Threshold = 0.6f;
        private float _level4Threshold = 0.45f;
        private float _level3Threshold = 0.3f;
        private float _level2Threshold = 0.15f;
        private float _toleranceThr = 0.1f;
        private float[] forceThresholds = new float [4];

        private bool _outOfForceLevel = false;

        private int _currentState = 0;  //to log time events
        

        private void Start()
        {            
            _dataProvider = GetComponent<ITactilityDataProvider>();
            _trialManager = GetComponent<TrialManager>();
            
            floatable = grabbable!.gameObject.GetComponent<FreeFloatable>();
            loadingIndicator = GetComponent<LoadingIndicator>();

            // UpdateTargetPosition();

            // WhenOnSuccess += () => floatable.ResetPosition();
            // WhenOnFailure += (ScenarioTrigger cause) => floatable.ResetPosition();
            // WhenOnSuccess += UpdateTargetPosition;
            // WhenOnFailure += UpdateTargetPosition;

#if DEBUG
            WhenOnSuccess += (ScenarioTrigger cause) => Debug.Log($"{this} target reached!");
            WhenOnFailure += (ScenarioTrigger cause) => Debug.Log($"{this} target failed!");
#endif
        }

        private void Update()
        {
            textBox.text = $"Pressure: {_pressureString}\nScenario: {_triggerString}\nTrial: {_trialManager.fileLineIndex}";
            _triggerString = SceneManager.GetActiveScene().name;

            if (grabbable && grabbable.isGrabbed)
            {
                var origin = originPosition;
                var progress = (Vector3.Distance(origin, targetPosition) - Vector3.Distance(floatable.transform.position, targetPosition)) / Vector3.Distance(origin, targetPosition);
                Progress = Mathf.Clamp01(progress);

                if (_currentState != 1)
                {
                    _currentState = 1; //object grasped
                    Logger.LogTimeEvent(_currentState);
                }

                if (SceneManager.GetActiveScene().buildIndex != 4)
                {
                    if (Progress >= 0.9f && currentForceLevel == _trialManager.targetForceLevel)
                    {
                        if (!isDwellTimeCounting)
                        {
                            isDwellTimeCounting = true;
                            dwellTimer = 0f;
                            loadingIndicator.HideLoadingIndicator();
                        }
                        else
                        {
                            dwellTimer += Time.deltaTime;
                            loadingIndicator.ShowLoadingIndicator(targetPosition);
                            loadingIndicator.UpdateProgress(dwellTimer);
                            if (dwellTimer >= dwellTime)
                            {
                                loadingIndicator.HideLoadingIndicator();
                                WhenOnSuccess?.Invoke(ScenarioTrigger.Success);
                                _triggerString = "Success";
                                _currentState = 0;
                                Progress = 0f;
                                Logger.LogScenarioState(0);
                                return;
                            }
                        }
                    }
                    else
                    {
                        isDwellTimeCounting = false;
                        dwellTimer = 0f;
                        loadingIndicator.HideLoadingIndicator();
                    }
                }
                else
                {
                    if (Progress >= 0.9f)
                    {
                        if (!isDwellTimeCounting)
                        {
                            isDwellTimeCounting = true;
                            dwellTimer = 0f;
                            loadingIndicator.HideLoadingIndicator();
                        }
                        else
                        {
                            dwellTimer += Time.deltaTime;
                            loadingIndicator.ShowLoadingIndicator(targetPosition);
                            loadingIndicator.UpdateProgress(dwellTimer);
                            if (dwellTimer >= dwellTime)
                            {
                                loadingIndicator.HideLoadingIndicator();
                                WhenOnSuccess?.Invoke(ScenarioTrigger.Success);
                                _triggerString = "Success";
                                _currentState = 0;
                                Progress = 0f;
                                Logger.LogScenarioState(0);
                                return;
                            }
                        }
                    }
                    else
                    {
                        isDwellTimeCounting = false;
                        dwellTimer = 0f;
                        loadingIndicator.HideLoadingIndicator();
                    }
                }
                

                ref var modulationData = ref _dataProvider.GetTactilityData();

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

                var maxPressure = Mathf.Max(valueBatch);

                if (currentForceLevel != _trialManager.targetForceLevel)
                {
                    _level6Threshold = 0.75f;
                    _level5Threshold = 0.6f;
                    _level4Threshold = 0.45f;
                    _level3Threshold = 0.3f;
                    _level2Threshold = 0.15f;

                    _outOfForceLevel = true;
                }

                // Adjust thresholds to make the current level more tolerant
                else if (currentForceLevel == _trialManager.targetForceLevel && _outOfForceLevel)
                {
                    _outOfForceLevel = false;

                    switch (_trialManager.targetForceLevel)
                    {
                        case 6:
                            _level5Threshold -= _toleranceThr * 1.5f;
                            _level4Threshold -= _toleranceThr;
                            _level3Threshold += _toleranceThr / 2;
                            break;
                        case 5:
                            _level6Threshold += _toleranceThr;
                            _level5Threshold -= _toleranceThr;
                            _level4Threshold -= _toleranceThr / 2;
                            break;
                        case 4:
                            _level6Threshold += _toleranceThr / 2;
                            _level5Threshold += _toleranceThr;
                            _level4Threshold -= _toleranceThr;
                            _level3Threshold -= _toleranceThr / 2;
                            break;
                        case 3:
                            _level5Threshold += _toleranceThr / 2;
                            _level4Threshold += _toleranceThr;
                            _level3Threshold -= _toleranceThr;
                            _level2Threshold -= _toleranceThr / 2;
                            break;
                        case 2:
                            _level4Threshold += _toleranceThr / 2;
                            _level3Threshold += _toleranceThr;
                            _level2Threshold -= _toleranceThr;
                            break;
                        case 1:
                            _level4Threshold += _toleranceThr / 2;
                            _level3Threshold += _toleranceThr;
                            _level2Threshold += _toleranceThr * 1.5f;
                            break;
                    }
                }

                // Calculate the current force level with the adjusted (more tolerant) thresholds
                if (maxPressure > _level6Threshold)
                {
                    currentForceLevel = 6;
                }
                else if (maxPressure > _level5Threshold)
                {
                    currentForceLevel = 5;
                }
                else if (maxPressure > _level4Threshold)
                {
                    currentForceLevel = 4;
                }
                else if (maxPressure > _level3Threshold)
                {
                    currentForceLevel = 3;
                }
                else if (maxPressure > _level2Threshold)
                {
                    currentForceLevel = 2;
                }
                else if (maxPressure == 0)
                {
                    currentForceLevel = 0;
                }
                else
                {
                    currentForceLevel = 1;
                }

                Logger.LogForce(maxPressure, progress, currentForceLevel);

                _pressureString = currentForceLevel.ToString();

                forceThresholds[0] = _level2Threshold;
                forceThresholds[1] = _level3Threshold;
                forceThresholds[2] = _level4Threshold;
                forceThresholds[3] = _level5Threshold;

                Logger.LogForceThresholds(forceThresholds);

                if (Progress <= safeRadius)
                {
                    return;
                }

                if (SceneManager.GetActiveScene().buildIndex != 1 && SceneManager.GetActiveScene().buildIndex != 4)
                {
                    var isPressureOutside = currentForceLevel != _trialManager.targetForceLevel;

                    if (isPressureOutside)
                    {
                        _pressureOutsideTime += Time.deltaTime;

                        if (_pressureOutsideTime >= AllowedOutsideTime)
                        {
                            if (currentForceLevel > _trialManager.targetForceLevel)
                            {
                                Logger.LogScenarioState(2);
                                WhenOnFailure?.Invoke(ScenarioTrigger.TooMuchPressure);
                                _triggerString = "Too much pressure";
                                _currentState = 0;
                            }
                            else if (currentForceLevel < _trialManager.targetForceLevel)
                            {
                                Logger.LogScenarioState(1);
                                WhenOnFailure?.Invoke(ScenarioTrigger.TooLittlePressure);
                                _triggerString = "Too little pressure";
                                _currentState = 0;
                            }
                            Progress = 0f;
                            _pressureOutsideTime = 0f; // Reset the timer
                        }
                    }
                    else
                    {
                        _pressureOutsideTime = 0f; // Reset the timer if pressure is back within range
                    }
                }
            }
            else if (grabbable.allowGrabbing)
            {
                loadingIndicator.HideLoadingIndicator();
                if (Progress > 0.1f)
                {
                    Logger.LogScenarioState(3);
                    //WhenOnFailure?.Invoke(ScenarioTrigger.LossOfGrab);
                    //_triggerString = "Loss of grab";
                    _currentState = 0;
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
