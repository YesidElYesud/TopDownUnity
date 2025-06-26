using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
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

    [Header("Shooting Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float shootingCooldown = 1.5f;
    [SerializeField] private float shootingDistance = 3f;
    [SerializeField] private Transform firePoint;

    private float shootTimer = 0f;


    private NavMeshAgent _navMeshAgent;
    private State _currentState = State.Patrol;
    private Transform _player;
    private int _currentPatrolIndex = 0;
    private float _chaseTimer = 0f;
    private Animator _animator;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player")?.transform;
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();

        _navMeshAgent.updateUpAxis = false;
        _navMeshAgent.updateRotation = false;
        _navMeshAgent.speed = patrolSpeed;

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
                    _navMeshAgent.speed = chaseSpeed;
                }
                break;

            case State.Chase:
                Chase();
                if (!CanSeePlayer())
                {
                    _chaseTimer -= Time.deltaTime;
                    if (_chaseTimer <= 0)
                    {
                        _currentState = State.Patrol;
                        _navMeshAgent.speed = patrolSpeed;
                        GoToNextPatrolPoint();
                    }
                }
                else
                {
                    _chaseTimer = chaseTime;
                }
                break;

            case State.Idle:
                _navMeshAgent.isStopped = true;
                break;
        }

        UpdateSpriteDirection();
    }

    private void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        if (!_navMeshAgent.hasPath || _navMeshAgent.remainingDistance < 0.2f)
        {
            GoToNextPatrolPoint();
        }
    }

    private void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        Transform target = patrolPoints[_currentPatrolIndex];
        _navMeshAgent.SetDestination(target.position);
        _currentPatrolIndex = (_currentPatrolIndex + 1) % patrolPoints.Length;
    }

    private void Chase()
    {
        if (_player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

        if (distanceToPlayer > shootingDistance)
        {
            _navMeshAgent.isStopped = false;
            _navMeshAgent.SetDestination(_player.position);
        }
        else
        {
            _navMeshAgent.isStopped = true;
            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0f)
            {
                _animator.SetTrigger("Shoot");
                shootTimer = shootingCooldown;
            }
        }
    }


    private void UpdateSpriteDirection()
    {
        if (_navMeshAgent.velocity.sqrMagnitude > 0.01f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(_navMeshAgent.velocity.x) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
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

    private void ShootAtPlayer()
    {
        Debug.Log("Enemy shot projectile");
        if (projectilePrefab == null || firePoint == null || _player == null)
        {
            if(_player == null)
                Debug.LogError("Player not found. Make sure it has the tag 'Player'.");
            else
                   Debug.LogError("Projectile prefab or fire point is not set.");
            return;
        }

        Vector2 direction = (_player.position - firePoint.position).normalized;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectiles projectileScript = proj.GetComponent<Projectiles>();

        if (projectileScript != null)
        {
            projectileScript.SetDirection(direction);
        }

        
    }


    public void Kill()
    {
        Debug.Log("Kill");
    }
}
