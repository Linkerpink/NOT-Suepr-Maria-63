using UnityEngine;
using UnityEngine.SceneManagement;

public class MarioHead : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("Peach's Castle Outside Front");
    }
}
