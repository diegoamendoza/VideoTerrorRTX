using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    public float startWaitTime = 4;
    public float timeToRotate = 2;
    public float speedWalk = 6;
    public float speedRun = 9;

    public float viewRadius = 15;
    public float viewAngle = 80;
    public LayerMask playerMask;
    public LayerMask obstacleMask;


    public Transform[] waypoints;
    int m_CurrentWaypointIndex;
    Vector3 playerLastPosition = Vector3.zero;
    Vector3 m_PlayerPosition;

    float m_WaitTime;
    float m_TimeToRotate;
    public bool m_playerInRange;
    public bool m_PlayerNear;
    public bool m_IsPatrol;
    public bool m_CaughtPlayer;
    private player_movement player;

    void Start()
    {
        player = FindObjectOfType<player_movement>();
        m_PlayerPosition = Vector3.zero;
        m_IsPatrol = true;
        m_CaughtPlayer = false;
        m_playerInRange = false;
        m_PlayerNear = false;
        m_WaitTime = startWaitTime;
        m_TimeToRotate = timeToRotate;

        m_CurrentWaypointIndex = 0;
        navMeshAgent = GetComponent<NavMeshAgent>();

        navMeshAgent.isStopped = false;
        navMeshAgent.speed = speedWalk;
        navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex]
            .position);
    }

    private void Update()
    {
        if (!m_IsPatrol &&
            Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("Player").transform.position) < 14 &&
            m_playerInRange)
        {
            Debug.Log("Pierdes");
            CaughtPlayer();
            player.medidorCardiaco = 201;
        }


        EnviromentView();

        if (!m_IsPatrol)
        {
            Chasing();
        }
        else
        {
            Patroling();
        }
    }

    private void Chasing()
    {
        m_PlayerNear = false;
        playerLastPosition = Vector3.zero;

        if (!m_CaughtPlayer)
        {
            Move(speedRun);
            navMeshAgent.SetDestination(m_PlayerPosition);
        }

        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance
        )
        {
            if (m_WaitTime <= 0 && !m_CaughtPlayer && Vector3.Distance(transform.position,
                GameObject.FindGameObjectWithTag("Player").transform.position) >= 6f)
            {
                m_IsPatrol = true;
                m_PlayerNear = false;
                Move(speedWalk);
                m_TimeToRotate = timeToRotate;
                m_WaitTime = startWaitTime;
                navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);
            }
            else
            {
                if (Vector3.Distance(transform.position,
                    GameObject.FindGameObjectWithTag("Player").transform.position) >= 2.5f)
                {
                    Stop();
                    m_WaitTime -= Time.deltaTime;
                }
            }
        }
    }

    private void Patroling()
    {
        if (m_PlayerNear)
        {
            if (m_TimeToRotate <= 0)
            {
                Move(speedWalk);
                LookingPlayer(playerLastPosition);
            }
            else
            {
                Stop();
                m_TimeToRotate -= Time.deltaTime;
            }
        }
        else
        {
            m_PlayerNear = false;
            playerLastPosition = Vector3.zero;
            navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex]
                .position);
            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                if (m_WaitTime <= 0)
                {
                    NextPoint();
                    Move(speedWalk);
                    m_WaitTime = startWaitTime;
                }
                else
                {
                    Stop();
                    m_WaitTime -= Time.deltaTime;
                }
            }
        }
    }
    public void NextPoint()
    {
        m_CurrentWaypointIndex = (m_CurrentWaypointIndex + 1) % waypoints.Length;
        navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);
    }

    void Stop()
    {
        navMeshAgent.isStopped = true;
        navMeshAgent.speed = 0;
    }

    void Move(float speed)
    {
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = speed;
    }

    void CaughtPlayer()
    {
        m_CaughtPlayer = true;
    }

    void LookingPlayer(Vector3 player)
    {
        navMeshAgent.SetDestination(player);
        if (Vector3.Distance(transform.position, player) <= 0.3)
        {
            if (m_WaitTime <= 0)
            {
                m_PlayerNear = false;
                Move(speedWalk);
                navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);
                m_WaitTime = startWaitTime;
                m_TimeToRotate = timeToRotate;
            }
            else
            {
                Stop();
                m_WaitTime -= Time.deltaTime;
            }
        }
    }

    void EnviromentView()
    {
        Collider[]
            playerInRange =
                Physics.OverlapSphere(transform.position, viewRadius,
                    playerMask);

        for (int i = 0; i < playerInRange.Length; i++)
        {
            Transform player = playerInRange[i].transform;
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2)
            {
                float dstToPlayer =
                    Vector3.Distance(transform.position, player.position);
                if (!Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), dirToPlayer, dstToPlayer,
                    obstacleMask))
                {
                    m_playerInRange =
                        true;
                    m_IsPatrol = false;
                }
                else
                {
                    m_playerInRange = false;
                }
            }

            if (Vector3.Distance(transform.position, player.position) > viewRadius)
            {
                m_playerInRange = false;
            }

            if (m_playerInRange)
            {
                m_PlayerPosition =
                    player.transform
                        .position;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, m_PlayerPosition * 100);
    }
}