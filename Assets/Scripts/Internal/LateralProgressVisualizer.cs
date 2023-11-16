using UnityEngine;

namespace Internal
{
    [RequireComponent(typeof(GrabAndMoveScenario))]
    public class LateralProgressVisualizer : MonoBehaviour
    {
        [SerializeField] private float visualSize = .01f;           // Size of the visual indicator
        [SerializeField] private float offsetDistance = .1f;        // Distance from the original object
        [SerializeField] private Material transparentMaterial;
        [SerializeField] private float animationDuration = 1.0f;    // Duration of the animation
        [SerializeField] private float waitBetweenLoops = 1.5f;     // Wait time between animation loops

        private GrabAndMoveScenario _scenario;
        private UniformGrabbable _ug;
        private Transform _headTransform;
        
        private GameObject _visualProgressInstance;
        private GameObject _targetProgressInstance;
        private float _animationTimer;
        private bool _isAnimating;

        private void Start()
        {
            _scenario = GetComponent<GrabAndMoveScenario>();
            _ug = FindObjectOfType<UniformGrabbable>();
            _headTransform = FindObjectOfType<OVRInitializer>().headTransform;
        }

        private void Update()
        {
            if (_ug.isGrabbed)
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
                if (_visualProgressInstance == null) return;
                
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
            var headToUGDirection = (_ug.transform.position - _headTransform.position).normalized;
            // Calculate the perpendicular vector to the left or right
            var perpendicularDirection = _ug.IsLeftHandTouching() ? Vector3.Cross(Vector3.up, headToUGDirection) : Vector3.Cross(headToUGDirection, Vector3.up);

            // Apply the offset
            var offset = perpendicularDirection * offsetDistance;
            _visualProgressInstance.transform.position = _ug.transform.position + offset;
            _targetProgressInstance.transform.position = new Vector3(_visualProgressInstance.transform.position.x, _scenario.targetHeight, _visualProgressInstance.transform.position.z);
        }

        private void UpdateSphereScale()
        {
            if (!_isAnimating)
            {
                _visualProgressInstance.transform.localScale = Vector3.one * visualSize;  // Reset scale when not animating
                return;
            }

            var distanceToTarget = Vector3.Distance(_visualProgressInstance.transform.position, _targetProgressInstance.transform.position);
            var scaleMultiplier = Mathf.Clamp01(1 - (distanceToTarget / offsetDistance));
            _visualProgressInstance.transform.localScale = Vector3.one * visualSize * (1 + scaleMultiplier);
        }

        private void AnimateTowardsTarget()
        {
            if (!_isAnimating) return;
            
            _animationTimer += Time.deltaTime;
            var lerpFactor = Mathf.SmoothStep(0.0f, 1.0f, _animationTimer / animationDuration);  // Ease-in and ease-out effect
            _visualProgressInstance.transform.position = Vector3.Lerp(_visualProgressInstance.transform.position, _targetProgressInstance.transform.position, lerpFactor);

            if (!(_animationTimer >= animationDuration)) return;
            
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
