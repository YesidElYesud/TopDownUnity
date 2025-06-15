using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesController : MonoBehaviour
{
    private enum State { Patrol, Chase, Idle }

    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolSpeed = 2f;

    [Header("Detection Settings")]
    [SerializeField] private float viewDistance = 5f;
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private Transform eyes;

    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float chaseTime = 3f;

    private State _currentState = State.Patrol;
    private Transform _player;
    private int _currentPatrolIndex = 0;
    private float _chaseTimer = 0f;
    private Animator _animator;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player")?.transform;
        _animator = GetComponent<Animator>();
        if (_player == null)
            Debug.LogError("Player not found. Make sure it has the tag 'Player'.");
    }

    private void Update()
    {
        switch (_currentState)
        {
            case State.Patrol:
                Patrol();
                if (CanSeePlayer())
                {
                    _currentState = State.Chase;
                    _chaseTimer = chaseTime;
                }
                break;

            case State.Chase:
                Chase();
                if (!CanSeePlayer())
                {
                    _chaseTimer -= Time.deltaTime;
                    if (_chaseTimer <= 0)
                        _currentState = State.Patrol;
                }
                else
                {
                    _chaseTimer = chaseTime;
                }
                break;

            case State.Idle:
                // Puedes agregar lógica de espera aquí
                break;
        }
    }

    private void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        Transform target = patrolPoints[_currentPatrolIndex];
        MoveTowards(target.position, patrolSpeed);

        if (Vector2.Distance(transform.position, target.position) < 0.1f)
            _currentPatrolIndex = (_currentPatrolIndex + 1) % patrolPoints.Length;
    }

    private void Chase()
    {
        if (_player != null)
            MoveTowards(_player.position, chaseSpeed);
    }

    private void MoveTowards(Vector2 target, float speed)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        transform.localScale = new Vector3(Mathf.Sign(direction.x), 1, 1);
    }

    private bool CanSeePlayer()
    {
        if (_player == null) return false;

        Vector2 dirToPlayer = _player.position - eyes.position;
        if (dirToPlayer.magnitude > viewDistance) return false;

        float angle = Vector2.Angle(eyes.right * transform.localScale.x, dirToPlayer);
        if (angle > viewAngle / 2f) return false;

        RaycastHit2D hit = Physics2D.Raycast(eyes.position, dirToPlayer.normalized, viewDistance, playerLayer | obstacleLayer);
        if (hit && hit.collider.CompareTag("Player"))
            return true;

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (eyes == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyes.position, viewDistance);

        Vector3 forward = eyes.right * transform.localScale.x;
        Quaternion leftRayRotation = Quaternion.Euler(0, 0, viewAngle / 2);
        Quaternion rightRayRotation = Quaternion.Euler(0, 0, -viewAngle / 2);

        Vector3 leftRayDirection = leftRayRotation * forward;
        Vector3 rightRayDirection = rightRayRotation * forward;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(eyes.position, leftRayDirection * viewDistance);
        Gizmos.DrawRay(eyes.position, rightRayDirection * viewDistance);
    }

    public void Shoot()
    {
        Debug.Log("Shooting");
    }

    public void Kill()
    {
        Debug.Log("Kill");
    }
}
