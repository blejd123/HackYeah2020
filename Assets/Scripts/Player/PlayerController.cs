namespace HackYeah
{
    using System.Collections.Generic;
    using UnityEngine;

    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private float playerSpeed = 2.0f;
        [SerializeField] private float gravityValue = -9.81f;
        [SerializeField] private float groundForce;
        [SerializeField] private LayerMask layerMask;

        [SerializeField] private List<AudioClip> footstepClips;
        [SerializeField] private float stepInterval;

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Transform mainCamera;
        [SerializeField] private InputController input;

        private CharacterController controller;

        private Vector3 movementDirection = Vector3.zero;
        private CollisionFlags collisionFlags;

        private bool wasGrounded;

        private float stepCycle;
        private float nextStep;

        private void Start()
        {
            controller = GetComponent<CharacterController>();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            stepCycle = 0.0f;
            nextStep = stepCycle * 0.5f;
        }

        private void Update()
        {
            if (!wasGrounded && controller.isGrounded)
            {
                movementDirection.y = 0f;
            }

            if (!controller.isGrounded && wasGrounded)
            {
                movementDirection.y = 0f;
            }

            wasGrounded = controller.isGrounded;
        }

        private void FixedUpdate()
        {
            var movement = input.Movement;
            var moveDir = new Vector3(movement.x, 0.0f, movement.y);
            moveDir = mainCamera.transform.forward * moveDir.z + mainCamera.transform.right * moveDir.x;

            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, controller.radius, Vector3.down, out hitInfo, controller.height * 0.5f, layerMask, QueryTriggerInteraction.Ignore);
            moveDir = Vector3.ProjectOnPlane(moveDir, hitInfo.normal).normalized;

            movementDirection.x = moveDir.x * playerSpeed;
            movementDirection.z = moveDir.z * playerSpeed;

            if (controller.isGrounded)
            {
                movementDirection.y = -groundForce;
            }
            else
            {
                movementDirection += Physics.gravity * gravityValue * Time.fixedDeltaTime;
            }

            collisionFlags = controller.Move(movementDirection * Time.fixedDeltaTime);

            WalkCycle(playerSpeed);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            var body = hit.collider.attachedRigidbody;

            if (collisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }

            body.AddForceAtPosition(controller.velocity * 0.1f, hit.point, ForceMode.Impulse);
        }

        private void WalkCycle(float delta)
        {
            if(controller.velocity.sqrMagnitude > 0 && input.Movement != Vector2.zero)
            {
                stepCycle += (controller.velocity.magnitude + delta) * Time.fixedDeltaTime;
            }

            if (!(stepCycle > nextStep))
            {
                return;
            }

            nextStep = stepCycle + stepInterval;

            if (controller.isGrounded == false)
            {
                return;
            }

            var random = Random.Range(1, footstepClips.Count);
            audioSource.clip = footstepClips[random];
            audioSource.PlayOneShot(audioSource.clip);

            footstepClips[random] = footstepClips[0];
            footstepClips[0] = audioSource.clip;
        }
    }
}