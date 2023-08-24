using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityAgent : MonoBehaviour
{
    protected int id;
    public Region region;

    private void OnEnable()
    {
        Setup();
    }
    public virtual void Setup()
    {
        return;
    }
    public virtual void Go()
    {
        return;
    }
    
    public void Initialize(int id, Region region, Color color, Sprite shape, float scale)
    {
        this.id = id;
        this.region = region;
        gameObject.GetComponent<SpriteRenderer>().sprite = shape;
        gameObject.GetComponent<SpriteRenderer>().color = color;
        gameObject.transform.localScale = new Vector3(scale, scale, scale);
        gameObject.transform.SetAsLastSibling();
    }

    public void SetColor(Color color)
    {
        gameObject.GetComponent<SpriteRenderer>().color = color;
    }

    public void SetShape(Sprite shape) {
        gameObject.GetComponent<SpriteRenderer>().sprite = shape;
    }
}