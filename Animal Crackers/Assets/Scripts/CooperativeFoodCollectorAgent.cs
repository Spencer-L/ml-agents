using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

public class CooperativeFoodCollectorAgent : Agent
{
    private static readonly int Speed = Animator.StringToHash("Speed");
    public GameObject area;
    CooperativeFoodCollectorArea m_MyArea;
    bool m_Poisoned;
    bool m_Satiated;
    float m_EffectTime;
    Rigidbody m_AgentRb;

    [SerializeField] private Animator anim;

    [Header("Agent Parameters")]
    // Lifespan of agent in milliseconds
    [Tooltip("Lifespan of agent in milliseconds")]
    public float agentLifespan = 120000f;
    // Lifespan of agent without food.
    [Tooltip("Lifespan of agent without food in seconds")]
    public float starvationResistance = 10f;
    // Speed of agent rotation.
    public float turnSpeed = 200;
    // Speed of agent movement.
    public float moveSpeed = 1;

    // Birth time of agent.
    private float m_BirthTime;
    // Time since last food.
    private float m_TimeSinceLastFood;

    [Header("Materials")]
    public Material normalMaterial;
    public Material badMaterial;
    public Material goodMaterial;
    public Renderer indicatorRend;

    public bool useVectorObs;
    public int generationNumber = 0;

    EnvironmentParameters m_ResetParams;

    // Global settings
    [SerializeField] private CooperativeFoodCollectorSettings m_CooperativeFoodCollectorSettings;
    private CooperativeFoodEnvController m_CooperativeFoodEnvController;
    private QueenLogic m_queenLogic;

    private void Start()
    {
        m_CooperativeFoodCollectorSettings = FindFirstObjectByType<CooperativeFoodCollectorSettings>();
        m_CooperativeFoodEnvController = FindFirstObjectByType<CooperativeFoodEnvController>();
        m_queenLogic = FindFirstObjectByType<QueenLogic>();
        area = GameObject.Find("Ground");
    }

    public override void Initialize()
    {
        m_AgentRb = GetComponent<Rigidbody>();
        area = GameObject.Find("Ground");
        m_MyArea = area.GetComponent<CooperativeFoodCollectorArea>();
        m_ResetParams = Academy.Instance.EnvironmentParameters;
        m_BirthTime = Time.time;
        SetResetParameters();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (useVectorObs)
        {
            var localVelocity = transform.InverseTransformDirection(m_AgentRb.velocity);
            sensor.AddObservation(localVelocity.x);
            sensor.AddObservation(localVelocity.z);
        }
        sensor.AddObservation(m_CooperativeFoodCollectorSettings.foodStored);
        sensor.AddObservation(m_queenLogic.isCoolingDown);
    }

    public Color32 ToColor(int hexVal)
    {
        var r = (byte)((hexVal >> 16) & 0xFF);
        var g = (byte)((hexVal >> 8) & 0xFF);
        var b = (byte)(hexVal & 0xFF);
        return new Color32(r, g, b, 255);
    }

    public void MoveAgent(ActionBuffers actionBuffers)
    {
        if (Time.time > m_EffectTime + 0.5f)
        {
            if (m_Poisoned)
            {
                Unpoison();
            }
            if (m_Satiated)
            {
                Unsatiate();
            }
        }

        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var continuousActions = actionBuffers.ContinuousActions;
        // var discreteActions = actionBuffers.DiscreteActions;


        var forward = Mathf.Clamp(continuousActions[0], -1f, 1f);
        var right = Mathf.Clamp(continuousActions[1], -1f, 1f);
        var rotate = Mathf.Clamp(continuousActions[2], -1f, 1f);

        dirToGo = transform.forward * forward;
        dirToGo += transform.right * right;
        rotateDir = -transform.up * rotate;


        m_AgentRb.AddForce(dirToGo * moveSpeed, ForceMode.VelocityChange);
        if(anim) anim.SetFloat(Speed, m_AgentRb.velocity.sqrMagnitude);
        transform.Rotate(rotateDir, Time.fixedDeltaTime * turnSpeed);

        if (m_AgentRb.velocity.sqrMagnitude > 25f) // slow it down
        {
            m_AgentRb.velocity *= 0.95f;
        }
    }

    void Poison()
    {
        m_Poisoned = true;
        m_EffectTime = Time.time;
        indicatorRend.material = badMaterial;
    }

    void Unpoison()
    {
        m_Poisoned = false;
        indicatorRend.material = normalMaterial;
    }

    void Satiate()
    {
        m_Satiated = true;
        m_EffectTime = Time.time;
        m_TimeSinceLastFood = 0;
        indicatorRend.material = goodMaterial;
    }

    void Unsatiate()
    {
        m_Satiated = false;
        indicatorRend.material = normalMaterial;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)

    {
        MoveAgent(actionBuffers);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        if (Input.GetKey(KeyCode.D))
        {
            continuousActionsOut[2] = 1;
        }
        if (Input.GetKey(KeyCode.W))
        {
            continuousActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            continuousActionsOut[2] = -1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            continuousActionsOut[0] = -1;
        }
        // var discreteActionsOut = actionsOut.DiscreteActions;
        // discreteActionsOut[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }

    public override void OnEpisodeBegin()
    {
        Unpoison();
        Unsatiate();
        m_AgentRb.velocity = Vector3.zero;
        transform.position = new Vector3(Random.Range(-m_MyArea.rangeX, m_MyArea.rangeX),
            2f, Random.Range(-m_MyArea.rangeZ, m_MyArea.rangeZ))
            + area.transform.position;
        transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));

        SetResetParameters();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("food"))
        {
            collision.gameObject.GetComponent<CooperativeFoodLogic>().OnEaten(this);
            Satiate();
            AddReward(10f);
        }

        if (collision.gameObject.CompareTag("queen"))
        {
            if (collision.gameObject.GetComponent<QueenLogic>().HandleReproduceRequest(gameObject))
            {
                AddReward(5f);
            }
        }

        if (collision.gameObject.CompareTag("outOfBounds"))
        {
            // AddReward(-10f);
            m_CooperativeFoodEnvController.OnAgentDeath(this);
        }

        // if (collision.gameObject.CompareTag("breakableWall"))
        // {
        //     bool wallBroken = collision.gameObject.GetComponent<BreakableWallController>().BreakWallCheck();
        // }
        // if (collision.gameObject.CompareTag("badFood"))
        // {
        //     Poison();
        //     collision.gameObject.GetComponent<FoodLogic>().OnEaten();
        //
        //     AddReward(-1f);
        //     if (contribute)
        //     {
        //         m_FoodCollecterSettings.totalScore -= 1;
        //     }
        // }
    }

    private void Update()
    {
        // penalize agent for not getting food over time
        // float timePenalty = -0.001f * StepCount; // Increase penalty over time
        // AddReward(timePenalty);

        // Handle Agent lifespan
        if (Time.time > m_BirthTime + agentLifespan)
        {
            m_CooperativeFoodEnvController.OnAgentDeath(this);
            Debug.Log(transform.name + " died of old age.  It was born " + m_BirthTime + " and died" + Time.time);
        }

        // Handle Agent starvation
        m_TimeSinceLastFood += Time.deltaTime;
        if (m_TimeSinceLastFood > starvationResistance)
        {
            AddReward(-50f);
            m_CooperativeFoodEnvController.OnAgentDeath(this);
            Debug.Log(transform.name + " died of starvation.  Time since last food: " + m_TimeSinceLastFood);
        }

        // Debug.Log(transform.name + " Time since last food: " + m_TimeSinceLastFood);
    }

    public void SetAgentScale()
    {
        float agentScale = m_ResetParams.GetWithDefault("agent_scale", 1.0f);
        gameObject.transform.localScale = new Vector3(agentScale, agentScale, agentScale);
    }

    public void SetResetParameters()
    {
        SetAgentScale();
    }
}
