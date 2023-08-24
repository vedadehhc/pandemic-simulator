using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class PersonAgentManager : UnityAgentManager
{
    // Regions
    [SerializeField] protected Region workRegion, schoolRegion, storeRegion, socialRegion;
    [SerializeField] protected GameObject regionPrefab;


    // Settings
    [SerializeField] protected InputField infectionRateInput, recoveryRateInput, simulationSpeedInput, initPopInput, initInfInput;
    [SerializeField] protected Text[] infectionRateDisplay, recoveryRateDisplay, simulationSpeedDisplay;
    [SerializeField] protected Slider infectionRateSlider, recoveryRateSlider, simulationSpeedSlider, initPopSlider, initInfSlider;
    protected NormalRandom randomRecoveryTime;

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


    // Initial Conditions
    [SerializeField] protected int initPop;
    [SerializeField] protected int initInf;


    [HideInInspector] public List<PersonAgent> personAgents = new List<PersonAgent>();

    protected float infectionRate = 0.01f, infectionRadius = 0.2f;

    protected Dictionary<Status, int> pops, prevPops;
    protected List<float> betaList, recTimeList;

    protected List<Region> homeRegions;

    protected int graphUpdateRate;
    protected int modTick;
    protected const int tickDayLength = 24;

    public override void Setup()
    {
        base.Setup();

        CloseAll();

        /// Load Setup and Sim Vars
        UpdateInfectionRateSlider();
        UpdateRecoveryRateSlider();
        UpdateSimulationSpeedSlider();
        UpdateInitPopSlider();
        UpdateInitInfSlider();


        /// Graph Setup
        popGraph.ClearAll();
        popGraph.maxYVal = initPop;
        popGraph.minYVal = 0;

        // S, I, R
        popGraph.AddLineSeries(new Color(0f, 1f, 0f, 0.5f), "Susceptible");
        popGraph.AddLineSeries(new Color(1f, 0f, 0f, 0.5f), "Infected");
        popGraph.AddLineSeries(new Color(0f, 0f, 1f, 0.5f), "Recovered");


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
        workRegion.Setup(true);
        schoolRegion.Setup(true);
        storeRegion.Setup(true);
        socialRegion.Setup(true);


        /// Home Region Setup
        if (homeRegions == null)
        {
            homeRegions = new List<Region>();
        }

        if (homeRegions.Count != (initPop / 4))
        {
            for (int i = (initPop / 4); i < homeRegions.Count;)
            {
                Destroy(homeRegions[i].gameObject);
                homeRegions.RemoveAt(i);
            }

            int colSize = 10;
            float homeScale = 0.1f;
            for (int i = homeRegions.Count; i < (initPop / 4); i++)
            {
                GameObject regionObject = Instantiate(regionPrefab);
                regionObject.transform.parent = gameObject.transform;
                regionObject.transform.localPosition = new Vector3((i / colSize) * (homeScale + infectionRadius) - 5f,
                    (i % colSize) * (homeScale + infectionRadius) - 4.5f, 0f);
                regionObject.transform.localScale = new Vector3(homeScale, homeScale, 1f);
                homeRegions.Add(regionObject.GetComponent<Region>());
                homeRegions[i].Setup(false);
            }
        }


        /// Pops Setup
        pops = new Dictionary<Status, int>();
        pops.Add(Status.Susceptible, initPop);
        pops.Add(Status.Infected, 0);
        pops.Add(Status.Recovered, 0);


        /// Agents Setup
        PersonAgent.manager = this;
        PersonAgent.ClearAll();

        List<int> adultSchedule = new List<int> { 0, 9, 17 };
        List<int> childSchedule = new List<int> { 0, 8, 15 };
        for (int i = 0; i < initPop; i++)
        {
            Region homeRegion = homeRegions[i % homeRegions.Count];
            RandomDefaultSetup(1, defaultAgent, homeRegion);
            PersonAgent agent = agentGameObjectList[i].GetComponent<PersonAgent>();

            Routine routine = new Routine(homeRegion, new List<Region> { homeRegion, workRegion, homeRegion }, adultSchedule);

            if (i % 3 == 2)
            {
                // not adult
                routine = new Routine(homeRegion, new List<Region> { homeRegion, schoolRegion, homeRegion }, childSchedule);
            }

            agent.SetRoutine(routine);
        }

        for (int i = 0; i < initInf && i < agentGameObjectList.Count; i++)
        {
            agentGameObjectList[i].GetComponent<PersonAgent>().SetInfected();
        }

        prevPops = new Dictionary<Status, int>(pops);
        betaList = new List<float>();
        recTimeList = new List<float>();


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
        UpdateDataText();
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
            PersonAgent agent = agentGameObject.GetComponent<PersonAgent>();
            agent.CheckRoutine();
        }

        foreach (GameObject agentGameObject in agentGameObjectList)
        {
            PersonAgent agent = agentGameObject.GetComponent<PersonAgent>();
            agent.CheckInfection();
        }

        foreach (GameObject agentGameObject in agentGameObjectList)
        {
            PersonAgent agent = agentGameObject.GetComponent<PersonAgent>();
            agent.WalkAndUpdate();
        }

        // Parameter Updates
        int totalPop = pops[Status.Susceptible] + pops[Status.Infected] + pops[Status.Recovered];
        if (prevPops[Status.Susceptible] * prevPops[Status.Infected] > 0)
        {
            float beta = ((pops[Status.Susceptible] - prevPops[Status.Susceptible]) * -1f * totalPop)
            / (prevPops[Status.Susceptible] * prevPops[Status.Infected] * 1f);
            betaList.Add(beta);
        }
        // if (prevPops[Status.Infected] > 0)
        // {
        //     float gamma = (1f * (pops[Status.Recovered] - prevPops[Status.Recovered])) / (1f * prevPops[Status.Infected]);
        //     gammaList.Add(gamma);
        // }
        prevPops = new Dictionary<Status, int>(pops);

        // Display updates
        if (tick % graphUpdateRate == 0)
        {
            popGraph.PushValues(new List<float>() { pops[Status.Susceptible], pops[Status.Infected], pops[Status.Recovered] });
            // paramGraph.PushValues(new List<float>() {GetBeta(), GetGamma(), GetR0()});

            UpdateDataText();
        }


        if (started && pops[Status.Infected] == 0 && tick % graphUpdateRate == 0)
        {
            started = false;
            finished = true;
        }

        Tick();
    }

    protected void UpdateDataText()
    {
        dataText.text = pops[Status.Susceptible].ToString() + "\n" + pops[Status.Infected].ToString() +
            "\n" + pops[Status.Recovered].ToString() + "\n\n" + GetBeta().ToString("F4") + "\n" +
            GetGamma().ToString("F4") + "\n" + GetR0().ToString("F3");
    }

    protected float GetBeta()
    {
        if (betaList.Count == 0)
        {
            return 0f;
        }
        return betaList.Average();
    }

    public void UpdateRecTimeList(float recTime) {
        recTimeList.Add(recTime);
    }

    protected float GetGamma()
    {
        if (recTimeList.Count == 0)
        {
            return 0f;
        }
        return 1f / recTimeList.Average();
    }

    protected float GetR0()
    {
        if (GetBeta() * GetGamma() == 0f)
        {
            return 0f;
        }
        return GetBeta() / GetGamma();
    }

    public void UpdateAgentStatus(Status prevStatus, Status status)
    {
        // TODO: write this
        pops[prevStatus]--;
        pops[status]++;
    }

    public int GetRecoveryTime()
    {
        return Mathf.Max(Mathf.RoundToInt(randomRecoveryTime.NextFloat()), 0);
    }

    public Region GetStoreRegion()
    {
        return storeRegion;
    }

    public Region GetSocialRegion()
    {
        return socialRegion;
    }

    public float GetInfectionRadius()
    {
        return infectionRadius;
    }

    public float GetInfectionRate()
    {
        return infectionRate;
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

    public void ResetSettings()
    {
        infectionRateSlider.value = .01f;
        recoveryRateSlider.value = 14f;
        simulationSpeedSlider.value = 20f;
        initPopSlider.value = 100f;
        initInfSlider.value = 1f;

        UpdateInfectionRateSlider();
        UpdateRecoveryRateSlider();
        UpdateSimulationSpeedSlider();
        UpdateInitPopSlider();
        UpdateInitInfSlider();
    }

    public void UpdateInfectionRate()
    {
        infectionRate = Mathf.Clamp(float.Parse("0" + infectionRateInput.text), infectionRateSlider.minValue, infectionRateSlider.maxValue);
        infectionRateInput.text = infectionRate.ToString("F4");
        infectionRateSlider.value = infectionRate;
        foreach (Text t in infectionRateDisplay)
        {
            t.text = "Infection rate: " + infectionRate.ToString("F4");
        }
    }

    public void UpdateInfectionRateSlider()
    {
        infectionRate = infectionRateSlider.value;
        infectionRateInput.text = infectionRate.ToString("F4");
        foreach (Text t in infectionRateDisplay)
        {
            t.text = "Infection rate: " + infectionRate.ToString("F4");
        }
    }

    public void UpdateRecoveryRate()
    {
        float recoveryRate = Mathf.Clamp(float.Parse("0" + recoveryRateInput.text), recoveryRateSlider.minValue, recoveryRateSlider.maxValue);
        recoveryRateInput.text = recoveryRate.ToString("F1");
        recoveryRateSlider.value = recoveryRate;
        randomRecoveryTime = new NormalRandom(recoveryRate * tickDayLength, 2f * tickDayLength);
        foreach (Text t in recoveryRateDisplay)
        {
            t.text = "Average Recovery Time (days): " + recoveryRate.ToString("F1");
        }
    }

    public void UpdateRecoveryRateSlider()
    {
        float recoveryRate = recoveryRateSlider.value;
        recoveryRateInput.text = recoveryRate.ToString("F1");
        randomRecoveryTime = new NormalRandom(recoveryRate * tickDayLength, 2f * tickDayLength);
        foreach (Text t in recoveryRateDisplay)
        {
            t.text = "Average Recovery Time (days): " + recoveryRate.ToString("F1");
        }
    }

    public void UpdateSimulationSpeed()
    {
        float raw = Mathf.Clamp(float.Parse("0" + simulationSpeedInput.text), simulationSpeedSlider.minValue, simulationSpeedSlider.maxValue);
        simulationSpeedSlider.value = raw;
        simulationSpeedInput.text = simulationSpeedSlider.value.ToString("F2");
        simulationSpeed = 1f / raw;
        foreach (Text t in simulationSpeedDisplay)
        {
            t.text = "Simulation Speed: " + raw.ToString("F2");
        }
    }

    public void UpdateSimulationSpeedSlider()
    {
        simulationSpeed = 1f / simulationSpeedSlider.value;
        simulationSpeedInput.text = simulationSpeedSlider.value.ToString("F2");
        foreach (Text t in simulationSpeedDisplay)
        {
            t.text = "Simulation Speed: " + simulationSpeedSlider.value.ToString("F2");
        }
    }

    public void UpdateInitPop()
    {
        initPop = Mathf.RoundToInt(Mathf.Clamp(int.Parse("0" + initPopInput.text), initPopSlider.minValue, initPopSlider.maxValue));
        initPopInput.text = initPop.ToString();
        initPopSlider.value = initPop;
        initInfSlider.maxValue = initPop;
        if (initInf > initPop)
        {
            initInf = initPop;
            initInfSlider.value = initInf;
            initInfInput.text = initInf.ToString();
        }
    }

    public void UpdateInitPopSlider()
    {
        initPop = Mathf.RoundToInt(initPopSlider.value);
        initPopInput.text = initPop.ToString();
        initInfSlider.maxValue = initPop;
        if (initInf > initPop)
        {
            initInf = initPop;
            initInfSlider.value = initInf;
            initInfInput.text = initInf.ToString();
        }
    }

    public void UpdateInitInf()
    {
        initInf = Mathf.RoundToInt(Mathf.Clamp(int.Parse("0" + initInfInput.text), initInfSlider.minValue, initInfSlider.maxValue));
        initInfInput.text = initInf.ToString();
        initInfSlider.value = initInf;
    }

    public void UpdateInitInfSlider()
    {
        initInf = Mathf.RoundToInt(initInfSlider.value);
        initInfInput.text = initInf.ToString();
    }

    public void ShowPopGraph()
    {
        if (popGraph.gameObject.activeSelf)
        {
            popGraph.gameObject.SetActive(false);
            popGraphImage.color = inactiveColor;
            simulationImage.color = activeColor;
        }
        else
        {
            CloseAll();
            popGraph.gameObject.SetActive(true);
            simulationImage.color = inactiveColor;
            popGraphImage.color = activeColor;
        }
    }

    // public void ShowParGraph()
    // {
    //     if (paramGraph.gameObject.activeSelf)
    //     {
    //         paramGraph.gameObject.SetActive(false);
    //         paramGraphImage.color = inactiveColor;
    //         simulationImage.color = activeColor;
    //     }
    //     else
    //     {
    //         CloseAll();
    //         paramGraph.gameObject.SetActive(true);
    //         simulationImage.color = inactiveColor;
    //         paramGraphImage.color = activeColor;
    //     }
    // }

    public void ShowSettings()
    {
        if (settingsObject.activeSelf)
        {
            settingsObject.SetActive(false);
            settingsImage.color = inactiveColor;
            simulationImage.color = activeColor;
        }
        else
        {
            CloseAll();
            settingsObject.SetActive(true);
            simulationImage.color = inactiveColor;
            settingsImage.color = activeColor;
        }
    }

    public void CloseAll()
    {
        popGraph.gameObject.SetActive(false);
        // paramGraph.gameObject.SetActive(false);
        settingsObject.SetActive(false);
        popGraphImage.color = inactiveColor;
        // paramGraphImage.color = inactiveColor;
        settingsImage.color = inactiveColor;
        simulationImage.color = activeColor;
    }
}