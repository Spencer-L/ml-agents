using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class CooperativeFoodEnvController : MonoBehaviour
{
[System.Serializable]
    public class PlayerInfo
    {
        public CooperativeFoodCollectorAgent Agent;
        [HideInInspector]
        public Vector3 StartingPos;
        [HideInInspector]
        public Quaternion StartingRot;
        [HideInInspector]
        public Rigidbody Rb;
    }

    /// <summary>
    /// Max Academy steps before this platform resets
    /// </summary>
    [Header("Max Environment Steps")] public int MaxEnvironmentSteps = 25000;

    /// <summary>
    /// The area bounds.
    /// </summary>
    [HideInInspector]
    public Bounds areaBounds;
    /// <summary>
    /// The ground. The bounds are used to spawn the elements.
    /// </summary>
    public GameObject ground;

    public GameObject agentPrefab;

    //List of Agents On Platform
    public List<PlayerInfo> AgentsList = new List<PlayerInfo>();

    public bool UseRandomAgentRotation = true;
    public bool UseRandomAgentPosition = true;
    public CooperativeFoodCollectorSettings m_CooperativeFoodCollectorSettings;

    private SimpleMultiAgentGroup m_AgentGroup;

    private int m_ResetTimer;

    void Start()
    {
        // Get the ground's bounds
        areaBounds = ground.GetComponent<Collider>().bounds;
        m_CooperativeFoodCollectorSettings = FindObjectOfType<CooperativeFoodCollectorSettings>();
        // Initialize TeamManager
        m_AgentGroup = new SimpleMultiAgentGroup();
        foreach (var item in AgentsList)
        {
            item.StartingPos = item.Agent.transform.position;
            item.StartingRot = item.Agent.transform.rotation;
            item.Rb = item.Agent.GetComponent<Rigidbody>();
            m_AgentGroup.RegisterAgent(item.Agent);
        }
        ResetScene();
    }

    void FixedUpdate()
    {
        m_ResetTimer += 1;
        if (m_ResetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            m_AgentGroup.GroupEpisodeInterrupted();
            ResetScene();
        }

        //Hurry Up Penalty
        m_AgentGroup.AddGroupReward(-0.5f / MaxEnvironmentSteps);
    }

    /// <summary>
    /// Use the ground's bounds to pick a random spawn position.
    /// </summary>
    public Vector3 GetRandomSpawnPos()
    {
        var foundNewSpawnLocation = false;
        var randomSpawnPos = Vector3.zero;
        while (foundNewSpawnLocation == false)
        {
            var randomPosX = Random.Range(-areaBounds.extents.x * m_CooperativeFoodCollectorSettings.spawnAreaMarginMultiplier,
                areaBounds.extents.x * m_CooperativeFoodCollectorSettings.spawnAreaMarginMultiplier);

            var randomPosZ = Random.Range(-areaBounds.extents.z * m_CooperativeFoodCollectorSettings.spawnAreaMarginMultiplier,
                areaBounds.extents.z * m_CooperativeFoodCollectorSettings.spawnAreaMarginMultiplier);
            randomSpawnPos = ground.transform.position + new Vector3(randomPosX, 1f, randomPosZ);
            if (Physics.CheckBox(randomSpawnPos, new Vector3(1.5f, 0.01f, 1.5f)) == false)
            {
                foundNewSpawnLocation = true;
            }
        }
        return randomSpawnPos;
    }

    /// <summary>
    /// Called when an agent collects a food.
    /// </summary>
    public void OnFoodEaten(CooperativeFoodLogic foodLogic, CooperativeFoodCollectorAgent agent)
    {
        //Give Agent Rewards
        m_AgentGroup.AddGroupReward(1f);
        m_CooperativeFoodCollectorSettings.totalFoodCollected += 1;
        m_CooperativeFoodCollectorSettings.foodStored += 1;
    }

    /// <summary>
    /// Called when the agent moves the block into the goal.
    /// </summary>
    public void OnWallBroken(BreakableWallController wallController)
    {
        //Give Agent Rewards
        m_AgentGroup.AddGroupReward(10f);
    }

    public void OnQueenReproduce()
    {
        //Give Agent Rewards
        m_AgentGroup.AddGroupReward(10f);

        m_CooperativeFoodCollectorSettings.foodStored -= 10;

        // Spawn New Agent
        var pos = GetRandomSpawnPos();
        var rot = GetRandomRot();
        var newAgent = Instantiate(agentPrefab, pos, rot);
        var newAgentComponent = newAgent.GetComponent<CooperativeFoodCollectorAgent>();
        newAgentComponent.isSpawned = true;
        m_AgentGroup.RegisterAgent(newAgentComponent);
        AgentsList.Add(new PlayerInfo
        {
            Agent = newAgentComponent,
            StartingPos = pos,
            StartingRot = rot,
            Rb = newAgentComponent.GetComponent<Rigidbody>()
        });
    }

    Quaternion GetRandomRot()
    {
        return Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0);
    }

    public void ResetScene()
    {
        m_ResetTimer = 0;

        //Reset Agents
        foreach (var item in AgentsList)
        {
            if(item.Agent.isSpawned)
            {
                AgentsList.Remove(item);
                m_AgentGroup.UnregisterAgent(item.Agent);
                Destroy(item.Agent.gameObject);
                continue;
            }
            var pos = UseRandomAgentPosition ? GetRandomSpawnPos() : item.StartingPos;
            var rot = UseRandomAgentRotation ? GetRandomRot() : item.StartingRot;

            item.Agent.transform.SetPositionAndRotation(pos, rot);
            item.Rb.velocity = Vector3.zero;
            item.Rb.angularVelocity = Vector3.zero;
        }

        // Reset Walls
        foreach (var breakableWall in FindObjectsByType<BreakableWallController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            breakableWall.ResetWall();
        }

        // End Episode
        m_AgentGroup.EndGroupEpisode();

        m_CooperativeFoodCollectorSettings.EnvironmentReset();
    }
}
