using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scene References")]
    [SerializeField] private PlayerRunner player;
    [SerializeField] private Transform chaser;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Scoring")]
    [SerializeField] private float pointsPerSecond = 10f;

    [Header("Chaser")]
    [SerializeField] private float chaserCatchDistance = 1.5f;
    [SerializeField] private float defaultChaserOffset = 4f;

    public float Score { get; private set; }
    public bool IsGameOver { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        gameOverPanel?.SetActive(false);
        Score = 0f;
        RefreshScore();
    }

    private void Update()
    {
        if (IsGameOver)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Restart();
            }

            return;
        }

        Score += pointsPerSecond * Time.deltaTime;
        RefreshScore();

        if (chaser == null || player == null)
        {
            return;
        }

        var targetPosition = new Vector3(
            Mathf.Lerp(chaser.position.x, player.transform.position.x, Time.deltaTime * 3f),
            player.transform.position.y - defaultChaserOffset,
            chaser.position.z);

        chaser.position = Vector3.Lerp(chaser.position, targetPosition, Time.deltaTime * 4f);

        if (Vector2.Distance(chaser.position, player.transform.position) <= chaserCatchDistance)
        {
            TriggerGameOver();
        }
    }

    public void TriggerGameOver()
    {
        if (IsGameOver)
        {
            return;
        }

        IsGameOver = true;
        gameOverPanel?.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ForceChaserBack(float distance)
    {
        if (chaser == null)
        {
            return;
        }

        chaser.position += Vector3.down * distance;
    }

    private void RefreshScore()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {Mathf.FloorToInt(Score)}";
        }
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
