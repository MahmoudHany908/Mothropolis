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
        public float hopVisualFrequency = 15f; 
        public float hopVisualAmplitude = 0.25f;

        private Rigidbody2D _rb;
        private bool _isGrounded;
        private float _hopTime;
        private Vector3 _visualBaseline;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (spriteVisual != null)
            {
                _visualBaseline = spriteVisual.localPosition;
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
            }

            // Apply horizontal movement directly
            _rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, _rb.linearVelocity.y);

            // ==========================================
            // VISUAL HOPPING (Animator + Code Driven)
            // ==========================================
            if (spriteVisual != null)
            {
                bool isMoving = Mathf.Abs(horizontalInput) > 0.01f;
                
                if (isMoving && _isGrounded)
                {
                    _hopTime += Time.deltaTime * hopVisualFrequency;
                    // Abs(Sin) creates a bouncy arc (0 to 1) so the sprite never dips below the floor
                    float yOffset = Mathf.Abs(Mathf.Sin(_hopTime)) * hopVisualAmplitude;
                    spriteVisual.localPosition = _visualBaseline + new Vector3(0, yOffset, 0);
                }
                else
                {
                    _hopTime = 0f;
                    // Smoothly settle back to the baseline offset when stopping or jumping
                    spriteVisual.localPosition = Vector3.Lerp(spriteVisual.localPosition, _visualBaseline, Time.deltaTime * 20f);
                }
                
                // Pass parameters to the Animator for the real animation states
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
