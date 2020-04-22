﻿using System.Collections;
using System.Collections.Generic;
using Application;
using UnityEngine;

public class FollowController : SceneObjController, IEntity
{
    public Transform FollowTarget;
    private SceneObjController followTargetController;

    public float MoveSpeed_Max { get { return m_MoveSpeed_Max; } }
    private float m_MoveSpeed_Max;

    private List<int> m_Path = new List<int>();
    private WalkState m_State;

    /// <summary>
    /// 本次移动方向
    /// </summary>
    private Vector2 m_CurrentVelocity;
    
    /// <summary>
    /// 本次 wander 的角度
    /// </summary>
    private float m_CurrentWanderAngle;

    /// <summary>
    /// wander 转向最大角度
    /// </summary>
    private float m_AngleChange_Max;

    /// <summary>
    /// 最大转向力（用以平滑转向）
    /// </summary>
    private float m_ForceSteer_Max;

    /// <summary>
    /// 本物体的质量
    /// </summary>
    private float m_TempMass;

    /// <summary>
    /// 距离目的地 停下的距离
    /// </summary>
    public float m_StopRadius;

    /// <summary>
    /// 距离目的地 开始减速的距离
    /// </summary>
    private float m_SlowingRadius;

    /// <summary>
    /// 漫步圆的距离
    /// </summary>
    private float m_CircleDistance;

    /// <summary>
    /// 漫步圆的大小
    /// </summary>
    private float m_CircleRadius;

    private Vector2 m_Position;
    private SteeringManager m_Steering;

    protected override void OnStart()
    {
        base.OnStart();
        if(FollowTarget!= null){
            followTargetController = FollowTarget.GetComponent<SceneObjController>();
        }
        m_MoveSpeed_Max = 0.1f;
        m_ForceSteer_Max = 1f;
        m_AngleChange_Max = 50;

        m_CircleDistance = 0.3f;
        m_CircleRadius = 0.1f;

        m_StopRadius = 0.5f;
        m_SlowingRadius = 1f;

        m_CurrentVelocity = Vector2.zero;
        m_TempMass = 1;
        m_Steering = new SteeringManager(this);
    }

    public void SetFollowTarget(Transform followTarget){
        FollowTarget = followTarget;
        followTargetController = FollowTarget.GetComponent<SceneObjController>();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (Input.GetKey(KeyCode.G))
        {
            CalFollowPath();

            if (!CheckNeedFollow() || m_Path == null)
            {
                return;
            }
            

            // Step 1. 平滑转向

            //Vector2 steeringVector = CalSteeringVector(desiredVelocity, m_CurrentVelocity);

            //Vector2 finalVelocity = AddSteering(m_CurrentVelocity, steeringVector);


            // Step 2. 随机漫步

            //Vector2 wanderVector = CalWanderVector(finalVelocity);

            //finalVelocity = AddSteering(finalVelocity, wanderVector);

            //
            Vector2 targetPos = GetNextTargetPos();
            m_Steering.Seek(targetPos);
            m_Steering.Update();
            transform.position = m_Position;
            
            
            if (m_CurrentVelocity.x == 0 && m_CurrentVelocity.y == 0)
            {
                Debug.Log($"m_CurrentCellId: {m_CurrentCellId}           path: {Logger.ListToString(m_Path)}");
            }

            if (m_Steering.Steering.magnitude == 0 && m_Path != null && m_Path.Count != 0)
                Debug.LogError("11");
            Debug.Log($"m_CurrentVelocity: {m_Steering.Steering}     transform:{transform.position}");
        }
    }

    private void CalFollowPath()
    {
        Debug.Log($"FindPathRequest - m_CurrentCellId: {m_CurrentCellId}");
        MapManager.Instance.PathFinder.FindPathRequest(CurrentCellId, followTargetController.CurrentCellId, PathFindAlg.Astar, SetPath);
    }

    private Vector2 GetNextTargetPos()
    {
        int idx = m_Path.IndexOf(m_CurrentCellId);

        if(m_Path.Count <= idx + 1)
        {
            return Vector2.zero;
        }

        var targetPos = CellManager.Instance.GetCellByID(m_Path[idx + 1]).transform.position;

        return targetPos;
    }

    private Vector2 CalSteeringVector(Vector2 desiredVelocity, Vector2 currentVelocity)
    {
        Vector2 steeringForce = desiredVelocity - currentVelocity;
        
        VectorHandler.Truncate(ref steeringForce, m_ForceSteer_Max);

        Vector2 steeringVector = steeringForce / m_TempMass;

        return steeringVector;
    }

    private Vector2 AddSteering(Vector2 moveVector, Vector2 steeringVector)
    {
        Vector2 moveDir = moveVector + steeringVector;

        VectorHandler.Truncate(ref moveDir, m_MoveSpeed_Max);

        return moveDir;
    }

    private Vector2 CalWanderVector(Vector2 moveDir)
    {
        Vector2 circleCenter = moveDir.normalized * m_CircleDistance;

        Vector2 displacement = Vector2.down * m_CircleRadius;

        // 通过修改当前角度，随机改变向量的方向，m_CurrentWanderAngle 初始为 0，默认向上
        SetAngle(ref displacement, m_CurrentWanderAngle);

        m_CurrentWanderAngle += (Random.Range(0f, 1f) * m_AngleChange_Max) - (m_AngleChange_Max * 0.5f);

        Vector2 wanderForce = circleCenter + displacement;

        VectorHandler.Truncate(ref wanderForce, m_ForceSteer_Max);

        Vector2 wanderVector = wanderForce / m_TempMass;

        return wanderVector;
    }

    private void SetAngle(ref Vector2 vector, float value)
    {
        float len = vector.magnitude;
        vector.x = Mathf.Cos(value) * len;
        vector.y = Mathf.Sin(value) * len;
    }

    private bool CheckNeedFollow()
    {
        bool needFollow = false;

        if (FollowTarget != null && followTargetController != null)
        {
            if (followTargetController.CurrentCellId == m_CurrentCellId)
            {
                if ((FollowTarget.position - transform.position).magnitude > m_StopRadius)
                {
                    needFollow = true;
                }
            }
            else
            {
                needFollow = true;
            }
        }

        return needFollow;
    }

    private void SetPath(List<int> path)
    {
        Debug.Log($"SetPath - m_CurrentCellId: {m_CurrentCellId}          path: {Logger.ListToString(path)}");

        if (!path.Contains(m_CurrentCellId))
            return;

        m_Path.Clear();
        m_Path.AddRange(path);
    }
#region IEntity Interface
    public Vector2 GetVelocity()
    {
        return m_CurrentVelocity;
    }

    public float GetMaxVelocity()
    {
        return m_MoveSpeed_Max;
    }

    public Vector2 GetPosition()
    {
        return m_Position;
    }

    public float GetMass()
    {
        return m_TempMass;
    }

    public void SetPosition(Vector2 pos)
    {
        m_Position = pos;
    }

    public void SetVelocity(Vector2 velocity)
    {
        m_CurrentVelocity = velocity;
    }
    #endregion
}
