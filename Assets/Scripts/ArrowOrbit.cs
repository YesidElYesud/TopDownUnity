using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowOrbit : MonoBehaviour
{
    public Transform player;       // referencia al jugador
    public float orbitRadius = 1f; // radio de la órbita

    void Update()
    {
        // 1. Obtener posición del mouse en el mundo
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        // 2. Dirección del jugador al mouse
        Vector3 direction = (mousePos - player.position).normalized;

        // 3. Posicionar la flecha en la órbita
        transform.position = player.position + direction * orbitRadius;

        // 4. Rotar la flecha para que apunte hacia el mouse
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

    }
}

