using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class SceneManager : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private float fadeSpeed = 1f;
    public CanvasGroup fadeCanvasGroup;

    [SerializeField] private GameObject GameOverUI;
    [SerializeField] private GameObject GameWonUI;

    public enum GamePhase
    {
        MainMenu,
        TestLevel,
        Loading,
        Gameplay,
        Paused,
        GameOver,
        GameWon
    }

    private GamePhase currentPhase = GamePhase.MainMenu;
    public GamePhase CurrentPhase => currentPhase;

    public event Action<GamePhase> OnPhaseChanged;

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
        yield return StartCoroutine(FadeRoutine(1f));

        ChangePhase(GamePhase.Loading);

        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        ChangePhase(phase);

        if (ButtonEffect.Instance != null)
        {
            ButtonEffect.Instance.RefreshButtons();
        }

        yield return StartCoroutine(FadeRoutine(0f));

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

    public void GameOverScreen()
    {
        Debug.Log("Game Over! Transitioning to GameOver phase.");
        ChangePhase(GamePhase.Paused);
        GameOverUI.SetActive(true);
    }

    public void ResumeGame()
    {
        if (currentPhase == GamePhase.Paused)
        {
            Time.timeScale = 1f;
            ChangePhase(GamePhase.Gameplay);
            GameOverUI.SetActive(false);
        }
    }

    public void GameWonScreen()
    {
        Debug.Log("Game Won! Transitioning to GameWon phase.");
        ChangePhase(GamePhase.GameWon);
        GameWonUI.SetActive(true);
    }
}