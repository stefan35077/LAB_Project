using UnityEngine;

public class WinBlock : MonoBehaviour
{
    SceneManager sceneManager;
    void Start()
    {
        sceneManager = FindFirstObjectByType<SceneManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Collided with " + collision.name);

        if (collision.CompareTag("Player"))
        {
            sceneManager.LoadScene("MainMenu", SceneManager.GamePhase.MainMenu);
        }

    }
}
