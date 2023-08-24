using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Region : MonoBehaviour
{
    public bool open;
    private float xMin, xMax, yMin, yMax;
    
    public void Setup(bool interact)
    {
        float xr = Mathf.Abs(gameObject.transform.localScale.x / 2f);
        float yr = Mathf.Abs(gameObject.transform.localScale.y / 2f);
        xMin = gameObject.transform.position.x - 0.9f * xr;
        xMax = gameObject.transform.position.x + 0.9f * xr;
        yMin = gameObject.transform.position.y - 0.9f * yr;
        yMax = gameObject.transform.position.y + 0.9f * yr;
        open = true;

        Transform buttonTransform = gameObject.transform.Find("OpenButton");
        if (interact)
        {
            if (buttonTransform == null)
            {
                GameObject buttonObject = new GameObject("OpenButton", typeof(SpriteRenderer));
                buttonObject.transform.SetParent(transform, false);
                buttonObject.GetComponent<SpriteRenderer>().sprite = gameObject.GetComponent<SpriteRenderer>().sprite;
            }

            GameObject button = gameObject.transform.Find("OpenButton").gameObject;

            button.GetComponent<SpriteRenderer>().color = new Color(0f, 1f, 0f, 1f);
            button.transform.localScale = new Vector3(.5f / gameObject.transform.localScale.x, .5f / gameObject.transform.localScale.y, 1f);
            button.transform.localPosition = new Vector3(
                (xr / -gameObject.transform.localScale.x) + (button.transform.localScale.x / 2f),
                (yr / -gameObject.transform.localScale.y) - (button.transform.localScale.y / 2f), 0);


            BoxCollider2D collider = button.GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                button.AddComponent<BoxCollider2D>();
            }

            RegionButton regionButton = button.GetComponent<RegionButton>();
            if(regionButton == null) {
                button.AddComponent<RegionButton>();
            }

            UpdateButton();
        }
        else
        {
            if (buttonTransform != null)
            {
                Destroy(buttonTransform.gameObject);
            }
        }

    }

    public void Toggle()
    {
        open = !open;
        UpdateButton();
    }

    protected void UpdateButton()
    {
        GameObject button = gameObject.transform.Find("OpenButton").gameObject;
        if (open)
        {
            button.GetComponent<SpriteRenderer>().color = new Color(0f, 1f, 0f, 1f);
        }
        else
        {
            button.GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0f, 1f);
        }
    }

    public float RandomXCor()
    {
        return Random.Range(xMin, xMax);
    }

    public float RandomYCor()
    {
        return Random.Range(yMin, yMax);
    }

    public float XMin()
    {
        return xMin;
    }
    public float XMax()
    {
        return xMax;
    }
    public float YMin()
    {
        return yMin;
    }
    public float YMax()
    {
        return yMax;
    }

    public string ToString()
    {
        return "(" + xMin + ", " + yMin + ") -> (" + xMax + ", " + yMax + ")";
    }
}
