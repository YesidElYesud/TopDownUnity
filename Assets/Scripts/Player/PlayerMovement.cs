using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 3f;

    private Rigidbody2D playerRb;
    private Vector2 moveInput;
    private Animator playerAnimator;
    private SpriteRenderer sprite;
    private bool isDead = false;

    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(moveX, moveY).normalized;

        if (moveInput.x < 0)
        {
            sprite.flipX = true;
        }
        else if (moveInput.x > 0)
        {
            sprite.flipX = false;
        }

        bool isMoving = moveInput.magnitude > 0.1f;
        playerAnimator.SetBool("IsMoving", isMoving);
    }

    private void FixedUpdate()
    {
        // Físicas
        playerRb.MovePosition(playerRb.position + moveInput * speed * Time.fixedDeltaTime);
    }

    public void Death()
    {
        isDead = true;
        playerAnimator.SetTrigger("Dead");
        playerRb.velocity = Vector2.zero;
    }

    public void GameOver()
    {
        SceneManager.LoadScene("S_Game1");
    }
}
