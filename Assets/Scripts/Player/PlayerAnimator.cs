using UnityEngine;
using Game.Combat;

namespace Game.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimator : MonoBehaviour
    {
        [Header("Ссылки")]
        [SerializeField] private PlayerController controller;
        [SerializeField] private Animator animator;
        [SerializeField] private PlayerHealth health;
        [SerializeField] private PlayerMelee melee;

        [Header("Имена параметров Animator")]
        [SerializeField] private string speedParam = "Speed";
        [SerializeField] private string isGroundedParam = "IsGrounded";
        [SerializeField] private string hurtTrigger = "Hurt";
        [SerializeField] private string attackTrigger = "Attack";

        private int _speedId;
        private int _isGroundedId;
        private int _hurtId;
        private int _attackId;
        private int _lastHealth;

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            if (controller == null) controller = GetComponent<PlayerController>();
            if (health == null) health = GetComponent<PlayerHealth>();
            if (melee == null) melee = GetComponent<PlayerMelee>();

            _speedId = Animator.StringToHash(speedParam);
            _isGroundedId = Animator.StringToHash(isGroundedParam);
            _hurtId = Animator.StringToHash(hurtTrigger);
            _attackId = Animator.StringToHash(attackTrigger);
            if (health != null) _lastHealth = health.CurrentHealth;
        }

        private void OnEnable()
        {
            if (health != null)
                health.OnHealthChanged += OnHealthChanged;
        }

        private void OnDisable()
        {
            if (health != null)
                health.OnHealthChanged -= OnHealthChanged;
        }

        private void Update()
        {
            if (animator == null || !animator.isInitialized) return;

            float speed = 0f;
            bool grounded = false;
            if (controller != null)
            {
                speed = Mathf.Abs(controller.InputX);
                grounded = controller.IsGrounded;
            }

            animator.SetFloat(_speedId, speed);
            animator.SetBool(_isGroundedId, grounded);
        }

        private void OnHealthChanged(int current, int max)
        {
            if (animator == null || !animator.isInitialized) return;
            if (current < _lastHealth) animator.SetTrigger(_hurtId);
            _lastHealth = current;
        }

        public void TriggerAttack()
        {
            if (animator != null && animator.isInitialized)
                animator.SetTrigger(_attackId);
        }
    }
}
