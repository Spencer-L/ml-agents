using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueenLogic : MonoBehaviour
{
    private CooperativeFoodEnvController m_CooperativeFoodEnvController;

    private void Start()
    {
        m_CooperativeFoodEnvController = FindFirstObjectByType<CooperativeFoodEnvController>();
    }

    public bool HandleReproduceRequest(GameObject agent)
    {
        if(!m_CooperativeFoodEnvController)
        {
            m_CooperativeFoodEnvController = FindFirstObjectByType<CooperativeFoodEnvController>();
        }

        if (m_CooperativeFoodEnvController.m_CooperativeFoodCollectorSettings.foodStored >= 10)
        {
            m_CooperativeFoodEnvController.OnQueenReproduce(agent);
            return true;
        }

        return false;
    }
}
