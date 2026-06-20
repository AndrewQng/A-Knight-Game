using UnityEngine;
using UnityEngine.InputSystem; // Sử dụng New Input System
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 6.0f;
    [SerializeField] private float jumpForce = 8.0f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float checkRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float jumpCooldown = 0.15f; // Thời gian bỏ qua kiểm tra đất ngay sau khi nhảy

    [Header("Combat Settings")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private float attackCooldown = 0.25f; // Khoảng thời gian tối thiểu giữa các đòn đánh
    [SerializeField] private bool limitMovementDuringAttack = true; // Khóa di chuyển khi đang chém
    [SerializeField] private GameObject rangeIndicator; // GameObject vòng tròn mờ hiển thị tầm đánh

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded;
    private float horizontalInput;
    private float jumpTimer; // Bộ đếm thời gian bỏ qua check đất

    // Các biến phục vụ combo tấn công
    private int currentAttack = 0;
    private float timeSinceAttack = 0.0f;
    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Tự động ẩn vòng tròn tầm chém khi bắt đầu game
        if (rangeIndicator != null)
        {
            rangeIndicator.SetActive(false);
        }
    }

    void Update()
    {
        // Tăng bộ đếm thời gian từ đòn đánh cuối cùng
        timeSinceAttack += Time.deltaTime;

        // Giảm bộ đếm thời gian nhảy
        if (jumpTimer > 0)
        {
            jumpTimer -= Time.deltaTime;
        }

        // 1. Nhận Input di chuyển
        horizontalInput = 0f;

        // Chỉ cho phép lấy Input di chuyển nếu không bị khóa bởi đang tấn công
        if (!isAttacking || !limitMovementDuringAttack)
        {
            // Đọc từ bàn phím
            if (Keyboard.current != null)
            {
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                    horizontalInput = 1f;
                else if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                    horizontalInput = -1f;
            }

            // Đọc từ Gamepad (nếu có kết nối)
            if (Gamepad.current != null)
            {
                float stickX = Gamepad.current.leftStick.x.ReadValue();
                if (Mathf.Abs(stickX) > 0.1f)
                    horizontalInput = stickX;
            }
        }

        // 2. Quay mặt nhân vật (Chỉ quay mặt khi không tấn công)
        // Sử dụng localScale.x thay vì flipX để tự động xoay toàn bộ GameObject con (như AttackPoint, GroundCheck) đi theo hướng mặt
        if (!isAttacking)
        {
            if (horizontalInput > 0.01f)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (horizontalInput < -0.01f)
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }

        // 3. Kiểm tra chạm đất
        if (jumpTimer > 0)
        {
            isGrounded = false;
        }
        else
        {
            isGrounded = false;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheckPoint.position, checkRadius, groundLayer);
            foreach (var col in colliders)
            {
                if (col.gameObject != gameObject && !col.isTrigger)
                {
                    isGrounded = true;
                    break;
                }
            }
        }

        // 4. Nhấn phím nhảy
        bool jumpPressed = false;
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            jumpPressed = true;
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
            jumpPressed = true;

        if (jumpPressed && isGrounded && !isAttacking)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetTrigger("Jump");
            isGrounded = false;
            jumpTimer = jumpCooldown;
        }

        // 5. Tấn công (Combo 3 bước)
        bool attackPressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame) // Nút X trên Xbox / Vuông trên PS
            attackPressed = true;

        if (attackPressed && timeSinceAttack > attackCooldown && isGrounded)
        {
            StartCoroutine(PerformAttackCombo());
        }

        // 6. Cập nhật Animation di chuyển
        anim.SetBool("Grounded", isGrounded);
        anim.SetFloat("AirSpeedY", rb.linearVelocity.y);

        if (Mathf.Abs(horizontalInput) > 0.01f)
            anim.SetInteger("AnimState", 1); // Trạng thái chạy
        else
            anim.SetInteger("AnimState", 0); // Trạng thái đứng yên
    }

    void FixedUpdate()
    {
        // 7. Di chuyển vật lý
        if (isAttacking && limitMovementDuringAttack)
        {
            // Đứng yên khi chém để tạo cảm giác chém đầm tay
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);
        }
    }

    // Coroutine xử lý combo chém liên hoàn và khóa di chuyển tạm thời
    private IEnumerator PerformAttackCombo()
    {
        isAttacking = true;
        currentAttack++;

        // Bật vòng tròn hiển thị tầm đánh khi bắt đầu chém
        if (rangeIndicator != null) rangeIndicator.SetActive(true);

        // Nếu quá lâu chưa chém tiếp (> 1 giây), đặt lại combo về đòn 1
        if (timeSinceAttack > 1.0f)
            currentAttack = 1;

        // Giới hạn combo tối đa 3 đòn
        if (currentAttack > 3)
            currentAttack = 1;

        // Kích hoạt hoạt ảnh tấn công tương ứng
        anim.SetTrigger("Attack" + currentAttack);

        // Reset thời gian chờ tấn công
        timeSinceAttack = 0.0f;

        // Tắt vòng tròn chỉ thị tầm chém sau 0.15 giây vung kiếm
        yield return new WaitForSeconds(0.15f);
        if (rangeIndicator != null) rangeIndicator.SetActive(false);

        // Chờ nốt thời gian còn lại của đòn chém
        yield return new WaitForSeconds(0.20f);

        isAttacking = false;
    }

    // Hàm được gọi bởi Animation Event trong các Clip Attack của Unity để kiểm tra va chạm gây sát thương
    // Bạn cần mở các clip Attack1, Attack2, Attack3 và add Animation Event gọi hàm này
    public void AE_AttackHit()
    {
        Debug.Log("AE_AttackHit() đã được gọi từ Animation Event!");

        if (attackPoint == null)
        {
            Debug.LogWarning("attackPoint (Điểm tấn công) chưa được gán trong Inspector!");
            return;
        }

        // Quét tất cả kẻ địch trong vùng chém
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        
        // Đảm bảo mỗi quái vật chỉ nhận sát thương 1 lần duy nhất trong 1 cú chém
        // (Tránh trường hợp quái vật có nhiều Collider bị quét trúng nhiều lần dẫn đến lặp/giật hoạt ảnh Hurt)
        System.Collections.Generic.List<Enemy> damagedEnemies = new System.Collections.Generic.List<Enemy>();

        foreach (Collider2D enemy in hitEnemies)
        {
            Enemy enemyScript = enemy.GetComponentInParent<Enemy>();
            if (enemyScript != null && !damagedEnemies.Contains(enemyScript))
            {
                damagedEnemies.Add(enemyScript);
                Debug.Log("Hiệp sĩ chém trúng: " + enemyScript.name);
                enemyScript.TakeDamage(attackDamage);
            }
        }
    }

    // Vẽ vòng tròn hiển thị liên tục trong cửa sổ Scene (không cần chọn Player)
    private void OnDrawGizmos()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, checkRadius);
        }

        if (attackPoint != null)
        {
            // Sử dụng màu hồng tím rực để dễ phân biệt vùng tấn công
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
            
            // Vẽ một tâm điểm nhỏ màu vàng tại chính giữa AttackPoint
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(attackPoint.position, 0.05f);
        }
    }
}
