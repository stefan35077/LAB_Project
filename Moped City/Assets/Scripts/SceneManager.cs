using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance { get; private set; }

    [Header("Transition Settings")]
    [SerializeField] private float fadeSpeed = 1f;
    public CanvasGroup fadeCanvasGroup;

    public enum GamePhase
    {
        MainMenu,
        TestLevel,
        Loading,
        Gameplay,
        Paused,
        GameOver
    }

    private GamePhase currentPhase = GamePhase.MainMenu;
    public GamePhase CurrentPhase => currentPhase;

    public event Action<GamePhase> OnPhaseChanged;

    private void Awake()
    {
        SceneManager.DontDestroyOnLoad(this);

        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ChangePhase(GamePhase newPhase)
    {
        currentPhase = newPhase;
        OnPhaseChanged?.Invoke(newPhase);
    }

    public void LoadScene(string sceneName, GamePhase phase = GamePhase.Gameplay)
    {
        StartCoroutine(LoadSceneRoutine(sceneName, phase));
    }

    private IEnumerator LoadSceneRoutine(string sceneName, GamePhase phase)
    {
        // Start fade out
        yield return StartCoroutine(FadeRoutine(1f));

        // Change to loading phase
        ChangePhase(GamePhase.Loading);

        // Load the new scene
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Change to specified phase
        ChangePhase(phase);

        // Ensure ButtonEffect refreshes after scene is fully loaded
        if (ButtonEffect.Instance != null)
        {
            ButtonEffect.Instance.RefreshButtons();
        }

        // Fade back in
        yield return StartCoroutine(FadeRoutine(0f));

        // Refresh buttons again after fade to ensure everything is properly set up
        if (ButtonEffect.Instance != null)
        {
            ButtonEffect.Instance.RefreshButtons();
        }
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        if (fadeCanvasGroup == null) yield break;

        float startAlpha = fadeCanvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < fadeSpeed)
        {
            elapsedTime += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeSpeed);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }

    public void RestartCurrentScene()
    {
        LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void LoadTestLevel()
    {
        LoadScene("TestLevel", GamePhase.TestLevel);
    }

    public void LoadMainMenu()
    {
        LoadScene("MainMenu", GamePhase.MainMenu);
    }

    public void PauseGame()
    {
        if (currentPhase == GamePhase.Gameplay)
        {
            Time.timeScale = 0f;
            ChangePhase(GamePhase.Paused);
        }
    }

    public void ResumeGame()
    {
        if (currentPhase == GamePhase.Paused)
        {
            Time.timeScale = 1f;
            ChangePhase(GamePhase.Gameplay);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}