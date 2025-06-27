using System;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;   // El prefab del proyectil
    [SerializeField] private Transform firePoint;       // El punto de origen del disparo (puede ser el centro de la flecha)
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float damage = 1f; // Daño del proyectil

    private Animator animator;


    void Start() => animator = GetComponent<Animator>();
    

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Clic izquierdo
        {
            animator.SetTrigger("Shoot"); // Activar animación de disparo
        }
    }

    void Shoot()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Vector3 direction = (mousePos - firePoint.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Projectiles projectileScript = bullet.GetComponent<Projectiles>();
        projectileScript.SetDirection(direction);
        projectileScript.damage = damage; // Set the damage of the projectile
    }
}
