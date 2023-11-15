using UnityEngine;

namespace Internal
{
    [RequireComponent(typeof(UniformGrabbable))]
    [RequireComponent(typeof(FreeFloatable))]
    public class BasicGrabAndMoveScenario : MonoBehaviour, IScenario
    {
        [Tooltip("The y-level to which the ball must be moved to complete the task.")]
        public float targetHeight;

        public float Progress { get; private set; }

        public event ScenarioOnSuccess WhenOnSuccess;
        public event ScenarioOnFailure WhenOnFailure;

        private UniformGrabbable _ug;
        private FreeFloatable _ff;

        private void Start()
        {
            _ug = GetComponent<UniformGrabbable>();
            _ff = GetComponent<FreeFloatable>();

            WhenOnSuccess += () => _ff.ResetPosition();
            WhenOnFailure += () => _ff.ResetPosition();
            
            // NOTE: For debugging
#if UNITY_EDITOR
            WhenOnSuccess += () => Debug.Log($"{this} (GrabAndMoveScenario) target reached!");
            WhenOnFailure += () => Debug.Log($"{this} (GrabAndMoveScenario) target failed!");
#endif
        }

        private void Update()
        {
            // Check if the object is being grabbed
            if (_ug.isGrabbed)
            {
                var currY = transform.position.y;
                var originY = _ff.OriginPoint.y;
                
                // Invert the relationship such that Progress is 1 at or below target height and 0 at or above origin height
                // Also, clamp the value between 0 and 1
                Progress = (currY - originY) / (targetHeight - originY);
                Progress = Mathf.Clamp(Progress, 0f, 1f);
                
                if (Progress >= 1f) WhenOnSuccess?.Invoke();
            }
            else
            {
                // Reset progress to 0 if not grabbed
                Progress = 0f;
            }
        }
    }
}