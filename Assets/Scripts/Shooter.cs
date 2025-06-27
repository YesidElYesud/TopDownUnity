using UnityEngine;

public class Shooter : MonoBehaviour
{
    public GameObject bulletPrefab;   // El prefab del proyectil
    public Transform firePoint;       // El punto de origen del disparo (puede ser el centro de la flecha)
    public float bulletSpeed = 10f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Clic izquierdo
        {
            Shoot();
        }
    }

    void Shoot()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Vector3 direction = (mousePos - firePoint.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.velocity = direction * bulletSpeed;
    }
}
