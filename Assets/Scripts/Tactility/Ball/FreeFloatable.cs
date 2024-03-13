using UnityEngine;

namespace Tactility.Ball
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(UniformGrabbable))]
    public class FreeFloatable : MonoBehaviour
    {
        private const float DotMoveThreshold = 0.4f;
        
        [Tooltip("Determines whether the object should move relative to the player's head movement. When enabled, the object will adjust its position based on the orientation and position of the player's head.")]
        public bool moveWithPlayerHead;
        [Tooltip("Specifies the height at which the object should reset when 'ResetPosition' is called. This height determines the vertical position of the object upon reset.")]
        public float resetHeight;

        public Vector3 OriginPoint { get; private set; }
        
        [Tooltip("The base force applied to move the object towards its origin point. This force determines how strongly the object is pushed back to its starting position.")]
        [SerializeField] private float baseForce;
        [Tooltip("A factor that restricts the movement of the object towards its origin point. A higher value results in less force applied, dampening the movement.")]
        [SerializeField] private float restriction;

        private Rigidbody _rigidbody;
        private float _originDistanceFromCenter;
        private Transform _playerHeadTransform;

        private UniformGrabbable _localGrabbable;
        private FixedJoint _localFixedJoint;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            OriginPoint = transform.position;
            _originDistanceFromCenter = Vector3.Distance(new Vector3(OriginPoint.x, 0, OriginPoint.z), Vector3.zero);
            _playerHeadTransform = GameObject.FindGameObjectWithTag("MainCamera")!.transform;
            _localGrabbable = GetComponent<UniformGrabbable>();
        }

        private void FixedUpdate()
        {
            if (_localGrabbable.isGrabbed && _localFixedJoint is null)
            {
                var touchingHand = _localGrabbable.GetTouchingHandRoot();
                var joint = gameObject.AddComponent<FixedJoint>();
            
                joint.anchor = touchingHand!.position;
                joint.connectedBody = touchingHand!.GetComponentInParent<Rigidbody>();
                joint.enableCollision = false;
                _localFixedJoint = joint;
                touchingHand!.isKinematic = true;
            }
            else if (!_localGrabbable.isGrabbed && _localFixedJoint is not null)
            {
                var touchingHand = _localGrabbable.GetTouchingHandRoot();
                if (!touchingHand)
                {
                    return;
                }

                touchingHand.isKinematic = false;
                Destroy(_localFixedJoint);
                _localFixedJoint = null;
            }

            if (moveWithPlayerHead)
            {
                // Calculate the direction from the player's head to the object
                var delta = transform.position - _playerHeadTransform.position;
                delta.y = 0f; // Make sure the movement is only in the XZ plane

                // Calculate the dot product to check the relative position
                var forward = _playerHeadTransform.TransformDirection(Vector3.forward);
                forward.y = 0f;
                var dot = Vector3.Dot(forward, delta.normalized);
            
                // If the object is within a certain angle in front of the player
                if (dot < DotMoveThreshold)
                {
                    // Rotate the delta direction around the Y axis by 90 degrees to get a perpendicular direction
                    var rotatedDirection = Quaternion.Euler(0, 90, 0) * delta.normalized;

                    // Move the object in this new direction by the desired distance
                    var newPosition = _playerHeadTransform.position + rotatedDirection * _originDistanceFromCenter;
                    newPosition.y = OriginPoint.y;
                    OriginPoint = newPosition;
                }
            }
        
            // Add force to the ball, moving it towards the origin point
            var distanceVector = OriginPoint - transform.position;
            var movementVector = distanceVector * (baseForce * (1f - restriction));
            _rigidbody.AddForce(movementVector, ForceMode.Impulse);
            _rigidbody.velocity *= distanceVector.magnitude;
        }

        public void ResetPosition()
        {
            transform.position = new Vector3(OriginPoint.x, resetHeight, OriginPoint.z);
        }
    }
}
