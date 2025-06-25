using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puerta : MonoBehaviour
{
    public string idkeyneed;
    private Animator animator;
    private bool abierta = false;
    
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!abierta && other.CompareTag("Player"))
        {
            if(GameManager.Instance.haskey(idkeyneed))
            {
                AbrirPuerta();
            }
        }
    }

    void AbrirPuerta()
    {
        abierta = true;
        animator.SetTrigger("Abierta");
    }
}
