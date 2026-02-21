using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerRunner : MonoBehaviour
{
    [Header("Lane Movement")]
    [SerializeField] private float laneDistance = 2.5f;
    [SerializeField] private float laneSwitchSpeed = 14f;

    [Header("Run")]
    [SerializeField] private float forwardSpeed = 8f;
    [SerializeField] private float maxForwardSpeed = 18f;
    [SerializeField] private float acceleration = 0.03f;

    [Header("Jetpack")]
    [SerializeField] private float jetpackDuration = 4f;
    [SerializeField] private float jetpackChaserPushBack = 2f;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer playerRenderer;
    [SerializeField] private Color jetpackTint = new Color(1f, 0.93f, 0.5f);

    private Rigidbody2D body;
    private int laneIndex;
    private float jetpackTimer;
    private Color defaultTint;

    public bool IsFlying => jetpackTimer > 0f;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;

        if (playerRenderer == null)
        {
            playerRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (playerRenderer != null)
        {
            defaultTint = playerRenderer.color;
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            return;
        }

        HandleInput();

        forwardSpeed = Mathf.Min(maxForwardSpeed, forwardSpeed + acceleration * Time.deltaTime);

        if (jetpackTimer > 0f)
        {
            jetpackTimer -= Time.deltaTime;
            if (jetpackTimer <= 0f && playerRenderer != null)
            {
                playerRenderer.color = defaultTint;
            }
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            body.velocity = Vector2.zero;
            return;
        }

        float targetX = laneIndex * laneDistance;
        float nextX = Mathf.Lerp(body.position.x, targetX, laneSwitchSpeed * Time.fixedDeltaTime);
        float nextY = body.position.y + forwardSpeed * Time.fixedDeltaTime;

        body.MovePosition(new Vector2(nextX, nextY));
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            laneIndex = Mathf.Max(-1, laneIndex - 1);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            laneIndex = Mathf.Min(1, laneIndex + 1);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle") && !IsFlying)
        {
            GameManager.Instance?.TriggerGameOver();
            return;
        }

        if (other.CompareTag("Jetpack"))
        {
            ActivateJetpack();
            Destroy(other.gameObject);
        }
    }

    private void ActivateJetpack()
    {
        jetpackTimer = jetpackDuration;
        GameManager.Instance?.ForceChaserBack(jetpackChaserPushBack);

        if (playerRenderer != null)
        {
            playerRenderer.color = jetpackTint;
        }
    }
}
