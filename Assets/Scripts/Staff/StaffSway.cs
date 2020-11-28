namespace HackYeah
{
    using UnityEngine;

    public sealed class StaffSway : MonoBehaviour
    {
        [SerializeField] private InputController input;

        [SerializeField] private float strength;
        [SerializeField] private float strengthLimit;
        [SerializeField] private float smooth;

        private Vector3 startingPosition;

        private void Start()
        {
            startingPosition = transform.localPosition;
        }

        private void Update()
        {
            var movement = -input.Look;
            movement *= strength;

            movement.x = Mathf.Clamp(movement.x, -strengthLimit, strengthLimit);
            movement.y = Mathf.Clamp(movement.y, -strengthLimit, strengthLimit);

            var moveDir = new Vector3(movement.x, movement.y, 0.0f);
            transform.localPosition = Vector3.Lerp(transform.localPosition, moveDir + startingPosition, Time.deltaTime * smooth);
        }
    }
}