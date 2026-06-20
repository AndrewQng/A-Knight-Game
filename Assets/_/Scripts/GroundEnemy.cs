using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour
{
    [Header("Health & Combat Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private string hurtTriggerName = "Hurt";
    [SerializeField] private string deathTriggerName = "Death";
    [SerializeField] private float hurtStunDuration = 0.4f; // Thời gian quái bị khựng (đứng yên) khi trúng đòn

    [Header("Patrol Settings (Tuần tra)")]
    [SerializeField] private bool canPatrol = true;
    [SerializeField] private float patrolSpeed = 2.0f;
    [SerializeField] private float patrolDistance = 3.0f; // Khoảng cách tuần tra sang trái/phải từ vị trí bắt đầu
    [SerializeField] private string moveParameterName = "AnimState"; // Tên tham số di chuyển trong Animator

    private int currentHealth;
    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isDead = false;
    private bool isHurt = false; // Trạng thái đang bị khựng do trúng đòn

    // Các biến giới hạn di chuyển tuần tra
    private float leftLimit;
    private float rightLimit;
    private bool movingRight = true;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Xác định giới hạn di chuyển dựa trên vị trí bắt đầu của quái vật trên Scene
        leftLimit = transform.position.x - patrolDistance;
        rightLimit = transform.position.x + patrolDistance;
    }

    void Update()
    {
        if (isDead) return;

        // Nếu đang bị khựng (Hurt Stun), đứng yên và không chạy logic tuần tra
        if (isHurt)
        {
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            return;
        }

        // Thực hiện di chuyển tuần tra
        if (canPatrol)
        {
            Patrol();
        }
        else
        {
            // Nếu không tuần tra, đứng yên (Idle)
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            if (anim != null)
            {
                anim.SetInteger(moveParameterName, 0); // Trạng thái đứng yên
            }
        }
    }

    private void Patrol()
    {
        if (movingRight)
        {
            // Di chuyển sang phải
            rb.linearVelocity = new Vector2(patrolSpeed, rb.linearVelocity.y);
            
            // Xoay mặt Sprite sang phải
            if (spriteRenderer != null) spriteRenderer.flipX = false;

            // Nếu vượt quá giới hạn bên phải, quay đầu sang trái
            if (transform.position.x >= rightLimit)
            {
                movingRight = false;
            }
        }
        else
        {
            // Di chuyển sang trái
            rb.linearVelocity = new Vector2(-patrolSpeed, rb.linearVelocity.y);
            
            // Xoay mặt Sprite sang trái
            if (spriteRenderer != null) spriteRenderer.flipX = true;

            // Nếu vượt quá giới hạn bên trái, quay đầu sang phải
            if (transform.position.x <= leftLimit)
            {
                movingRight = true;
            }
        }

        // Kích hoạt hoạt ảnh di chuyển (AnimState = 1)
        if (anim != null)
        {
            anim.SetInteger(moveParameterName, 1); // Trạng thái chạy/đi bộ
        }
    }

    // Hàm nhận sát thương được gọi từ Player
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log(gameObject.name + " nhận " + damage + " sát thương. Máu còn lại: " + currentHealth);

        // Kích hoạt trạng thái khựng đứng yên
        StartCoroutine(HandleHurtStun());

        // Phát hoạt ảnh bị thương
        if (anim != null)
        {
            anim.SetTrigger(hurtTriggerName);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Coroutine xử lý việc khựng di chuyển khi bị đánh trúng
    private IEnumerator HandleHurtStun()
    {
        isHurt = true;

        // Đứng yên lập tức bằng cách triệt tiêu vận tốc ngang
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        // Đặt trạng thái hoạt ảnh di chuyển về đứng yên (Idle)
        if (anim != null)
        {
            anim.SetInteger(moveParameterName, 0);
        }

        // Chờ hết thời gian khựng
        yield return new WaitForSeconds(hurtStunDuration);

        isHurt = false;
    }

    private void Die()
    {
        isDead = true;
        Debug.Log(gameObject.name + " đã bị tiêu diệt!");

        // Ngừng hoàn toàn mọi Coroutine đang chạy (như khựng)
        StopAllCoroutines();

        // Phát hoạt ảnh chết
        if (anim != null)
        {
            anim.SetTrigger(deathTriggerName);
        }

        // Tắt va chạm (Collider) lập tức để người chơi đi xuyên qua xác quái
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Đổi Rigidbody sang Static để giữ xác đứng im trên không trung/nền đất không bị rơi xuống vực thẳm
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        // Hủy quái sau 2 giây để chạy xong hoạt ảnh chết
        Destroy(gameObject, 2.0f);
    }

    // Vẽ giới hạn tuần tra trên Scene view để dễ quan sát
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 startPosition = Application.isPlaying ? new Vector3((leftLimit + rightLimit) / 2f, transform.position.y, transform.position.z) : transform.position;
        
        Gizmos.DrawLine(startPosition - Vector3.right * patrolDistance, startPosition + Vector3.right * patrolDistance);
        Gizmos.DrawWireSphere(startPosition - Vector3.right * patrolDistance, 0.1f);
        Gizmos.DrawWireSphere(startPosition + Vector3.right * patrolDistance, 0.1f);
    }
}
