using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// This script is used to spawn enemies in a burst in a specified position.
// It can be used to spawn enemies in waves or just a single burst of enemies.
[RequireComponent(typeof(Collider))]
public class BurstSpawnArea : MonoBehaviour
{      
    // Enemy Spawning
    [Header("Enemy Spawning")]

    [SerializeField] private Collider _spawnArea; // if null, will use the collider on this object. Enemys will spawn within this area
    [SerializeField] private EnemySpawner _enemySpawner; // EnemySpawner to use for spawning enemies
    [SerializeField] private List<EnemyScriptableObject> _enemies = new List<EnemyScriptableObject>(); // list of enemies to spawn
    [SerializeField] private EnemySpawner.SpawnMethod _spawnMethod = EnemySpawner.SpawnMethod.Random; // spawning method
    [SerializeField] private int _numEnemiesToSpawn = 5; // number of enemies to spawn
    [SerializeField] private float _spawnDelay = 0.5f; // delay between each enemy spawn

    // Wave Spawning
    [Header("Wave Spawning")]

    [SerializeField] private bool _waveSpawner = false; // if true, will spawn waves of enemies
    [SerializeField] private int _numWaves = 2; // number of waves to spawn
    [SerializeField] private int _enemiesPerWave = 3; // number of enemies to spawn per wave
    [SerializeField] private float _timeBetweenWaves = 1f; // time between each wave
    [SerializeField] private Transform _spawnPoint; // spawn point for enemies, sets specific spawn point for enemies

    private int _currentWave = 1; // current wave
    private int _currentEnemies = 0; // current enemies spawned
    private bool _spawningFinished = false; // if true, spawning is finished

    // Gate
    private bool _gateOpen = true; // if true, gate is open
    public bool GateOpen { get { return _gateOpen; } } // returns if gate is open
    public UnityEvent<bool> OnGate; // event for when gate is opened or closed, caught in ArenaGate.cs

    private Coroutine _spawnCoroutine; // coroutine for spawning enemies
    private Bounds _bounds; // bounds of the spawn area

    // Awake is called before Start
    private void Awake()
    {
        /* This code is checking if the `_spawnArea` variable is null. If it is null, it sets
        `_spawnArea` to the `Collider` component. It then sets `_bounds` to the bounds of the `_spawnArea`
        collider. This ensures that the script has a valid collider to use for spawning enemies
        within the designated area. */
        if (_spawnArea == null)
        {
            _spawnArea = GetComponent<Collider>();
        }
        _bounds = _spawnArea.bounds;
    }
    
    // Start is called before the first frame update
    private void Start()
    {
        OnGate?.Invoke(true); // gates should start open
    }

    /// <summary>
    /// The function checks if a wave spawner is present and either starts spawning enemies or starts
    /// spawning a wave depending on the condition.
    /// </summary>
    /// <param name="Collider">Collider is a component in Unity that defines a physical boundary for an
    /// object. It is used to detect collisions with other objects in the game world. In this code
    /// snippet, OnTriggerEnter is a method that is called when an object with a collider enters the
    /// trigger zone of another collider.r</param>
    private void OnTriggerEnter(Collider other) 
    {
        if (!_waveSpawner) { if (_spawnCoroutine == null) { _spawnCoroutine = StartCoroutine(SpawnEnemies()); } }
        else 
        {
            if (_spawnCoroutine == null)
            {
                _spawnCoroutine = StartCoroutine(SpawnWave());
                _gateOpen = false;
                OnGate?.Invoke(_gateOpen);
            }
        }
    }

    // Update is called once per frame
    /// <summary>
    /// The Update function sets a gate to open and invokes an event when spawning is finished.
    /// </summary>
    private void Update()
    {
        if (_spawningFinished)
        {
            _gateOpen = true; // Open gate
            OnGate?.Invoke(_gateOpen); // Invoking event caught in the ArenaGate.cs script
            Destroy(gameObject); // Destroying the spawner
        }
    }

    /// <summary>
    /// This function returns a random Vector3 position within a given bounds.
    /// </summary>
    /// <returns>
    /// A Vector3 representing a random position within the specified bounds. The x and z coordinates
    /// are randomly generated within the minimum and maximum x and z values of the bounds, while the y
    /// coordinate is set to the minimum y value of the bounds.
    /// </returns>
    private Vector3 GetRandomPositionInBounds()
    {
        return new Vector3(Random.Range(_bounds.min.x, _bounds.max.x), _bounds.min.y, Random.Range(_bounds.min.z, _bounds.max.z));
    }

    /// <summary>
    /// This function spawns a certain number of enemies using either a round-robin or random method.
    /// Used only when wave spawning is not enabled.
    /// </summary>
    private IEnumerator SpawnEnemies()
    {
        WaitForSeconds wait = new WaitForSeconds(_spawnDelay);
        for (int i= 0; i < _numEnemiesToSpawn; i++)
        {
            if (_spawnMethod == EnemySpawner.SpawnMethod.RoundRobin)
            {
                /* This line of code is calling the `SpawnEnemy` method of the `_enemySpawner` object,
                which is an instance of the `EnemySpawner` class. The method takes two arguments:
                the index of the enemy to spawn, and the position at which to spawn the enemy. */
                _enemySpawner.SpawnEnemy(
                    _enemySpawner.enemies.FindIndex((enemy) => enemy.Equals(_enemies[i % _enemies.Count])), 
                    GetRandomPositionInBounds()
                );
            }
            else if (_spawnMethod == EnemySpawner.SpawnMethod.Random)
            {
                int index = Random.Range(0, _enemies.Count); // Randomly select an enemy to spawn
                _enemySpawner.SpawnEnemy(
                    _enemySpawner.enemies.FindIndex((enemy) => enemy.Equals(_enemies[index])),
                    GetRandomPositionInBounds()
                );
            }

            yield return wait;
        }
        Destroy(gameObject);
    }

    /// <summary>
    /// This function spawns a wave of enemies either at a specific spawn point or randomly within a
    /// defined area.
    /// </summary>
    private IEnumerator SpawnWave()
    {
        Debug.Log("Spawning Wave " + _currentWave);
        _currentEnemies = 0;
        for (int i = 0; i < _enemiesPerWave; i++)
        {
            int enemyIndex = _enemySpawner.enemies.FindIndex((enemy) => enemy.Equals(_enemies[i % _enemies.Count]));
            if (_spawnPoint != null) // If spawn point is set, spawn enemies at spawn point
            {
                Enemy enemy = _enemySpawner.SpawnEnemy(
                    enemyIndex,
                    _spawnPoint.position
                );
                enemy.OnDeathWave.AddListener(EnemyKilled); // Add listener to enemy death event
            }
            else // Otherwise spawn enemies in the spawn area randomly
            {
                Enemy enemy = _enemySpawner.SpawnEnemy(
                    enemyIndex,
                    GetRandomPositionInBounds()
                );
                enemy.OnDeathWave.AddListener(EnemyKilled); // Add listener to enemy death event
            }
            
            _currentEnemies++;

            yield return new WaitForSeconds(_spawnDelay);
        }
    }

    /// <summary>
    /// The function decreases the number of current enemies and starts the next wave if all enemies
    /// have been killed. If all waves have been completed, the spawning is finished. Used by the OnDeathWave event
    /// in the Enemy.cs script.
    /// </summary>
    private void EnemyKilled()
    {
        Debug.Log("Enemy Killed");
        _currentEnemies--;
        if (_currentEnemies <= 0)
        {
            _currentWave++;
            if (_currentWave > _numWaves)
            {
                _spawningFinished = true;
            }
            else
            {
                StartCoroutine(StartNextWave());
            }
        }
    }

    /// <summary>
    /// This function waits for a specified time and then starts spawning a new wave.
    /// </summary>
    private IEnumerator StartNextWave()
    {
        yield return new WaitForSeconds(_timeBetweenWaves);
        StartCoroutine(SpawnWave());
    }
}
