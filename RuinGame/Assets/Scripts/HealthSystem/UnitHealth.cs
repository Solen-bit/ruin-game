// this class is used to store the health and max health of any unit in the game
public class UnitHealth
{
    // health and max health variables
    int _currentHealth;
    int _currentMaxHealth;

    // properties (getters and setters)
    public int Health { get { return _currentHealth; } set { _currentHealth = value; } }
    public int MaxHealth { get { return _currentMaxHealth; } set { _currentMaxHealth = value; } }

    // constructor
    public UnitHealth(int health, int maxHealth)
    {
        _currentHealth = health;
        _currentMaxHealth = maxHealth;
    }

    /// <summary>
    /// The function reduces the current health of a unit by a specified amount and checks if the unit
    /// is dead.
    /// </summary>
    /// <param name="damage">The amount of damage that the unit is taking.</param>
    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;

        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
        }
    }
}
