using UnityEngine;
using UnityEngine.UI;

/// <summary>EndGame scene: hiển thị điểm và xử lý nút Main Menu / Play Again / Quit.</summary>
public class EndGameUI : MonoBehaviour
{
    public Text scoreText;
    public Text highScoreText;

    void Start()
    {
        scoreText.text = "YOUR SCORE: " + GameManager.FinalScore;
        highScoreText.text = "HIGH SCORE: " + GameManager.HighScore;
    }

    public void MainMenu()  { GameAudio.Click(); SceneFader.Load("MainMenu"); }
    public void PlayAgain() { SceneFader.Load("Gameplay"); }
    public void Quit()      { GameAudio.Click(); SceneFader.Quit(); }
}
