using UnityEngine;

/// <summary>Xử lý các nút của Main Menu (được gắn vào Button.onClick bởi Editor builder).</summary>
public class MainMenuUI : MonoBehaviour
{
    public GameObject instructionsPanel;

    void Start()
    {
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
    }

    public void Play()             { GameAudio.Click(); SceneFader.Load("Gameplay"); }
    public void ShowInstructions() { GameAudio.Click(); instructionsPanel.SetActive(true); }
    public void HideInstructions() { instructionsPanel.SetActive(false); }
    public void Quit()             { GameAudio.Click(); SceneFader.Quit(); }
}
