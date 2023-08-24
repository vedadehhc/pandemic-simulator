using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelButton : MonoBehaviour
{
    [SerializeField] private bool rightPanel;
    private GameObject panelContent;
    private GameObject arrowImage;
    private bool open;
    private int openScaleX;

    public void Awake()
    {
        if(rightPanel) {
            openScaleX = 1;
        } else {
            openScaleX = -1;
        }

        panelContent = transform.parent.Find("Content").gameObject;

        arrowImage = transform.Find("Arrow").gameObject;
        arrowImage.transform.localScale = new Vector3(openScaleX, 1, 1);

        open = true;
        UpdatePanel();
    }

    public void TogglePanel() {
        arrowImage.transform.localScale *= -1;
        open = !open;
        UpdatePanel();
    }

    private void UpdatePanel()
    {
        panelContent.SetActive(open);
        if(open ^ rightPanel) {
            gameObject.GetComponent<RectTransform>().anchorMin = new Vector2(1f, 1f);
            gameObject.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
        } else {
            gameObject.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 1f);
            gameObject.GetComponent<RectTransform>().anchorMax = new Vector2(0f, 1f);
        }
    }
}
