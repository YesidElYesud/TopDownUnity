using UnityEngine;

public class keyItem : MonoBehaviour
{
    public string keyID;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            //GameManager.Instance.addkey(keyID)    
            Destroy(gameObject);
        }
    }
}
