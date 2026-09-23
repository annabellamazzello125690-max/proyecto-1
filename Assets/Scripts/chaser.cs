using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ChaserAI : MonoBehaviour
{
    [Header("Movimiento y Persecución")]
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float wanderSpeed = 2f;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float stopDistance = 0.5f; // Distancia para no encimarse al jugador

    [Header("Referencias")]
    [SerializeField] private Transform playerTransform;

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private float wanderTimer;
    private bool isChasing;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        GetNewWanderDirection();
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRange)
        {
            isChasing = true;

            // Si está muy cerca del jugador, deja de avanzar para no empujarlo/atravesarlo
            if (distanceToPlayer > stopDistance)
            {
                moveDirection = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            }
            else
            {
                moveDirection = Vector2.zero;
            }
        }
        else
        {
            isChasing = false;

            wanderTimer -= Time.deltaTime;
            if (wanderTimer <= 0f)
            {
                GetNewWanderDirection();
            }
        }
    }

    void FixedUpdate()
    {
        float currentSpeed = isChasing ? chaseSpeed : wanderSpeed;

        // Soporta Unity 6 (linearVelocity) y versiones anteriores (velocity)
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = moveDirection * currentSpeed;
#else
            rb.velocity = moveDirection * currentSpeed;
#endif
    }

    void GetNewWanderDirection()
    {
        moveDirection = Random.insideUnitCircle.normalized;
        wanderTimer = Random.Range(2f, 4f);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Si choca contra una pared mientras deambula, cambia de dirección
        if (!isChasing)
        {
            GetNewWanderDirection();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Dibuja el rango de detección en la vista de Escena
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Dibuja el rango de parada en rojo
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}