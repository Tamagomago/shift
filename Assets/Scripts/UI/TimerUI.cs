using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimerUI : MonoBehaviour
{
    public Slider progressSlider;
    public TextMeshProUGUI countdownText;

    public void SetProgress(float timeLeft)
    {
        progressSlider.value = timeLeft;
        countdownText.text = $"{timeLeft:F2}";
    }
}
