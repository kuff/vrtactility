using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(UniformGrabbable))]
public class FreeFloatable : MonoBehaviour
{
    private const float DotMoveThreshold = 0.4f;
    
    [Tooltip("Moves floatable in front of player head if enabled.")]
    public bool moveWithPlayerHead;

    [SerializeField] private float _baseForce;
    [SerializeField] private float _restriction;

    private Rigidbody _rigidbody;
    private Vector3 _originPoint;
    private float _originDistanceFromCenter;
    private Transform _playerHeadTransform;

    private UniformGrabbable _localGrabbable;
    private FixedJoint _localFixedJoint;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _originPoint = transform.position;
        _originDistanceFromCenter = Vector3.Distance(new Vector3(_originPoint.x, 0, _originPoint.z), Vector3.zero);
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
            if (!touchingHand) return;
            
            touchingHand.isKinematic = false;
            Destroy(_localFixedJoint);
            _localFixedJoint = null;
        }

        if (moveWithPlayerHead)
        {
            // Calculate the direction from the player's head to the object
            var delta = transform.position - _playerHeadTransform.position;
            delta.y = 0f;  // Make sure the movement is only in the XZ plane

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
                newPosition.y = _originPoint.y;
                _originPoint = newPosition;
            }
        }
        
        // Add force to the ball, moving it towards the origin point
        var distanceVector = _originPoint - transform.position;
        var movementVector = distanceVector * (_baseForce * (1f - _restriction));
        _rigidbody.AddForce(movementVector, ForceMode.Impulse);
        _rigidbody.velocity *= distanceVector.magnitude;
    }
}
