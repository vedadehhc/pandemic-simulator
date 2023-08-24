using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAgent : UnityAgent
{
    private float step = 0.1f;

    public override void Setup()
    {
        gameObject.transform.localEulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));
        gameObject.transform.SetAsLastSibling();
    }

    public override void Go()
    {
        RandomWalk();
    }
    
    protected void RandomWalk() {
        gameObject.transform.localEulerAngles += new Vector3(0f, 0f, Random.Range(-30f, 30f));

        Vector3 newPosI = gameObject.transform.position + gameObject.transform.right * step;
        Vector3 newPos = new Vector3(0f, 0f, 0f);
        newPos.x = Mathf.Clamp(newPosI.x, this.region.XMin(), this.region.XMax());
        newPos.y = Mathf.Clamp(newPosI.y, this.region.YMin(), this.region.YMax());

        if (newPos != newPosI)
        {
            gameObject.transform.localEulerAngles += new Vector3(0f, 0f, 180f);
        }

        gameObject.transform.position = newPos;
        gameObject.transform.SetAsLastSibling();
    }
}