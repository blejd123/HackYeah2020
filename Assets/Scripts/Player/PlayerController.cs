namespace HackYeah
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        public event Action<Vector3> OnFootstep;

        [SerializeField] private float playerSpeed = 2.0f;
        [SerializeField] private float gravityValue = -9.81f;
        [SerializeField] private float groundForce;
        [SerializeField] private LayerMask layerMask;

        [SerializeField] private List<AudioClip> footstepClips;
        [SerializeField] private float stepInterval;
        [SerializeField] private GameObject stepPrefab;

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Transform mainCamera;
        [SerializeField] private InputController input;

        [SerializeField] private float _footstepSoundRange;
        [SerializeField] private float _forwardOffset;
        [SerializeField] private float _footstepSoundSpeed;

        private CharacterController controller;

        private Vector3 movementDirection = Vector3.zero;

        private bool wasGrounded;

        private float stepCycle;
        private float nextStep;
        private bool wasLeft;
        private float _lastSoundTime;

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

            controller.Move(movementDirection * Time.fixedDeltaTime);
            WalkCycle(playerSpeed);
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

            var random = UnityEngine.Random.Range(1, footstepClips.Count);
            audioSource.clip = footstepClips[random];
            audioSource.PlayOneShot(audioSource.clip);

            footstepClips[random] = footstepClips[0];
            footstepClips[0] = audioSource.clip;

            var position = transform.position;
            if (wasLeft)
            {
                wasLeft = false;
                position.x += controller.radius * 0.5f;
            }
            else
            {
                wasLeft = true;
                position.x -= controller.radius * 0.5f;
            }

            RaycastHit raycastHit;
            if (Physics.Raycast(position, Vector3.down, out raycastHit))
            {
                OnFootstep?.Invoke(raycastHit.point);
                if (Time.time > _lastSoundTime + 2.0f)
                {
                    _lastSoundTime = Time.time;
                    HitDetectorUtilities.DetectHit(raycastHit.point + _forwardOffset * transform.forward, _footstepSoundRange, _footstepSoundSpeed);
                }
            }
        }
    }
}