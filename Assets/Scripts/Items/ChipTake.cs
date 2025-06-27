using UnityEngine;

public class ChipTake : MonoBehaviour
{
    public int value = 2;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.AddMoneda(value);
            Destroy(gameObject);
        }
    }
}