using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
public class PlayerDebugController : MonoBehaviour
{
    private HealthComponent health;

    void Start()
    {
        health = GetComponent<HealthComponent>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Recibiendo 25 de daño tipo físico");
            health.TakeDamage(new DamageInfo(25f, null, "physical"));
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("Curando 20 de vida");
            health.Heal(20f);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("Recargando 15 de escudo");
            health.RechargeShield(15f);
        }
    }
}
