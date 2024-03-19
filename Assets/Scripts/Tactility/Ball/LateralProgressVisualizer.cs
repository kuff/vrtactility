// Copyright (C) 2024 Peter Leth

#region
using UnityEngine;
#endregion

namespace Tactility.Ball
{
    [RequireComponent(typeof(GrabAndMoveScenario))]
    public class LateralProgressVisualizer : MonoBehaviour
    {
        [Tooltip("Initial size of the visual sphere indicator. This determines the starting size of the sphere before any animation or scaling takes place.")]
        [SerializeField] private float visualSize = .01f;
        [Tooltip("Distance from the 'UniformGrabbable' object at which the visual sphere indicator is placed. This determines how far to the side (left or right) the sphere appears from the object.")]
        [SerializeField] private float offsetDistance = .1f;
        [Tooltip("Material used for the visual sphere indicator. This material will be applied to the sphere to provide the desired visual effect.")]
        [SerializeField] private Material transparentMaterial;
        [Tooltip("Duration of the animation cycle where the sphere moves towards the target height. This is the time it takes for one complete animation from start to finish.")]
        [SerializeField] private float animationDuration = 1.0f;
        [Tooltip("Wait time between the end of one animation loop and the start of the next. This determines the pause duration before the animation restarts.")]
        [SerializeField] private float waitBetweenLoops = 1.5f;
        private float _animationTimer;
        private Transform _headTransform;
        private bool _isAnimating;

        private GrabAndMoveScenario _scenario;
        private GameObject _targetProgressInstance;

        private GameObject _visualProgressInstance;

        private void Start()
        {
            _scenario = GetComponent<GrabAndMoveScenario>();
            _headTransform = FindObjectOfType<OVRInitializer>().headTransform;
        }

        private void Update()
        {
            if (_scenario.grabbable.isGrabbed)
            {
                if (_visualProgressInstance == null)
                {
                    _visualProgressInstance = CreateSphere(visualSize);
                    _targetProgressInstance = CreateSphere(visualSize * 2);
                    StartAnimation();
                }

                UpdateSpherePositions();
                AnimateTowardsTarget();
                UpdateSphereScale();
            }
            else
            {
                if (_visualProgressInstance == null)
                {
                    return;
                }

                Destroy(_visualProgressInstance);
                Destroy(_targetProgressInstance);
            }
        }

        private GameObject CreateSphere(float size)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.localScale = Vector3.one * size;
            if (transparentMaterial != null)
            {
                sphere.GetComponent<Renderer>().material = transparentMaterial;
            }
            return sphere;
        }

        private void UpdateSpherePositions()
        {
            // Calculate the vector from the head to the _ug object
            var headToUgDirection = (_scenario.grabbable.transform.position - _headTransform.position).normalized;

            // Calculate the perpendicular vector to the left or right
            var perpendicularDirection = _scenario.grabbable.IsLeftHandTouching()
                ? Vector3.Cross(Vector3.up, headToUgDirection)
                : Vector3.Cross(headToUgDirection, Vector3.up);

            // Apply the offset
            var offset = perpendicularDirection * offsetDistance;
            var position = _scenario.grabbable.transform.position + offset;
            _visualProgressInstance.transform.position = position;
            _targetProgressInstance.transform.position = new Vector3(position.x, _scenario.targetHeight, position.z);
        }

        private void UpdateSphereScale()
        {
            var distanceToTarget = Vector3.Distance(_visualProgressInstance.transform.position, _targetProgressInstance.transform.position);
            var scaleMultiplier = Mathf.Clamp01(1 - distanceToTarget / offsetDistance);
            _visualProgressInstance.transform.localScale = Vector3.one * (visualSize * (1 + scaleMultiplier));
        }

        private void AnimateTowardsTarget()
        {
            if (!_isAnimating)
            {
                return;
            }

            _animationTimer += Time.deltaTime;
            var lerpFactor = Mathf.SmoothStep(0.0f, 1.0f, _animationTimer / animationDuration); // Ease-in and ease-out effect
            _visualProgressInstance.transform.position = Vector3.Lerp(_visualProgressInstance.transform.position, _targetProgressInstance.transform.position, lerpFactor);

            if (!(_animationTimer >= animationDuration))
            {
                return;
            }

            _isAnimating = false;
            _animationTimer = 0.0f;
            Invoke(nameof(StartAnimation), waitBetweenLoops);
        }

        private void StartAnimation()
        {
            _isAnimating = true;
            _animationTimer = 0.0f;
        }
    }
}
