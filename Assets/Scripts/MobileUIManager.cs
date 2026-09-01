using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lightweight mobile HUD: live distance score and Game Over restart panel.
/// </summary>
public class MobileUIManager : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private string scoreFormat = "{0:0} m";

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private Button tryAgainButton;

    private GameManager gameManager;

    private void Awake()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (tryAgainButton != null)
        {
            tryAgainButton.onClick.AddListener(OnTryAgainClicked);
        }
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogWarning("MobileUIManager: GameManager not found in scene.");
            return;
        }

        gameManager.OnScoreChanged += HandleScoreChanged;
        gameManager.OnGameOver += HandleGameOver;
        HandleScoreChanged(gameManager.Score);
    }

    private void OnDestroy()
    {
        if (gameManager == null)
        {
            return;
        }

        gameManager.OnScoreChanged -= HandleScoreChanged;
        gameManager.OnGameOver -= HandleGameOver;

        if (tryAgainButton != null)
        {
            tryAgainButton.onClick.RemoveListener(OnTryAgainClicked);
        }
    }

    private void HandleScoreChanged(float score)
    {
        if (scoreText != null)
        {
            scoreText.text = string.Format(scoreFormat, score);
        }
    }

    private void HandleGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalScoreText != null && gameManager != null)
        {
            finalScoreText.text = string.Format(scoreFormat, gameManager.Score);
        }
    }

    private void OnTryAgainClicked()
    {
        if (gameManager != null)
        {
            gameManager.RestartScene();
        }
    }
}
