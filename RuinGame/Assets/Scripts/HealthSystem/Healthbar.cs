using UnityEngine;
using UnityEngine.UI;

// Healthbar class, which handles the healthbar UI
public class Healthbar : MonoBehaviour
{
    Slider _healthSlider; // Health slider
    
    // Start is called on the first frame update
    private void Start()
    {
        _healthSlider = GetComponent<Slider>(); // Get the slider
    }

    // Set the max health of the healthbar
    public void SetMaxHealth(int maxHealth) 
    {
        _healthSlider.maxValue = maxHealth;
        _healthSlider.value = maxHealth;
    }

    // Set the health of the healthbar
    public void SetHealth(int health) 
    {
        _healthSlider.value = health;
    }
}
