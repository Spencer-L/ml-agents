using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CooperativeFoodLogic : MonoBehaviour
{
    public bool respawn;
    public CooperativeFoodCollectorArea myArea;
    public float cooldown = 3f;

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
            transform.position = new Vector3(Random.Range(-myArea.rangeX, myArea.rangeX), 3f, Random.Range(-myArea.rangeZ, myArea.rangeZ)) + myArea.transform.position;
            // disable the food until the respawn cooldown is over
            gameObject.SetActive(false);
            StartCoroutine(EnableFood());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator EnableFood()
    {
        yield return new WaitForSeconds(cooldown);
        gameObject.SetActive(true);
    }
}
