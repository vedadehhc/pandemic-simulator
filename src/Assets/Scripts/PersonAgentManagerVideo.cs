using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class PersonAgentManagerVideo : UnityAgentManager
{
    // Regions
    [SerializeField] protected Region mainRegion;
    [SerializeField] protected GameObject regionPrefab;


    // [SerializeField] protected Text[] parameterText;
    [SerializeField] protected Text dataText;


    // Infection Radius Disks
    [SerializeField] protected GameObject radiusObject;
    [SerializeField] protected bool showDisks;

    // Toolbar UI
    [SerializeField] protected Color activeColor, inactiveColor;
    [SerializeField] protected Image simulationImage, popGraphImage /*, paramGraphImage */, settingsImage;
    [SerializeField] protected GraphWindow popGraph/*, paramGraph*/;
    [SerializeField] protected GameObject settingsObject;


    [SerializeField] protected float simSpeedEdit = 0.1f;
    // Initial Conditions
    [SerializeField] protected int initPop = 300;
    [SerializeField] protected int initInf = 1;


    [HideInInspector] public List<PersonAgentVideo> personAgents = new List<PersonAgentVideo>();

    [SerializeField] protected float infectionRate = 0.01f, infectionRadius = 0.2f, recoveryTime = 14f;

    protected Dictionary<Status, int> pops, prevPops;

    protected List<Region> homeRegions;

    protected int graphUpdateRate;
    protected int modTick;
    protected const int tickDayLength = 24;

    public override void Setup()
    {
        base.Setup();
        simulationSpeed = simSpeedEdit;

        /// Graph Setup
        popGraph.ClearAll();
        popGraph.maxYVal = initPop;
        popGraph.minYVal = 0;

        // S, I, R
        popGraph.AddLineSeries(new Color(0f, 1f, 0f, 0.5f), "Susceptible");
        popGraph.AddLineSeries(new Color(1f, 0f, 0f, 0.5f), "Infected");
        popGraph.AddLineSeries(new Color(0f, 0f, 1f, 0.5f), "Recovered");

        popGraph.hasDots = false;


        // // Set tooltip/label format
        // paramGraph.SetAxisLabelY(delegate (float _f) { return (_f).ToString("F3"); });
        // paramGraph.ClearAll();
        // paramGraph.maxYVal = 1;
        // paramGraph.minYVal = 0;

        // // B, C, R0
        // paramGraph.AddLineSeries(new Color(1f, 0f, 0f, 0.5f));
        // paramGraph.AddLineSeries(new Color(0f, 1f, 0f, 0.5f));
        // paramGraph.AddLineSeries(new Color(0f, 0f, 1f, 0.5f));


        /// Main Region Setup
        mainRegion.Setup(false);



        /// Pops Setup
        pops = new Dictionary<Status, int>();
        pops.Add(Status.Susceptible, initPop);
        pops.Add(Status.Infected, 0);
        pops.Add(Status.Recovered, 0);


        /// Agents Setup
        PersonAgentVideo.manager = this;
        PersonAgentVideo.ClearAll();
        Debug.Log(initPop);
        for (int i = 0; i < initPop; i++)
        {
            Region homeRegion = mainRegion;
            RandomDefaultSetup(1, defaultAgent, homeRegion);
        }

        for (int i = 0; i < initInf && i < agentGameObjectList.Count; i++)
        {
            agentGameObjectList[i].GetComponent<PersonAgentVideo>().SetInfected();
        }

        prevPops = new Dictionary<Status, int>(pops);


        /// Disks Setup
        if (showDisks)
        {
            for (int i = 0; i < agentGameObjectList.Count; i++)
            {
                GameObject disk = Instantiate(radiusObject);
                disk.transform.parent = agentGameObjectList[i].transform;
                disk.transform.localPosition = new Vector3(0, 0, 0);
                disk.transform.localScale = new Vector3(2 * infectionRadius / defaultScale, 2 * infectionRadius / defaultScale, 1);
                // disk.transform.SetAsLastSibling();
            }
        }

        graphUpdateRate = tickDayLength;

        tickText.text = "Day: " + (tick / tickDayLength) + "\nHour: " + modTick;
    }

    protected override void ClearAll()
    {
        base.ClearAll();
        modTick = 0;
    }

    public void GoDayWrapper()
    {
        if (finished || started) return;
        started = true;
        StartCoroutine(GoDay());
    }

    protected IEnumerator GoDay()
    {
        for (int i = 0; i < tickDayLength && started; i++)
        {
            Go();
            yield return new WaitForSeconds(simulationSpeed);
        }
        started = false;
    }

    public void GoWeekWrapper()
    {
        if (finished || started) return;
        started = true;
        StartCoroutine(GoWeek());
    }

    protected IEnumerator GoWeek()
    {
        for (int i = 0; i < 7 * tickDayLength && started; i++)
        {
            Go();
            yield return new WaitForSeconds(simulationSpeed);
        }
        started = false;
    }

    protected override void Go()
    {
        foreach (GameObject agentGameObject in agentGameObjectList)
        {
            PersonAgentVideo agent = agentGameObject.GetComponent<PersonAgentVideo>();
            agent.CheckRoutine();
        }

        foreach (GameObject agentGameObject in agentGameObjectList)
        {
            PersonAgentVideo agent = agentGameObject.GetComponent<PersonAgentVideo>();
            agent.CheckInfection();
        }

        foreach (GameObject agentGameObject in agentGameObjectList)
        {
            PersonAgentVideo agent = agentGameObject.GetComponent<PersonAgentVideo>();
            agent.WalkAndUpdate();
        }

        if (tick % graphUpdateRate == 0)
        {
            popGraph.PushValues(new List<float>() { pops[Status.Susceptible], pops[Status.Infected], pops[Status.Recovered] });
            // paramGraph.PushValues(new List<float>() {GetBeta(), GetGamma(), GetR0()});

        }

        if (started && pops[Status.Infected] == 0 && tick % graphUpdateRate == 0)
        {
            started = false;
            finished = true;
        }

        Tick();
    }

    public void UpdateAgentStatus(Status prevStatus, Status status)
    {
        // TODO: write this
        pops[prevStatus]--;
        pops[status]++;
    }

    public float GetInfectionRadius()
    {
        return infectionRadius;
    }

    public float GetInfectionRate()
    {
        return infectionRate;
    }

    public float GetRecoveryRate()
    {
        return 1f / (recoveryTime * 24f);
    }

    public int GetModTick()
    {
        return modTick;
    }

    public int GetTickDayLength()
    {
        return tickDayLength;
    }

    protected override void Tick()
    {
        tick++;
        modTick++;
        if (modTick >= tickDayLength)
        {
            modTick -= tickDayLength;
        }
        tickText.text = "Day: " + (tick / tickDayLength) + "\nHour: " + modTick;
    }
}