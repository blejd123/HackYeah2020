namespace HackYeah
{
    using UnityEngine;

    public sealed class CameraPOV : MonoBehaviour
    {
        [SerializeField] private bool clampVertical = true;
        [SerializeField] private float clampAngle = 80.0f;
        [SerializeField] private float horizontalSpeed = 10.0f;
        [SerializeField] private float verticalSpeed = 10.0f;

        [SerializeField] private bool smooth;
        [SerializeField] private float smoothTime = 5f;

        [SerializeField] private InputController input;
        [SerializeField] private Transform characterTarget;
        [SerializeField] private Transform cameraTarget;

        private Quaternion characterTargetRotation;
        private Quaternion cameraTargetRotation;

        private void Start()
        {
            characterTargetRotation = characterTarget.localRotation;
            cameraTargetRotation = cameraTarget.localRotation;
        }

        private void Update()
        {
            float yRot = input.Look.x * horizontalSpeed;
            float xRot = input.Look.y * verticalSpeed;

            characterTargetRotation *= Quaternion.Euler(0f, yRot, 0f);
            cameraTargetRotation *= Quaternion.Euler(-xRot, 0f, 0f);

            if (clampVertical)
            {
                cameraTargetRotation = ClampRotationAroundXAxis(cameraTargetRotation);
            }

            if (smooth)
            {
                characterTarget.localRotation = Quaternion.Slerp(characterTarget.localRotation, characterTargetRotation,
                    smoothTime * Time.deltaTime);
                cameraTarget.localRotation = Quaternion.Slerp(cameraTarget.localRotation, cameraTargetRotation,
                    smoothTime * Time.deltaTime);
            }
            else
            {
                characterTarget.localRotation = characterTargetRotation;
                cameraTarget.localRotation = cameraTargetRotation;
            }
        }

        private Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

            angleX = Mathf.Clamp(angleX, -clampAngle, clampAngle);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }
    }
}
