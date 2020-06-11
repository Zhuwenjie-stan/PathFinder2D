using Application;
using UnityEngine;
//用来防止随机移动的时候超过leader
public class EvadeController : MonoBehaviour
{

    private IEntity leader_Locomotion;
    private IEntity m_pawn;
    private Vector2 leaderAheadPoint;
    private float LEADER_BEHIND_DIST;
    private Vector3 dist;
    public float evadeDistance;
    private float sqrEvadeDistance;

    void Start()
    {
        LEADER_BEHIND_DIST = 2.0f;
        sqrEvadeDistance = evadeDistance * evadeDistance;
    }


    void Update()
    {
        //计算领队前方的一个点
        leaderAheadPoint = leader_Locomotion.GetPosition() + leader_Locomotion.GetVelocity().normalized * LEADER_BEHIND_DIST;
        //计算角色当前位置与领队前方某店的距离，小于某个值，就需要躲避
        dist = m_pawn.GetPosition() - leaderAheadPoint;
        if (dist.sqrMagnitude < sqrEvadeDistance)
        {
            
             //enable force
        }
        else
        {
            
            //disable force 
        }
    }
}