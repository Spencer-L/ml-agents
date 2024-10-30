using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableWallController : MonoBehaviour
{
    [SerializeField] private int minAgentsToBreak = 3;
    [SerializeField] private Material brokenMaterial;

    private MeshRenderer _meshRenderer;
    private Collider _collider;
    private Material _originalMaterial;
    private int _agentsInContact;


    private CooperativeFoodEnvController m_CooperativeFoodEnvController;

    private void Awake()
    {
        _originalMaterial = GetComponent<MeshRenderer>().material;
        _meshRenderer = GetComponent<MeshRenderer>();
        _collider = GetComponent<Collider>();
        m_CooperativeFoodEnvController = FindFirstObjectByType<CooperativeFoodEnvController>();
    }

    public bool BreakWallCheck()
    {
        Debug.Log("Checking wall...");
        // check if there are enough agents in contact with the wall
        if (_agentsInContact >= minAgentsToBreak)
        {
            Debug.Log("Wall broken!");
            _meshRenderer.material = brokenMaterial;
            _collider.enabled = false;
            if (!m_CooperativeFoodEnvController)
            {
                m_CooperativeFoodEnvController = FindFirstObjectByType<CooperativeFoodEnvController>();
            }
            m_CooperativeFoodEnvController.OnWallBroken(this);
            return true;
        }
        return false;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("agent"))
        {
            _agentsInContact++;
            BreakWallCheck();
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("agent"))
        {
            _agentsInContact--;
        }
        if(_agentsInContact < 0 )
        {
            _agentsInContact = 0;
        }
    }

    public void ResetWall()
    {
        _meshRenderer.material = _originalMaterial;
        _collider.enabled = true;
        _agentsInContact = 0;
    }
}
