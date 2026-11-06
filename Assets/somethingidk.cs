using UnityEngine;
using UnityEngine.SceneManagement;

public class somethingidk : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentSceneIndex);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            Application.Quit();
        }
    }
}
