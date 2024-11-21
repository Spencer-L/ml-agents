using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;

public class CooperativeFoodCollectorSettings : MonoBehaviour
{
    [HideInInspector]
    public GameObject[] agents;
    [HideInInspector]
    public CooperativeFoodCollectorArea[] listArea;

    [SerializeField] private CooperativeFoodEnvController m_CooperativeFoodEnvController;

    public int totalFoodCollected, foodStored;
    public TextMeshProUGUI scoreText;

    /// <summary>
    /// The spawn area margin multiplier.
    /// ex: .9 means 90% of spawn area will be used.
    /// .1 margin will be left (so players don't spawn off of the edge).
    /// The higher this value, the longer training time required.
    /// </summary>
    public float spawnAreaMarginMultiplier = 0.9f;

    StatsRecorder m_Recorder;

    public void Awake()
    {
        Academy.Instance.OnEnvironmentReset += EnvironmentReset;
        m_Recorder = Academy.Instance.StatsRecorder;
    }

    public void EnvironmentReset()
    {
        // Reinstantiate dead agents
        while (m_CooperativeFoodEnvController.AgentsList.Count < 3)
        {
            // Create new agent
            var newAgent = m_CooperativeFoodEnvController.SpawnAgent();
            m_CooperativeFoodEnvController.RegisterNewAgent(newAgent);
        }

        ClearObjects(GameObject.FindGameObjectsWithTag("food"));

        agents = GameObject.FindGameObjectsWithTag("agent");
        listArea = FindObjectsOfType<CooperativeFoodCollectorArea>();
        foreach (var fa in listArea)
        {
            fa.ResetFoodArea(agents);
        }

        foreach (var breakableWall in FindObjectsByType<BreakableWallController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            breakableWall.ResetWall();
        }

        totalFoodCollected = 0;
        foodStored = 0;
    }

    void ClearObjects(GameObject[] objects)
    {
        foreach (var food in objects)
        {
            Destroy(food);
        }
    }

    public void Update()
    {
        scoreText.text = $"Total Food: {totalFoodCollected} \nFood Stored: {foodStored} \nAgent Population: {m_CooperativeFoodEnvController.totalAgents}";

        // Send stats via SideChannel so that they'll appear in TensorBoard.
        // These values get averaged every summary_frequency steps, so we don't
        // need to send every Update() call.
        if ((Time.frameCount % 100) == 0)
        {
            m_Recorder.Add("TotalFood", totalFoodCollected);
            m_Recorder.Add("FoodStored", foodStored);
            m_Recorder.Add("AgentPopulation", m_CooperativeFoodEnvController.totalAgents);
        }
    }
}
