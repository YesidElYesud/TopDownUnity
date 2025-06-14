using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeInOut : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private AnimationClip AnimFade;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            StartCoroutine(CambiarEscena());
        }
    }

    IEnumerator CambiarEscena()
    {
        animator.SetTrigger("Iniciar");

        yield return new WaitForSeconds(AnimFade.length);

        SceneManager.LoadScene(1); 
    }
}
