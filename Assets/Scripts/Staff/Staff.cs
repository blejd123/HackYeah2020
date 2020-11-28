namespace HackYeah
{
    using UnityEngine;

    public sealed class Staff : MonoBehaviour
    {
        [SerializeField] private InputController input;
        [SerializeField] private Transform head;
        [SerializeField] private GameObject particles;
        [SerializeField] private Animator animator;
        [SerializeField] private float _hitSoundRange;

        private void Update()
        {
            if (input.UseStaff)
            {
                animator.SetTrigger("Use");
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            HitDetectorUtilities.DetectHit(collision.GetContact(0).point, _hitSoundRange);
        }
    }
}