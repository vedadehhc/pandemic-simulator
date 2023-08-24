using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonAgentVideo : RandomAgent
{
    public static PersonAgentManagerVideo manager;
    protected static int storeTime = 2, socialTime = 4;
    protected Status infectionStatus; // 0 = s, 1 = i, 2 = r
    protected bool updateInfection;
    protected Routine routine;
    protected int lastStore, lastSocial;
    protected int recoveryTime, infectedPeriod;
    protected int curRegion, nextRegion;

    public override void Setup()
    {
        base.Setup();
        manager.personAgents.Add(this);
        if (routine != null)
        {
            curRegion = 0;
            nextRegion = (curRegion + 1) % routine.regionList.Count;
        }
        lastStore = Mathf.RoundToInt(Random.value * -4f * manager.GetTickDayLength());
        lastSocial = Mathf.RoundToInt(Random.value * -1f * manager.GetTickDayLength());

        recoveryTime = 0;
        infectedPeriod = 0;
    }

    public static void ClearAll()
    {
        manager.personAgents = new List<PersonAgentVideo>();
    }

    protected void ChangeRegion(Region newRegion)
    {
        if (newRegion != null && newRegion.open)
        {
            this.region = newRegion;
            gameObject.transform.position = new Vector3(this.region.RandomXCor(), this.region.RandomYCor(), 0f);
        }
        else if (this.region != routine.defaultRegion)
        {
            this.region = routine.defaultRegion;
            gameObject.transform.position = new Vector3(this.region.RandomXCor(), this.region.RandomYCor(), 0f);
        }
    }

    public void SetInfected()
    {
        manager.UpdateAgentStatus(Status.Susceptible, Status.Infected);
        infectionStatus = Status.Infected;
        SetColor(new Color(1f, 0f, 0f, 1f));
    }

    public void SetRoutine(Routine routine)
    {
        this.routine = routine;
        if (routine != null)
        {
            curRegion = 0;
            nextRegion = (curRegion + 1) % routine.regionList.Count;
        }
    }

    public void CheckRoutine()
    {
        if (routine != null)
        {
            
        }
    }

    public void CheckInfection()
    {
        if (infectionStatus == Status.Infected)
        {
            foreach (PersonAgentVideo pa in manager.personAgents)
            {
                if (pa.infectionStatus == Status.Susceptible)
                {
                    this.Infect(pa);
                }
            }

            if (Random.value < (manager.GetRecoveryRate()))
            {
                this.updateInfection = true;
            }
        }
    }

    protected void Infect(PersonAgentVideo pa)
    {
        if (this.region == pa.region &&
            Vector3.Distance(pa.gameObject.transform.position, this.gameObject.transform.position) < manager.GetInfectionRadius()
            && Random.value < manager.GetInfectionRate())
        {
            pa.updateInfection = true;
        }
    }

    public void WalkAndUpdate()
    {
        RandomWalk();
        if (updateInfection)
        {
            if (infectionStatus == Status.Susceptible)
            {
                infectionStatus = Status.Infected;
                SetColor(new Color(1f, 0f, 0f, 1f));
                manager.UpdateAgentStatus(Status.Susceptible, Status.Infected);
            }
            else if (infectionStatus == Status.Infected)
            {
                infectionStatus = Status.Recovered;
                SetColor(new Color(0f, 0f, 1f, 1f));
                manager.UpdateAgentStatus(Status.Infected, Status.Recovered);
            }
        }
        updateInfection = false;
    }
}