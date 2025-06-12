using UnityEngine;

public class HeadTrigger : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        SceneManager.Instance.GameOverScreen();
    }
}
