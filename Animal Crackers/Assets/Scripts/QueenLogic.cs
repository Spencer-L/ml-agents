using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueenLogic : MonoBehaviour
{
    [Header("Reproduction Settings")]
    public float reproductionRate = 5f;
    public bool isCoolingDown;

    private float m_ReproductionTimer;

    private CooperativeFoodEnvController m_CooperativeFoodEnvController;

    private void Start()
    {
        m_CooperativeFoodEnvController = FindFirstObjectByType<CooperativeFoodEnvController>();
    }

    private void Update()
    {
        if (isCoolingDown)
        {
            m_ReproductionTimer += Time.deltaTime;
            if (m_ReproductionTimer >= reproductionRate)
            {
                isCoolingDown = false;
                m_ReproductionTimer = 0;
            }
        }
    }

    public bool HandleReproduceRequest(GameObject agent)
    {
        if(!m_CooperativeFoodEnvController)
        {
            m_CooperativeFoodEnvController = FindFirstObjectByType<CooperativeFoodEnvController>();
        }

        if (m_CooperativeFoodEnvController.m_CooperativeFoodCollectorSettings.foodStored >= 10 && !isCoolingDown)
        {
            isCoolingDown = true;
            m_CooperativeFoodEnvController.OnQueenReproduce(agent);
            return true;
        }

        return false;
    }
}
