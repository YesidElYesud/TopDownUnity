using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public HealthComponent target;
    public Image healthBar;
    public Image shieldBar;

    void Update()
    {
        if (target == null) return;

        healthBar.fillAmount = target.currentHealth / target.maxHealth;
        shieldBar.fillAmount = target.currentShield / target.maxShield;
    }
}
