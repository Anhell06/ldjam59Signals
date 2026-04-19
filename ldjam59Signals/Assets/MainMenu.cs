using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void StartGame()
    {
        SceneManager.LoadScene("Init");
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
    }


    public void Exit()
    {
        Application.Quit();
    }
}
