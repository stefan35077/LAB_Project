// Add this to a new UI GameObject in your scene:
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class FadePanel : MonoBehaviour
{
    private void Awake()
    {
        var sceneManager = Object.FindFirstObjectByType<SceneManager>();
        if (sceneManager != null)
        {
            sceneManager.fadeCanvasGroup = GetComponent<CanvasGroup>();
        }
    }
}