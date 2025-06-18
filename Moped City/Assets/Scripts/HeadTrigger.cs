using UnityEngine;

public class HeadTrigger : MonoBehaviour
{
    SceneManager sceneManager;

    private void Start()
    {
        sceneManager = FindFirstObjectByType<SceneManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Collided with " + collision.name);
        sceneManager.GameOverScreen();
    }
}
