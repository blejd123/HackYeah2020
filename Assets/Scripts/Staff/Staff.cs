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

        private float _lastUseTime = -100.0f;

        private void Update()
        {
            if (input.UseStaff)
            {
                animator.SetTrigger("Use");
                if (Time.time > _lastUseTime)
                {
                    _lastUseTime = Time.time;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (Time.time > _lastUseTime + 0.75f)
            {
                return;
            }
            
            _lastUseTime = 0.0f;
            var closest = head.position;
            HitDetectorUtilities.DetectHit(closest, _hitSoundRange, null);
        }

        private void OnTriggerStay(Collider other)
        {
            if (Time.time > _lastUseTime + 0.75f)
            {
                return;
            }

            _lastUseTime = 0.0f;
            var closest = head.position;
            HitDetectorUtilities.DetectHit(closest, _hitSoundRange, null);
        }
    }
}