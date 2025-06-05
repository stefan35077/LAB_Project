using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class ButtonEffect : MonoBehaviour
{
    public static ButtonEffect Instance { get; private set; }

    [Header("Animation Settings")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float clickScale = 0.9f;
    [SerializeField] private float animationSpeed = 10f;

    [Header("Sound Settings")]
    [SerializeField] private AudioClip clickSound;
    private AudioSource audioSource;

    private Button[] allButtons;
    private Vector3[] originalScales;


    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        RefreshButtons();
    }

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSource();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        FindAndSetupAllButtons();
    }

    private void SetupAudioSource()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void FindAndSetupAllButtons()
    {
        // Find all buttons in the scene using the updated method
        allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        originalScales = new Vector3[allButtons.Length];

        // Store original scales and add listeners
        for (int i = 0; i < allButtons.Length; i++)
        {
            Button button = allButtons[i];
            originalScales[i] = button.transform.localScale;

            // Add event triggers if they don't exist
            EventTrigger eventTrigger = button.gameObject.GetComponent<EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = button.gameObject.AddComponent<EventTrigger>();
            }

            // Add hover events
            AddEventTrigger(eventTrigger, EventTriggerType.PointerEnter,
                (data) => { StartCoroutine(ScaleButton(button.transform, hoverScale)); });

            AddEventTrigger(eventTrigger, EventTriggerType.PointerExit,
                (data) => { StartCoroutine(ScaleButton(button.transform, 1f)); });

            AddEventTrigger(eventTrigger, EventTriggerType.PointerDown,
                (data) => {
                    StartCoroutine(ScaleButton(button.transform, clickScale));
                    PlayClickSound();
                });

            AddEventTrigger(eventTrigger, EventTriggerType.PointerUp,
                (data) => { StartCoroutine(ScaleButton(button.transform, hoverScale)); });
        }
    }

    private void AddEventTrigger(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback.AddListener((data) => { action((BaseEventData)data); });
        trigger.triggers.Add(entry);
    }

    private IEnumerator ScaleButton(Transform buttonTransform, float targetScale)
    {
        Vector3 startScale = buttonTransform.localScale;
        Vector3 targetScaleVector = Vector3.one * targetScale;
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * animationSpeed;
            buttonTransform.localScale = Vector3.Lerp(startScale, targetScaleVector, elapsedTime);
            yield return null;
        }

        buttonTransform.localScale = targetScaleVector;
    }

    private void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    // Call this when loading a new scene to setup new buttons
    public void RefreshButtons()
    {
        FindAndSetupAllButtons();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}