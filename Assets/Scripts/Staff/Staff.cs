namespace HackYeah
{
    using UnityEngine;

    public sealed class Staff : MonoBehaviour
    {
        [SerializeField] private InputController input;
        [SerializeField] private Transform head;
        [SerializeField] private GameObject particles;
        [SerializeField] private Animator animator;

        private void Update()
        {
            if (input.UseStaff)
            {
                animator.SetTrigger("Use");
            }
        }
    }
}