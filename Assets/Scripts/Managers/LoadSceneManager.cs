using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

public class LoadSceneManager : MonoBehaviour
{
    public static LoadSceneManager Instance { get; private set; }

    public Animator transition;

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
    }

    private void Start()
    {
        ActivateMenu();
    }

    public void ActivateMenu()
    {
        StartCoroutine(_ActivateScene(0));
    }

    public void ActivateGame()
    {
        StartCoroutine(_ActivateScene(1));
    }

    private IEnumerator _ActivateScene(int n)
    {
        transition.SetTrigger("start");
        yield return new WaitForSeconds(0.5f);
        yield return SceneManager.LoadSceneAsync(n);
        yield return new WaitForSeconds(0.5f);
        transition.SetTrigger("end");
    }

    public void GameOver()
    {
        GUIManager.Instance.HideCursor(false);
        GUIManager.Instance.SetPage("gameover");
        FindFirstObjectByType<PlayerInput>().enabled = false;
        FindFirstObjectByType<AgentAI>().enabled = false;
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
    }
}
