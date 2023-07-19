using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.Feedbacks;

// This script is used to manage the game's state such as the player's health, the game over state,
// and the game's soundtracks. It also contains a singleton that can be accessed from any script.
public class GameManager : MonoBehaviour
{
    public GameObject _gameOverUI; // Game Over UI object
    public static GameManager gameManager { get; private set; } // Singleton
    public MMF_Player _gameFeedbacks; // Game Feedbacks
    public int _playerHealthMax = 5; // Player's max health
    public UnitHealth _playerHealth; // Player's health component
    public bool _isGameOver; // Is the game over?
    public bool _receivePlayerInput = true; // Can the player receive input?
    public bool _receivePlayerMovementInput = false; // Can the player receive movement input?
    public bool combatMode = false; // Is the player in combat? Used for soundrack management
    public AudioSource explorationTrack; // Exploration soundtrack
    public AudioSource combatTrack; // Combat soundtrack
    public float fadeTime = 1.0f; // Time it takes to fade in/out the soundtracks
    private bool _inCombat = false; // Is the player in combat? Used for soundrack management

    private List<ObjectPool> _pools = new List<ObjectPool>(); // List of all the pools in the scene

    // Awake is called before first frame update
    void Awake()
    {
        _playerHealth = new UnitHealth(_playerHealthMax, _playerHealthMax); // Initialize player's health

        // Singleton
        /* This code is implementing a singleton pattern for the GameManager class. It checks if there
        is already an instance of the GameManager in the scene. If there is, it destroys the current
        instance. If there isn't, it sets the current instance as the gameManager and initializes
        the _isGameOver variable to false. This ensures that there is only one instance of the
        GameManager in the scene at any given time. */
        if (gameManager != null && gameManager != this) { Destroy(this.gameObject); } 
        else
        {
            gameManager = this;
            _isGameOver = false;
        }
        explorationTrack.Play(); // Play exploration soundtrack on start
    }

    /// <summary>
    /// This function fades out an audio source over a specified amount of time and then stops it.
    /// </summary>
    /// <param name="AudioSource">>The AudioSource is a component in Unity that is used to play audio
    /// clips. It is attached to a GameObject and can be used to play sound effects, music, and other
    /// audio assets in a game.</param>
    /// <param name="FadeTime">The duration of the fade out effect in seconds.</param>
    private IEnumerator FadeOut(AudioSource audioSource, float FadeTime)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;

            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }

    /// <summary>
    /// This function fades in an audio source over a specified amount of time.
    /// </summary>
    /// <param name="AudioSource">The AudioSource is a component in Unity that is used to play audio
    /// clips. It is attached to a GameObject and can be used to play sound effects, music, and other
    /// audio assets in a game.</param>
    /// <param name="FadeTime">The duration of the fade-in effect in seconds.</param>
    private IEnumerator FadeIn(AudioSource audioSource, float FadeTime)
    {
        audioSource.Play();
        float startVolume = audioSource.volume;
        audioSource.volume = 0.0f;

        while (audioSource.volume < startVolume)
        {
            audioSource.volume += Time.deltaTime / FadeTime;

            yield return null;
        }
    }

    /// <summary>
    /// This function updates the game state by checking if the game is in combat mode, if the game is
    /// over, and fades in/out the appropriate audio tracks.
    /// </summary>
    void Update() {
        if (combatMode && !_inCombat && !combatTrack.isPlaying) {
            _inCombat = true;
            StartCoroutine(FadeOut(explorationTrack, fadeTime));
            StartCoroutine(FadeIn(combatTrack, fadeTime));
        }
        if (!combatMode && _inCombat && !explorationTrack.isPlaying) {
            _inCombat = false;
            StartCoroutine(FadeOut(combatTrack, fadeTime));
            StartCoroutine(FadeIn(explorationTrack, fadeTime));
        }
        if (_isGameOver) {
            GameOver();
        }
        // Godmode Commands for testing
        if (Input.GetKeyDown(KeyCode.Alpha0)) // 0
        {
            _playerHealth.Health = _playerHealthMax;
        }
        if (Input.GetKeyDown(KeyCode.Alpha1)) // 1
        {
            GameOver();
        }
    }

    /// <summary>
    /// The function activates the game over UI, plays feedbacks, and destroys all object pools.
    /// </summary>
    private void GameOver() 
    {
        _gameOverUI.SetActive(true);
        _gameFeedbacks.PlayFeedbacks();
        _pools.ForEach(pool => pool.DestroyPool());
    }

    /// <summary>
    /// The function resets the player's health and hides the game over UI.
    /// </summary>
    private void CleanUIAndVariables() 
    {
        _playerHealth.Health = _playerHealthMax;

        _gameOverUI.SetActive(false);
        _isGameOver = false;
    }

    /// <summary>
    /// The function restarts the game by cleaning the UI and variables, stopping feedbacks, and
    /// reloading the current scene. It is called when the player presses the restart button on the 
    /// game over UI.
    /// </summary>
    public void Restart() 
    {
        CleanUIAndVariables();

        _gameFeedbacks.StopFeedbacks();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        CleanUIAndVariables();
        _gameFeedbacks.StopFeedbacks();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// The function "QuitGame" is used to exit the application in C#.
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// The ManagerAddPool function adds an ObjectPool to a list of pools.
    /// </summary>
    /// <param name="ObjectPool">ObjectPool is a class or data structure that manages a pool of objects
    /// that can be reused. It is used to improve performance and reduce memory allocation by reusing
    /// objects instead of creating new ones. The ManagerAddPool method takes an ObjectPool object as a
    /// parameter and adds it to a list of pools</param>
    public void ManagerAddPool(ObjectPool pool)
    {
        _pools.Add(pool);
    }
}
