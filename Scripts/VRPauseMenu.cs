using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class VRPauseMenu : MonoBehaviour
{
    [Header("Pause Menu")]
    public GameObject pauseMenuUI;

    private bool isPaused = false;

    private InputAction pauseAction;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        pauseAction = new InputAction(
            name: "Pause",
            type: InputActionType.Button,
            binding: "<XRController>{RightHand}/secondaryButton"
        );

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnEnable()
    {
        pauseAction.Enable();
    }

    private void OnDisable()
    {
        pauseAction.Disable();
    }

    private void Update()
    {
        if (pauseAction.WasPressedThisFrame())
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(isPaused);

        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void ResumeGame()
    {
        isPaused = false;

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        Time.timeScale = 1f;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isPaused = false;

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}