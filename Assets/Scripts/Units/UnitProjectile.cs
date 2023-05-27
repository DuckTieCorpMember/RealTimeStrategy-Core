using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UnitProjectile : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb = null;
    [SerializeField] private int damageToDeal = 20;
    [SerializeField] private float launchForce = 10f;
    [SerializeField] private float destroyAfterSecond = 5f;

    private void Start()
    {
        rb.velocity = transform.forward * launchForce;
    }

    public override void OnStartServer()
    {
        Invoke(nameof(DesroySelf), destroyAfterSecond);
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<NetworkIdentity>(out NetworkIdentity netIdentity))
        {
            if (netIdentity.connectionToClient == connectionToClient) { return; }
        }

        if(other.TryGetComponent<Health>(out Health health))
        {
            health.DealDamage(damageToDeal);
        }
        DesroySelf();
    }

    [Server]
    private void DesroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }
}
