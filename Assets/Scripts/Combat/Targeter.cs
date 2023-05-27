using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Targeter : NetworkBehaviour
{
    private Targetable target;

    public Targetable GetTarget() { return target; }

    [Command]
    public void CmdSetTarget(GameObject targetGameObject)
    {
        if (!targetGameObject.TryGetComponent<Targetable>(out Targetable newTarget)) { return; }
        target = newTarget;
    }

    [Server]
    public void ClearTarget()
    {
        target = null;
    }
}
