using UnityEngine;

namespace Internal.Ball
{
    public class GrabAndMoveScenario : MonoBehaviour, IScenario
    {
        [Tooltip("The target height that the grabbed object needs to reach. This height is used as a goal for the progress calculation and visual indicators.")]
        public float targetHeight;
        
        public float Progress { get; private set; }

        public event ScenarioOnSuccess WhenOnSuccess;
        public event ScenarioOnFailure WhenOnFailure;

        private UniformGrabbable _ug;   // The sphere's UniformGrabbable component
        private FreeFloatable _ff;      // The sphere's FreeFloatable component

        private void Start()
        {
            _ug = FindObjectOfType<UniformGrabbable>();
            _ff = _ug!.gameObject.GetComponent<FreeFloatable>();

            WhenOnSuccess += () => _ff.ResetPosition();
            WhenOnFailure += () => _ff.ResetPosition();

#if UNITY_EDITOR
            WhenOnSuccess += () => Debug.Log($"{this} target reached!");
            WhenOnFailure += () => Debug.Log($"{this} target failed!");
#endif
        }

        private void Update()
        {
            if (_ug && _ug.isGrabbed)
                UpdateProgress(_ug.gameObject.transform.position.y);
            else 
                Progress = 0f;
        }

        private void UpdateProgress(float currentY)
        {
            var originY = _ff.OriginPoint.y;
            Progress = (currentY - originY) / (targetHeight - originY);
            Progress = Mathf.Clamp01(Progress);  // Clamp between 0 and 1
            
            if (Progress >= 1f) WhenOnSuccess?.Invoke();
        }
    }
}