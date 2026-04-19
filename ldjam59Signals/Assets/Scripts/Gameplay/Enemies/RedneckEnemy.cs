using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RedneckEnemy : MonoBehaviour
{
    [Serializable]
    public class RedneckPatrolPoint
    {
        public Transform point;
        public float waitTime;
    }

    [SerializeField] 
    private List<RedneckPatrolPoint> _patrolPoints;
    
    [SerializeField]
    private Billboard billboard;

    [SerializeField] 
    private NavMeshAgent agent;

    [SerializeField] 
    private float _playerSearchRadius = 10;
    
    [SerializeField] 
    private float _playerLostRadius = 20;
    public NavMeshAgent Agent => agent;

    private TexturePainter _texturPainter;

    private Vector3 prevPosition;
    private bool _pursuit = false;

    private void Start()
    {
        _texturPainter = Game.Instance.TexturePainter;
    }

    private void OnEnable()
    {
        StartCoroutine(PatrolRoutine());
    }

    private int i = 0;
    private IEnumerator PatrolRoutine()
    {
        while (true)
        {
            i = (i + 1) % _patrolPoints.Count;
            var nextPoint = _patrolPoints[i];
            GoToPatrolPoint();
            while (Vector3.Distance(transform.position, nextPoint.point.position) < 2)
            {
                if (!TryPursuitPlayer())
                {
                    GoToPatrolPoint();
                }
                yield return null;
            }
            var nextTime = Time.time + nextPoint.waitTime;
            while (Time.time < nextTime)
            {
                if (!TryPursuitPlayer())
                {
                    GoToPatrolPoint();
                }
                yield return null;
            }
        }
    }

    private bool TryPursuitPlayer()
    {
        if (Game.Instance == null || Game.Instance.FirstPersonController == null)
        {
            return false;
        }
        if (Vector3.Distance(transform.position, Game.Instance.FirstPersonController.transform.position) > _playerLostRadius)
        {
            _pursuit = false;
        }
        if (Vector3.Distance(transform.position, Game.Instance.FirstPersonController.transform.position) < _playerSearchRadius || _pursuit)
        {
            _pursuit = true;
            GoToPlayer();
        }

        return _pursuit;
    }

    private void GoToPatrolPoint()
    {
        var nextPoint = _patrolPoints[i];
        Agent.SetDestination(nextPoint.point.position);
    }
    private void GoToPlayer()
    {
        Agent.SetDestination(Game.Instance.FirstPersonController.transform.position);
    }

    private void Update()
    {
        var position = transform.position;
        billboard.SetMovingDirection((position - prevPosition));
        prevPosition = position;
    }
}