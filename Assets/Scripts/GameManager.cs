using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Playing,
    GameOver
}

/// <summary>
/// Central game state and score authority. Attach to a persistent scene object.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Transform scoreReference;

    public GameState CurrentState { get; private set; } = GameState.Playing;
    public float Score { get; private set; }

    public event Action OnGameOver;
    public event Action<float> OnScoreChanged;

    private float startZ;

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
        if (scoreReference == null)
        {
            var player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                scoreReference = player.transform;
            }
        }

        startZ = scoreReference != null ? scoreReference.position.z : 0f;
    }

    private void Update()
    {
        if (CurrentState != GameState.Playing || scoreReference == null)
        {
            return;
        }

        float distance = Mathf.Max(0f, scoreReference.position.z - startZ);
        if (!Mathf.Approximately(distance, Score))
        {
            Score = distance;
            OnScoreChanged?.Invoke(Score);
        }
    }

    public void SetScoreReference(Transform reference)
    {
        scoreReference = reference;
        startZ = reference != null ? reference.position.z : 0f;
        Score = 0f;
    }

    public void TriggerGameOver()
    {
        if (CurrentState == GameState.GameOver)
        {
            return;
        }

        CurrentState = GameState.GameOver;
        OnGameOver?.Invoke();
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
