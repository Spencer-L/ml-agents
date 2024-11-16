using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CooperativeFoodLogic : MonoBehaviour
{
    public bool respawn;
    public CooperativeFoodCollectorArea myArea;

    private CooperativeFoodEnvController m_CooperativeFoodEnvController;

    private void Start()
    {
        m_CooperativeFoodEnvController = FindFirstObjectByType<CooperativeFoodEnvController>();
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
            transform.position = new Vector3(Random.Range(-myArea.rangeX, myArea.rangeX),
                3f,
                Random.Range(-myArea.rangeZ, myArea.rangeZ)) + myArea.transform.position;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
