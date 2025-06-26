using UnityEngine;

namespace Items
{
    public class KeyItem : MonoBehaviour
    {
        public Puerta PuertaAsociada;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (PuertaAsociada != null)
                {
                    PuertaAsociada.Abrir();
                }
                   
                Destroy(gameObject);
                
            }
        }
    }
}
