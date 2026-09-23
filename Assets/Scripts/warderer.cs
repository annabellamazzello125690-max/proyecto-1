using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class WandererAI : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float minWalkTime = 2f;
    [SerializeField] private float maxWalkTime = 4f;

    [Header("Pausas / Idle")]
    [SerializeField] private float minIdleTime = 1f;
    [SerializeField] private float maxIdleTime = 3f;

    [Header("Opciones")]
    [SerializeField] private bool rotateTowardsDirection = false;

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private float stateTimer;
    private bool isWalking;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ChooseNextState();
    }

    void Update()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            ChooseNextState();
        }

        if (rotateTowardsDirection && isWalking && moveDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            rb.rotation = Mathf.LerpAngle(rb.rotation, angle, Time.deltaTime * 10f);
        }
    }

    void FixedUpdate()
    {
        if (isWalking)
        {
            // Usamos .velocity para máxima compatibilidad con versiones previas de Unity
            rb.linearVelocity = moveDirection * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void ChooseNextState()
    {
        // Alterna aleatoriamente entre caminar (50% probabilidad) o detenerse
        isWalking = Random.value > 0.5f;

        if (isWalking)
        {
            moveDirection = Random.insideUnitCircle.normalized;
            stateTimer = Random.Range(minWalkTime, maxWalkTime);
        }
        else
        {
            moveDirection = Vector2.zero;
            stateTimer = Random.Range(minIdleTime, maxIdleTime);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Si choca contra una pared, elige una nueva dirección/estado de inmediato
        ChooseNextState();
    }
}