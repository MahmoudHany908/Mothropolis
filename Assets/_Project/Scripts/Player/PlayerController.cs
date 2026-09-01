using UnityEngine;
using UnityEngine.InputSystem;

namespace Mothropolis.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 8f;
        public float jumpForce = 14f;

        [Header("Grounding")]
        public Transform groundCheck;
        public float groundCheckRadius = 0.2f;
        public LayerMask groundLayer;

        [Header("Visuals")]
        public Transform spriteVisual; // Ensure this is a child of the Player, not the root!
        public Animator animator;

        private Rigidbody2D _rb;
        private bool _isGrounded;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (groundCheck == null)
            {
                var gc = transform.Find("GroundCheck");
                if (gc != null) groundCheck = gc;
            }
            if (spriteVisual == null)
            {
                var vis = transform.Find("Visuals");
                if (vis != null) spriteVisual = vis;
                else spriteVisual = transform;
            }
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
            if (groundLayer.value == 0)
            {
                groundLayer = LayerMask.GetMask("Ground");
            }
        }

        private void Update()
        {
            // Using the new InputSystem directly for rapid jam development
            float horizontalInput = 0f;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed) horizontalInput = -1f;
                if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed) horizontalInput = 1f;
            }

            _isGrounded = false;
            if (groundCheck != null)
            {
                _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            }

            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && _isGrounded)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
                if (animator != null) animator.SetTrigger("Jump");
                Audio.AudioService.PlayJump();
            }

            // Apply horizontal movement directly
            _rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, _rb.linearVelocity.y);

            // ==========================================
            // VISUAL STATE PASS TO ANIMATOR
            // ==========================================
            if (spriteVisual != null)
            {
                bool isMoving = Mathf.Abs(horizontalInput) > 0.01f;
                
                if (animator != null)
                {
                    animator.SetBool("IsMoving", isMoving);
                    animator.SetBool("IsGrounded", _isGrounded);
                    animator.SetFloat("VelocityY", _rb.linearVelocity.y);
                }

                // Sprite Flipping
                if (horizontalInput != 0)
                {
                    Vector3 scale = spriteVisual.localScale;
                    scale.x = horizontalInput > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                    spriteVisual.localScale = scale;
                }
            }
        }
    }
}
