using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NormalRandom
{
    [SerializeField] protected float mean, stdDev;

    public NormalRandom(float mean, float dev) {
        this.mean = mean;
        this.stdDev = Mathf.Abs(dev);
    }

    public float NextFloat() {
        float ans = NextStandardFloat() * stdDev + mean;

        if(ans > mean + 3*stdDev) {
            ans = mean + 3*stdDev;
        }

        if(ans < mean - 3*stdDev) {
            ans = mean - 3*stdDev;
        }

        return ans;
    }

    public float NextStandardFloat()
    {
        float u, v, S;

        do
        {
            u = 2.0f * Random.value - 1.0f;
            v = 2.0f * Random.value - 1.0f;
            S = u * u + v * v;
        }
        while (S >= 1.0);

        float fac = Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);
        return u * fac;
    }
}