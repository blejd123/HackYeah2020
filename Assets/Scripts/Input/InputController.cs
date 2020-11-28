namespace HackYeah
{
    using UnityEngine;

    public sealed class InputController : MonoBehaviour
    {
        public Vector2 Movement
        {
            get
            {
                if (inputActions == null)
                {
                    return Vector2.zero;
                }
                return inputActions.Player.Move.ReadValue<Vector2>();
            }
        }
        public Vector2 Look
        {
            get
            {
                if (inputActions == null)
                {
                    return Vector2.zero;
                }
                return inputActions.Player.Look.ReadValue<Vector2>();
            }
        }

        private InputActions inputActions;

        private void Awake()
        {
            inputActions = new InputActions();
        }

        private void OnEnable()
        {
            inputActions.Enable();
        }

        private void OnDisable()
        {
            inputActions.Disable();
        }
    }
}