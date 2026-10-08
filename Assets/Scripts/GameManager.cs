using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quản lý điểm số và trạng thái game trong Gameplay scene.
/// Điểm cuối cùng được lưu vào biến static để EndGame scene đọc được.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    /// <summary>Điểm của ván vừa chơi (đọc bởi EndGame scene).</summary>
    public static int FinalScore;
    /// <summary>Điểm cao nhất, lưu bằng PlayerPrefs.</summary>
    public static int HighScore => PlayerPrefs.GetInt("HighScore", 0);

    [Header("Scoring")]
    public int starPoints = 10;        // cộng khi nhặt sao
    public int asteroidHitPenalty = 10; // trừ khi tàu đâm thiên thạch
    public int asteroidShotPoints = 5;  // cộng khi bắn nổ thiên thạch

    [Header("UI")]
    public Text scoreText;

    [Tooltip("Thời gian chờ (giây) từ lúc nổ tàu đến khi chuyển sang EndGame")]
    public float gameOverDelay = 1.5f;

    public int Score { get; private set; }
    public bool IsGameOver { get; private set; }

    void Awake()
    {
        Instance = this;
        Score = 0;
        IsGameOver = false;
        UpdateUI();
        if (GetComponent<PauseMenuUI>() == null) gameObject.AddComponent<PauseMenuUI>();
    }

    public void AddScore(int amount)
    {
        if (IsGameOver) return;
        Score = Mathf.Max(0, Score + amount);
        UpdateUI();
    }

    /// <summary>Kết thúc game: lưu điểm, chờ một chút rồi sang EndGame scene.</summary>
    public void GameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        FinalScore = Score;
        if (Score > HighScore)
        {
            PlayerPrefs.SetInt("HighScore", Score);
            PlayerPrefs.Save();
        }
        StartCoroutine(GoToEndScene());
    }

    IEnumerator GoToEndScene()
    {
        yield return new WaitForSeconds(gameOverDelay);
        SceneFader.Load("EndGame");
    }

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "SCORE: " + Score;
    }
}
