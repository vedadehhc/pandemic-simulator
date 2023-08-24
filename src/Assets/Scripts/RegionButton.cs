using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionButton : MonoBehaviour
{
    public void OnMouseDown()
    {
        transform.parent.gameObject.GetComponent<Region>().Toggle();
    }
}
