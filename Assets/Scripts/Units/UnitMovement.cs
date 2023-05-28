using UnityEngine;
using Mirror;
using UnityEngine.AI;

public class UnitMovement : NetworkBehaviour
{
    [SerializeField] NavMeshAgent agent = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private float chaseRange = 5f;

    #region Server

    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    [ServerCallback]
    private void Update()
    {
        Targetable target = targeter.GetTarget();
        if(target != null) { 
            if((target.transform.position-transform.position).sqrMagnitude > chaseRange*chaseRange)
            {
                agent.SetDestination(target.transform.position);
            }else if(agent.hasPath)
            {
                agent.ResetPath();
            }

            return;
        }

        if(!agent.hasPath) { return; }
        if(agent.remainingDistance > agent.stoppingDistance) { return; }
        agent.ResetPath();
    }

    [Command]
    public void CmdMove(Vector3 position)
    {
        targeter.ClearTarget(); 

        if(!NavMesh.SamplePosition(position, out NavMeshHit hit, 1, NavMesh.AllAreas)) { return; }
        agent.SetDestination(hit.position);
    }

    private void ServerHandleGameOver()
    {
        agent.ResetPath();
    }
    #endregion
}
