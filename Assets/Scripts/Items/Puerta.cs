using UnityEngine;

    public class Puerta : MonoBehaviour
    {
        public void Abrir()
        {
            Debug.Log("Puerta Abierta");
            Destroy(gameObject);
        }
    }

