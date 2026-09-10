using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Obsolete("Deprecated. Use EnemyController instead.")]
public class EnemyBehaviour : MonoBehaviour
{
    [SerializeField] float _speed;
    [SerializeField] EnemyStats _currentStat = EnemyStats.Explore;
    Vector3 _positionTarget;
    public float rotationInterval = 2f;
    [SerializeField] float rotationTimer;
    private float targetRotationY;

    public Transform targetPlayer;
    public float distanceBtwAttack;
    public float distanceBtwRangedAttack = 10f;
    public bool damageEnemy;

    [SerializeField] private float _attackColdown;
    private float _attackColdownAux = 4f;
    [SerializeField] private float _rangedAttackColdown;
    private float _rangedAttackColdownAux = 2f; 
    [SerializeField] float life;
    Rigidbody _rb;
    bool enemyDie;
    [SerializeField] float _score;
    [SerializeField] private LineRenderer lineRenderer; 
    [SerializeField] private float rayDuration = 0.1f; 

    void Start()
    {
        _rb = GetComponentInChildren<Rigidbody>();
        targetPlayer = FindAnyObjectByType<PlayerManager>().transform;

        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer no encontrado en el enemigo.");
        }
    }

    void Update()
    {
        if (life < 35f)
        {
            _currentStat = EnemyStats.Defense;
        }

        switch (_currentStat)
        {
            case EnemyStats.Iddle:
                MoveRandom(_speed);
                EnemyDistanceAttack();
                break;
            case EnemyStats.Attack:
                EnemyDistanceAttack();
                Attack();
                break;
            case EnemyStats.Explore:
                MoveTowardsPlayer(_speed - 2f);
                EnemyDistanceAttack();
                break;
            case EnemyStats.Defense:
                EnemyDistanceDefense();
                MoveRandom(_speed);
                break;
            case EnemyStats.RangedAttack:
                RangedAttack();
                break;
        }
    }

    public void MoveRandom(float speed)
    {
        _rb.linearVelocity = transform.forward * speed;
        rotationTimer -= Time.deltaTime;

        if (rotationTimer <= 0f)
        {
            targetRotationY = Random.Range(0f, 360f);
            rotationTimer = rotationInterval;
        }
        Quaternion targetRotation = Quaternion.Euler(0f, targetRotationY, 0f);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 90f * Time.deltaTime);
    }

    public void MoveTowardsPlayer(float speed)
    {
        Vector3 direction = (targetPlayer.position - transform.position).normalized;
        _rb.linearVelocity = direction * speed;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 90f * Time.deltaTime);
    }

    void EnemyDistanceAttack()
    {
        float distance = Vector3.Distance(transform.position, targetPlayer.position);

        if (distance <= distanceBtwRangedAttack)
        {
            _currentStat = EnemyStats.RangedAttack; 
        }
        else if (distance <= distanceBtwAttack)
        {
            _currentStat = EnemyStats.Attack; 
        }
        else
        {
            _currentStat = EnemyStats.Explore; 
        }
    }

    void EnemyDistanceDefense()
    {
        float distance = Vector3.Distance(transform.position, targetPlayer.position);
        if (distance <= distanceBtwAttack + 2f)
        {
            _currentStat = EnemyStats.Defense;
        }
    }

    void Attack()
    {
        _attackColdown -= Time.deltaTime;
        if (_attackColdown <= 0f)
        {
            Vector3 direction = (targetPlayer.position - transform.position).normalized;

            if (_rb.linearVelocity.magnitude < 5f)
            {
                _rb.AddForce(direction * _speed, ForceMode.VelocityChange);
            }

            _attackColdown = _attackColdownAux;
        }
    }

    void RangedAttack()
    {
        _rangedAttackColdown -= Time.deltaTime;

        if (_rangedAttackColdown <= 0f)
        {
            Vector3 direction = (targetPlayer.position - transform.position).normalized;
            RaycastHit hit;

            if (Physics.Raycast(transform.position, direction, out hit, distanceBtwRangedAttack))
            {
                Debug.DrawLine(transform.position, hit.point, Color.red, rayDuration);

                if (lineRenderer != null)
                {
                    lineRenderer.SetPosition(0, transform.position);
                    lineRenderer.SetPosition(1, hit.point);
                    StartCoroutine(HideRayAfterDelay(rayDuration));
                }

                if (hit.collider.CompareTag("Player"))
                {
                    var player = hit.collider.GetComponent<PlayerManager>();
                    if (player != null)
                    {
                        //player.TakeDamage(30f); 
                    }
                }
            }

            _rangedAttackColdown = _rangedAttackColdownAux;
        }
    }

    IEnumerator HideRayAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, Vector3.zero);
        }
    }

    public void EnemyGetShot(float damage)
    {
        life -= damage;
        if (life <= 0)
        {
            enemyDie = true;

            //PlayerUI playerUI = FindObjectOfType<PlayerUI>();
            //if (playerUI != null)
            //{
            //    playerUI.AddScore((int)_score);
            //}

            Destroy(gameObject);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            var player = collision.collider.GetComponent<PlayerManager>();
            if (player != null)
            {
                //player.TakeDamage(15f);
            }
        }
    }
}

public enum EnemyStats { Iddle, Explore, Attack, Defense, RangedAttack }