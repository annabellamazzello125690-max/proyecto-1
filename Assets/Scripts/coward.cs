using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CowardAI : MonoBehaviour
{
    [Header("Velocidad")]
    [SerializeField] private float fleeSpeed = 4f;    // Más rápido al huir
    [SerializeField] private float wanderSpeed = 2f;  // Normal al patrullar

    [Header("Rangos")]
    [SerializeField] private float fleeRange = 5f;    // Distancia a la que se asusta
    [SerializeField] private float safeDistance = 7f;  // Distancia para calmarse y volver a patrullar

    [Header("Evitación de Paredes")]
    [SerializeField] private LayerMask obstacleMask; // Capa de paredes/obstáculos
    [SerializeField] private float rayDistance = 1.2f;

    [Header("Referencias")]
    [SerializeField] private Transform playerTransform;

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private float wanderTimer;
    private bool isFleeing;

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

        // Huye si el jugador entra en rango, o sigue huyendo hasta alcanzar la distancia segura
        if (distanceToPlayer <= fleeRange || (isFleeing && distanceToPlayer < safeDistance))
        {
            isFleeing = true;
            CalculateFleeDirection();
        }
        else
        {
            isFleeing = false;

            wanderTimer -= Time.deltaTime;
            if (wanderTimer <= 0f)
            {
                GetNewWanderDirection();
            }
        }
    }

    void FixedUpdate()
    {
        float currentSpeed = isFleeing ? fleeSpeed : wanderSpeed;

        // Compatible con Unity 6 (linearVelocity) y versiones anteriores (velocity)
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = moveDirection * currentSpeed;
#else
            rb.velocity = moveDirection * currentSpeed;
#endif
    }

    void CalculateFleeDirection()
    {
        // Dirección opuesta al jugador
        Vector2 rawFleeDir = ((Vector2)transform.position - (Vector2)playerTransform.position).normalized;

        // Lanza un Raycast para comprobar si hay una pared en la dirección de huida
        RaycastHit2D hit = Physics2D.Raycast(transform.position, rawFleeDir, rayDistance, obstacleMask);

        if (hit.collider != null)
        {
            // Si hay una pared enfrente, desvía la trayectoria usando la normal del impacto
            moveDirection = Vector2.Reflect(rawFleeDir, hit.normal).normalized;
        }
        else
        {
            moveDirection = rawFleeDir;
        }
    }

    void GetNewWanderDirection()
    {
        moveDirection = Random.insideUnitCircle.normalized;
        wanderTimer = Random.Range(2f, 4f);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Si choca contra un objeto mientras patrulla, cambia de dirección
        if (!isFleeing)
        {
            GetNewWanderDirection();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Rango en el que detecta al jugador y empieza a huir (Rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fleeRange);

        // Distancia a la que se siente seguro y vuelve a patrullar (Verde)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, safeDistance);
    }
}