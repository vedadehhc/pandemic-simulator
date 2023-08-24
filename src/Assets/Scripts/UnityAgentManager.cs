using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UnityAgentManager : MonoBehaviour
{
    [SerializeField] protected Text tickText;
    public GameObject defaultAgent;
    public Region defaultRegion;
    [SerializeField] protected Sprite defaultShape, altShape;
    [SerializeField] protected Color defaultColor;
    [SerializeField] protected float defaultScale;
    protected List<GameObject> agentGameObjectList;


    // Controls
    [SerializeField] protected Text goRepText;

    protected float simulationSpeed;
    protected int tick;

    protected bool started, finished;

    protected void Awake()
    {
        agentGameObjectList = new List<GameObject>();
    }
    protected void Start()
    {
        Setup();
    }

    protected void Update()
    {
        if (started && !finished)
        {
            goRepText.text = "Stop";
        }
        else
        {
            goRepText.text = "Simulate Continously";
        }
    }

    protected virtual void ClearAll()
    {
        foreach (GameObject go in agentGameObjectList)
        {
            Destroy(go);
        }
        agentGameObjectList.Clear();
        tick = 0;
        tickText.text = "Tick: " + tick;
    }

    public virtual void Setup()
    {
        ClearAll();
        started = false;
        finished = false;
    }

    protected void RandomDefaultSetup(int num)
    {
        for (int i = 0; i < num; i++)
        {
            CreateAgent(new Vector2(defaultRegion.RandomXCor(), defaultRegion.RandomYCor()));
        }
    }

    protected void RandomDefaultSetup(int num, GameObject agent, Region region)
    {
        for (int i = 0; i < num; i++)
        {
            CreateAgent(agent, new Vector2(region.RandomXCor(), region.RandomYCor()), region, defaultColor, defaultShape, defaultScale);
        }
    }

    public void GoWrapper()
    {
        if (finished) return;
        if (started) return;
        Go();
    }

    protected virtual void Go()
    {
        foreach (GameObject agentGameObject in agentGameObjectList)
        {
            UnityAgent agent = agentGameObject.GetComponent<UnityAgent>();
            agent.Go();
        }
        Tick();
    }

    public int GetTick()
    {
        return tick;
    }

    protected virtual void Tick()
    {
        tick++;
        tickText.text = "Tick: " + tick;
    }

    public void GoRepeatedWrapper()
    {
        if (finished) return;
        started = !started;
        if (started)
        {
            StartCoroutine(GoRepeated());
        }
    }

    protected IEnumerator GoRepeated()
    {
        while (started)
        {
            Go();
            yield return new WaitForSeconds(simulationSpeed);
        }
    }

    public UnityAgent CreateAgent(Vector2 position)
    {
        return CreateAgent(defaultAgent, position, defaultRegion, defaultColor, defaultShape, defaultScale);
    }

    public UnityAgent CreateAgent(GameObject agentPrefab, Vector2 position, Region region, Color color, Sprite shape, float scale)
    {
        int id = agentGameObjectList.Count;
        GameObject agentGameObject = Instantiate(agentPrefab);
        agentGameObject.transform.position = new Vector3(position.x, position.y, 0);
        agentGameObject.transform.parent = transform;

        UnityAgent agent = agentGameObject.GetComponent<UnityAgent>();
        agent.Initialize(id, region, color, shape, scale);

        agentGameObjectList.Add(agentGameObject);
        return agent;
    }
}