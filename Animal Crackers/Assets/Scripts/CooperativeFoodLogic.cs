using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CooperativeFoodLogic : MonoBehaviour
{
    public bool respawn;
    public CooperativeFoodCollectorArea myArea;
    public float cooldown = 3f;
    private float cooldownTimer;

    private CooperativeFoodEnvController m_CooperativeFoodEnvController;

    private void Start()
    {
        m_CooperativeFoodEnvController = FindFirstObjectByType<CooperativeFoodEnvController>();
    }

    private void FixedUpdate()
    {
        if(cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
        else if(cooldownTimer <= 0 && !GetComponent<Renderer>().enabled)
        {
            Respawn();
        }
    }

    public void OnEaten(CooperativeFoodCollectorAgent agent)
    {
        if(!m_CooperativeFoodEnvController)
        {
            m_CooperativeFoodEnvController = FindFirstObjectByType<CooperativeFoodEnvController>();
        }
        m_CooperativeFoodEnvController.OnFoodEaten(this, agent);
        if (respawn)
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            // disable the renderer and collider so it can't be eaten again
            GetComponent<Renderer>().enabled = false;
            GetComponent<Collider>().enabled = false;
            cooldownTimer = cooldown;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Respawn()
    {
        GetComponent<Renderer>().enabled = true;
        GetComponent<Collider>().enabled = true;
        transform.position = new Vector3(Random.Range(-myArea.rangeX, myArea.rangeX), 3f, Random.Range(-myArea.rangeZ, myArea.rangeZ)) + myArea.transform.position;
    }
}
