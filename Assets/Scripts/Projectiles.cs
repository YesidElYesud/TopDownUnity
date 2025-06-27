using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
public class Projectiles : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] public float damage = 1f;
    [SerializeField] private LayerMask collisionLayer;

    private Vector2 direction;
    private Rigidbody2D rb;
    private Animator animator;
    private bool hasCollided = false;

    // Llamado desde quien dispara
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        // SetDirection(Vector2.right); // Dirección por defecto
    }

    public void FixedUpdate()
    {
        if (direction != Vector2.zero)
        {
            // Mover el proyectil
            rb.velocity = direction * speed;

            // // Actualizar la rotación del proyectil
            // float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            // rb.rotation = angle;
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacles"))
        {
            animator.SetTrigger("Collide");
            return;
        }
        // Verificar si el objeto con el que colisiona es del layer de colisión
        if ((collisionLayer & (1 << other.gameObject.layer)) == 0 || hasCollided)
            return;
        if (!hasCollided)
        {
            hasCollided = true;
            // Detener el movimiento
            rb.velocity = Vector2.zero;
            speed = 0f;
            // Activar animación de colisión
            animator.SetTrigger("Collide");
            // Opcional: Desactivar collider para evitar múltiples colisiones
            GetComponent<Collider2D>().enabled = false;
            
            if (other.TryGetComponent(out IDamageable damageable))
            {
                // Crear un objeto DamageInfo para pasar información del daño
                DamageInfo damageInfo = new DamageInfo(damage, gameObject);
                // Aplicar daño al objeto colisionado
                damageable.TakeDamage(damageInfo);
            }
        }
    }

    // Este método debe llamarse al final de la animación "Collide"
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
