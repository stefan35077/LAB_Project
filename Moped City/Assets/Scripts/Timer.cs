using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class Timer : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float startingMinutes = 5f;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private bool startOnAwake = true;

    [Header("Events")]
    public UnityEvent onTimerEnd;

    private float currentTime;
    private bool isRunning;

    private void Awake()
    {
        // Convert minutes to seconds
        currentTime = startingMinutes * 60f;

        if (startOnAwake)
        {
            StartTimer();
        }
    }

    private void Update()
    {
        if (!isRunning) return;

        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerDisplay();
        }
        else if (currentTime <= 0)
        {
            TimerComplete();
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        float minutes = Mathf.FloorToInt(currentTime / 60);
        float seconds = Mathf.FloorToInt(currentTime % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void TimerComplete()
    {
        isRunning = false;
        currentTime = 0;
        UpdateTimerDisplay();
        onTimerEnd?.Invoke();
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void PauseTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        currentTime = startingMinutes * 60f;
        UpdateTimerDisplay();
    }

    public void ResetAndStartTimer()
    {
        ResetTimer();
        StartTimer();
    }

    // Get remaining time in minutes (for other scripts to check)
    public float GetRemainingMinutes()
    {
        return currentTime / 60f;
    }

    // Get remaining time in seconds
    public float GetRemainingSeconds()
    {
        return currentTime;
    }
}