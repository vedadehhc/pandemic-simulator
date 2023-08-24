using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Routine
{
    public Region defaultRegion;
    public List<Region> regionList;
    public List<int> swapTimes;

    public Routine (Region defaultRegion, List<Region> regionList, List<int> swapTimes) {
        this.defaultRegion = defaultRegion;
        this.regionList = regionList;
        this.swapTimes = swapTimes;
    }
}