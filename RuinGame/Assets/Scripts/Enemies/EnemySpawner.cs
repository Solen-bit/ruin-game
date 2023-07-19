using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// EnemySpawner.cs; Contains logic and parameters for spawning enemies
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] PlayerController _player; // player reference
    [SerializeField] int _numEnemiesToSpawn = 5; // number of enemies to spawn
    [SerializeField] float _spawnDelay = 1f; // delay between each enemy spawn

    public List<EnemyScriptableObject> enemies = new List<EnemyScriptableObject>(); // list of enemies to spawn

    [SerializeField] Camera _camera; // camera reference
    [SerializeField] Canvas _canvas; // canvas reference
    [SerializeField] Transform _floor; // floor reference

    HealthFollowCamera _healthFollowCamera; // health follow camera reference. This is used to make the health bars 'look' at the camera

    [SerializeField] SpawnMethod _spawnMethod = SpawnMethod.RoundRobin; // spawning method

    private int _enemiesSpawned = 0; // number of enemies spawned
    private NavMeshTriangulation _triangulation; // triangulation of the navmesh
    private Dictionary<int, ObjectPool> _enemyObjPools = new Dictionary<int, ObjectPool>(); // dictionary of object pools

    // Awake is called before first frame update
    private void Awake()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            _enemyObjPools.Add(i, ObjectPool.CreateInstance(enemies[i].E_Prefab, _numEnemiesToSpawn)); // create object pool for each enemy type
        }
    }

    // Start is called on the first frame
    private void Start()
    {
        _triangulation = NavMesh.CalculateTriangulation(); // calculate triangulation of the navmesh
        StartCoroutine(SpawnEnemies()); // spawn enemies
    }

    /// <summary>
    /// This function spawns a specified number of enemies using either a round-robin or random method
    /// with a delay between each spawn.
    /// </summary>
    private IEnumerator SpawnEnemies()
    {
        _enemiesSpawned = 0;
        WaitForSeconds wait = new WaitForSeconds(_spawnDelay);

        while (_enemiesSpawned < _numEnemiesToSpawn)
        {
            if (_spawnMethod == SpawnMethod.RoundRobin) { SpawnRoundRobinEnemy(_enemiesSpawned); }
            else if (_spawnMethod == SpawnMethod.Random) { SpawnRandomEnemy(); }

            _enemiesSpawned++;

            yield return wait;
        }
    }

    /// <summary>
    /// This function spawns enemies in a round-robin fashion based on the number of enemies already
    /// spawned.
    /// </summary>
    /// <param name="spawnedEnemies">The number of enemies that have already been spawned in the current
    /// round.</param>
    private void SpawnRoundRobinEnemy(int spawnedEnemies)
    {
        int spawnIndex = spawnedEnemies % enemies.Count;

        SpawnEnemy(spawnIndex, ChooseRandomPosition());
    }

    /// <summary>
    /// This function spawns a random enemy from a list of enemies at a randomly chosen position.
    /// </summary>
    private void SpawnRandomEnemy()
    {
        SpawnEnemy(Random.Range(0, enemies.Count), ChooseRandomPosition());
    }

    /// <summary>
    /// This function chooses a random position from an array of vertices in a triangulation.
    /// </summary>
    /// <returns>
    /// A randomly chosen Vector3 position from the vertices of a triangulation.
    /// </returns>
    private Vector3 ChooseRandomPosition()
    {
        int vertexIndex = Random.Range(0, _triangulation.vertices.Length);
        return _triangulation.vertices[vertexIndex];
    }

    /// <summary>
    /// This function spawns an enemy object from a pool at a specified position and sets up its
    /// properties.
    /// </summary>
    /// <param name="spawnIndex">The index of the enemy object pool from which the enemy will be
    /// spawned.</param>
    /// <param name="Vector3">A 3D vector representing the position where the enemy should be
    /// spawned.</param>
    /// <returns>
    /// An instance of the Enemy class is being returned.
    /// </returns>
    public Enemy SpawnEnemy(int spawnIndex, Vector3 spawnPosition)
    {
        PoolableObject poolableObject = _enemyObjPools[spawnIndex].GetObject(); // get enemy from pool

        if (poolableObject != null)
        {
            Enemy enemy = poolableObject.GetComponent<Enemy>(); // get enemy component
            enemies[spawnIndex].SetupEnemy(enemy); // set up enemy

            NavMeshHit hit; // create navmesh hit
            if (NavMesh.SamplePosition(spawnPosition, out hit, 2f, -1)) // if navmesh hit is successful
            {
                enemy.Agent.Warp(hit.position); // warp enemy to navmesh hit position

                if (!enemy.Movement.Sight.isActiveAndEnabled) { enemy.Movement.Sight.enabled = true; } // enable enemy sight

                enemy.Movement.E_Player = _player.transform; // set player transform for the Movement component
                enemy.Movement.E_Triangulation = _triangulation; // set triangulation reference

                _healthFollowCamera = enemy.GetComponentInChildren<HealthFollowCamera>(); // get health follow camera component
                _healthFollowCamera.HealthFollow = _camera; // set camera used by health follow camera

                enemy.Agent.enabled = true; // enable enemy agent
                if (enemy.EnemyDamager) enemy.EnemyDamager.DamagerCollider.enabled = false; // disable enemy damager collider
                enemy.EnemyHealthbar.enabled = false; // disable enemy healthbar

                enemy.Movement.Spawn(); // spawn enemy

                enemy.PlayerController = _player; // set player reference
                return enemy;
            }
            else
            {
                Debug.LogError($"Unable to spawn enemy of type {spawnIndex} at position {spawnPosition}.");
                return null;
            }
        }
        else
        {
            Debug.LogError($"Unable to fetch enemy of type {spawnIndex} from pool.");
            return null;
        }
    }

    /*This enumeration is used as a parameter for the `_spawnMethod` variable in the `SpawnEnemies()` 
    function to determine the method of spawning enemies. It allows the developer to easily 
    switch between different spawning methods without having to use magic numbers or strings. */
    public enum SpawnMethod
    {
        RoundRobin,
        Random
    }
}
