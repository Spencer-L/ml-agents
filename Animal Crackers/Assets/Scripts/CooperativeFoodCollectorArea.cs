using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgentsExamples;

public class CooperativeFoodCollectorArea : Area
{
    public GameObject food;
    public int numFood;
    public bool respawnFood;
    public float respawnCooldown = 3f;
    public float rangeX, rangeZ;

    void CreateFood(int num, GameObject type)
    {
        for (int i = 0; i < num; i++)
        {
            GameObject f = Instantiate(type, new Vector3(Random.Range(-rangeX, rangeX), 1f,
                    Random.Range(-rangeZ, rangeZ)) + transform.position,
                Quaternion.Euler(new Vector3(0f, Random.Range(0f, 360f), 90f)));
            f.GetComponent<CooperativeFoodLogic>().respawn = respawnFood;
            f.GetComponent<CooperativeFoodLogic>().myArea = this;
        }
    }

    public void ResetFoodArea(GameObject[] agents)
    {
        foreach (GameObject agent in agents)
        {
            if (agent.transform.parent == gameObject.transform)
            {
                agent.transform.position = new Vector3(Random.Range(-rangeX, rangeX), 2f,
                                               Random.Range(-rangeZ, rangeZ))
                                           + transform.position;
                agent.transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));
            }
        }

        CreateFood(numFood, food);
    }

    public override void ResetArea()
    {
    }
}
