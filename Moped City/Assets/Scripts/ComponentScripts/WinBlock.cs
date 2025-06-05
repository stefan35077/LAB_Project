using UnityEngine;

public class WinBlock : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Collided with " + collision.name);

        if (collision.CompareTag("Player"))
        {
            SceneManager.Instance.LoadScene("MainMenu", SceneManager.GamePhase.MainMenu);
        }

    }
}
