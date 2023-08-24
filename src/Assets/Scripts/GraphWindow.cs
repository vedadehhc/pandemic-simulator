using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CodeMonkey.Utils;

public class GraphWindow : MonoBehaviour
{
    public bool hasDots;
    [SerializeField] private bool hasXLabel;
    [SerializeField] private int minNumX;
    [SerializeField] private bool scaleYUp, scaleYDown;
    [SerializeField] public int minYVal, maxYVal;
    [SerializeField] private bool hasXSep, hasYSep;
    [SerializeField] private Sprite dotSprite;
    private RectTransform graphContainer, labelTemplateX, labelTemplateY, dashTemplateX, dashTemplateY, key, keyTemplate;
    private List<GameObject> gameObjectList, keyItemList;
    private List<IGraphVisualObject> graphVisualObjects;
    private GameObject tooltipGameObject;
    private List<RectTransform> yLabels;
    private bool setup = false;


    // Cached values
    private List<List<float>> valueLists;
    private List<IGraphVisual> graphVisuals;
    private int maxVisibleValues, offset;
    private Func<int, string> getAxisLabelX;
    private Func<float, string> getAxisLabelY; // Add setter method
    private float yMin, yMax;
    private float xSize;


    public void Setup()
    {
        if (setup) return;
        graphContainer = transform.Find("GraphContainer").GetComponent<RectTransform>();
        labelTemplateX = graphContainer.Find("LabelTemplateX").GetComponent<RectTransform>();
        labelTemplateY = graphContainer.Find("LabelTemplateY").GetComponent<RectTransform>();
        dashTemplateX = graphContainer.Find("DashTemplateX").GetComponent<RectTransform>();
        dashTemplateY = graphContainer.Find("DashTemplateY").GetComponent<RectTransform>();
        tooltipGameObject = graphContainer.Find("Tooltip").gameObject;
        key = transform.Find("Key").GetComponent<RectTransform>();
        keyTemplate = key.Find("KeyTemplate").GetComponent<RectTransform>();

        gameObjectList = new List<GameObject>();
        graphVisualObjects = new List<IGraphVisualObject>();
        yLabels = new List<RectTransform>();

        keyItemList = new List<GameObject>();

        // IGraphVisual lineGraphVisual = new LineGraphVisual(this, graphContainer, dotSprite, Color.white, new Color(1, 1, 1, .5f));
        // IGraphVisual barChartVisual = new BarChartVisual(this, graphContainer, Color.white, 0.8f);

        valueLists = new List<List<float>>();
        graphVisuals = new List<IGraphVisual>();
        ShowGraph(valueLists, graphVisuals);

        // transform.Find("BarChartButton").GetComponent<Button>().onClick.AddListener(delegate { SetGraphVisual(barChartVisual); });
        transform.Find("LineGraphButton").GetComponent<Button>().onClick.AddListener(delegate { ToggleDots(); });

        transform.Find("ZoomIn").GetComponent<Button>().onClick.AddListener(delegate { ChangeVisibleAmount(-10); });
        transform.Find("ZoomOut").GetComponent<Button>().onClick.AddListener(delegate { ChangeVisibleAmount(+10); });

        transform.Find("ShiftLeft").GetComponent<Button>().onClick.AddListener(delegate { Shift(+10); });
        transform.Find("ShiftRight").GetComponent<Button>().onClick.AddListener(delegate { Shift(-10); });

        HideTooltip();
        setup = true;
    }

    private void Awake()
    {
        Setup();
    }

    private void ShowTooltip(string tooltipText, Vector2 anchoredPosition)
    {
        tooltipGameObject.SetActive(true);
        tooltipGameObject.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;

        Text textObject = tooltipGameObject.transform.Find("Text").GetComponent<Text>();
        textObject.text = tooltipText;

        float padding = 4f;
        Vector2 backgroundSize = new Vector2(textObject.preferredWidth + padding * 2f, textObject.preferredHeight + padding * 2f);
        tooltipGameObject.transform.Find("Background").GetComponent<RectTransform>().sizeDelta = backgroundSize;

        tooltipGameObject.transform.SetAsLastSibling();
    }

    private void HideTooltip()
    {
        tooltipGameObject.SetActive(false);
    }

    public void AddLineSeries(Color color, String name)
    {
        IGraphVisual lineGraphVisual = new LineGraphVisual(this, graphContainer, dotSprite, color, color);
        graphVisuals.Add(lineGraphVisual);
        valueLists.Add(new List<float>());

        RectTransform keyItem = Instantiate(keyTemplate);
        keyItem.SetParent(key);
        keyItem.gameObject.SetActive(true);
        keyItem.localScale = new Vector3(1, 1, 1);

        Image colorImage = keyItem.Find("Image").GetComponent<Image>();
        color.a = 1f;
        colorImage.color = color;

        Text text = keyItem.Find("Text").GetComponent<Text>();
        text.text = name;

        keyItemList.Add(keyItem.gameObject);
    }

    public void ClearAll()
    {
        Setup();

        if (keyItemList != null)
        {
            foreach (GameObject go in keyItemList)
            {
                Destroy(go);
            }
            keyItemList.Clear();
        }

        this.graphVisuals = new List<IGraphVisual>();
        this.valueLists = new List<List<float>>();
        this.offset = 0;
        ShowGraph(this.valueLists, this.graphVisuals, this.maxVisibleValues, this.offset, this.getAxisLabelX, this.getAxisLabelY);
        HideTooltip();
    }

    public void PushValues(List<float> vals)
    {
        foreach (IGraphVisual graphVisual in graphVisuals)
        {
            graphVisual.Refresh();
        }
        for (int i = 0; i < valueLists.Count; i++)
        {
            valueLists[i].Add(vals[i]);
        }
        this.offset = 0;
        ShowGraph(this.valueLists, this.graphVisuals, this.maxVisibleValues, this.offset, this.getAxisLabelX, this.getAxisLabelY);
    }

    /* public void UpdateValue(int listInd, int index, int val)
    {
        if (index < graphVisualObjects.Count && index >= 0)
        {
            this.graphVisual.Refresh();
            valueList[index] = val;

            float graphWidth = graphContainer.sizeDelta.x;
            float graphHeight = graphContainer.sizeDelta.y;

            if (CalculateYScale())
            {
                for (int i = Math.Max(valueList.Count - maxVisibleValues - offset, 0), xi = 0; i < valueList.Count - offset; i++, xi++)
                {
                    float xPosition = (xi + 1) * this.xSize;
                    float yPosition = ((valueList[i] - this.yMin) / (this.yMax - this.yMin)) * graphHeight;

                    string tooltipText = getAxisLabelY(valueList[i]);
                    graphVisualObjects[xi].SetGraphVisualObjectInfo(this, new Vector2(xPosition, yPosition), this.xSize, tooltipText);
                }

                for (int i = 0; i < yLabels.Count; i++)
                {
                    float normalizedValue = i * 1f / (yLabels.Count - 1);
                    yLabels[i].GetComponent<Text>().text = getAxisLabelY(this.yMin + normalizedValue * (this.yMax - this.yMin));
                    yLabels[i].localScale = new Vector3(1, 1, 1);
                }
            }
            else
            {
                int xi = index - (valueList.Count - maxVisibleValues - offset);
                if (xi >= 0 && xi < maxVisibleValues)
                {
                    float xPosition = (xi + 1) * this.xSize;
                    float yPosition = ((valueList[index] - this.yMin) / (this.yMax - this.yMin)) * graphHeight;

                    string tooltipText = getAxisLabelY(valueList[index]);
                    graphVisualObjects[xi].SetGraphVisualObjectInfo(this, new Vector2(xPosition, yPosition), this.xSize, tooltipText);
                }
            }
        }
        else
        {
            Debug.Log("Index out of range");
        }
    } */

    private bool CalculateYScale()
    {
        float prevYMin = this.yMin;
        float prevYMax = this.yMax;

        this.yMin = minYVal;
        this.yMax = maxYVal;

        if (scaleYUp && valueLists.Count > 0)
        {
            for (int i = Math.Max(valueLists[0].Count - maxVisibleValues - offset, 0); i < valueLists[0].Count - offset; i++)
            {
                for (int j = 0; j < valueLists.Count; j++)
                {
                    float modVal = this.yMin + 1.2f * (valueLists[j][i] - this.yMin);
                    if (modVal > this.yMax)
                    {
                        this.yMax = modVal;
                    }
                }
            }
        }

        if (scaleYDown && valueLists.Count > 0)
        {
            for (int i = Math.Max(valueLists[0].Count - maxVisibleValues - offset, 0); i < valueLists[0].Count - offset; i++)
            {
                for (int j = 0; j < valueLists.Count; j++)
                {
                    float modVal = this.yMax + 1.2f * (valueLists[j][i] - this.yMax);
                    if (modVal < this.yMin)
                    {
                        this.yMin = modVal;
                    }
                }
            }
        }

        if (yMin != prevYMin || yMax != prevYMax)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void ChangeVisibleAmount(int amt)
    {
        if (this.valueLists.Count > 0)
        {
            amt = Mathf.Clamp(amt, -this.maxVisibleValues, this.valueLists[0].Count - this.maxVisibleValues);
            if (this.maxVisibleValues + amt <= this.valueLists[0].Count && this.valueLists[0].Count - this.offset < this.maxVisibleValues + amt)
            {
                this.offset = Mathf.Clamp(this.offset, 0, this.valueLists[0].Count - this.maxVisibleValues - amt);
            }
            foreach (IGraphVisual graphVisual in graphVisuals)
            {
                graphVisual.Refresh();
            }
            ShowGraph(this.valueLists, this.graphVisuals, this.maxVisibleValues + amt, this.offset, this.getAxisLabelX, this.getAxisLabelY);
        }
    }

    private void ToggleDots()
    {
        hasDots = !hasDots;
        foreach (IGraphVisual graphVisual in graphVisuals)
        {
            graphVisual.Refresh();
        }
        ShowGraph(this.valueLists, this.graphVisuals, this.maxVisibleValues, this.offset, this.getAxisLabelX, this.getAxisLabelY);
    }

    private void SetGraphVisual(int ind, IGraphVisual gv)
    {
        graphVisuals[ind] = gv;
        foreach (IGraphVisual graphVisual in graphVisuals)
        {
            graphVisual.Refresh();
        }
        ShowGraph(this.valueLists, this.graphVisuals, this.maxVisibleValues, this.offset, this.getAxisLabelX, this.getAxisLabelY);
    }

    private void Shift(int amt)
    {
        if (this.valueLists.Count > 0)
        {
            this.offset = Mathf.Clamp(this.offset + amt, 0, this.valueLists[0].Count - this.maxVisibleValues);
            foreach (IGraphVisual graphVisual in graphVisuals)
            {
                graphVisual.Refresh();
            }
            ShowGraph(this.valueLists, this.graphVisuals, this.maxVisibleValues, this.offset, this.getAxisLabelX, this.getAxisLabelY);
        }
    }

    private void ShowGraph(List<List<float>> valueLists, List<IGraphVisual> graphVisuals, int maxVisibleValues = -1, int offset = 0, Func<int, string> getAxisLabelX = null, Func<float, string> getAxisLabelY = null)
    {
        this.valueLists = valueLists;
        this.graphVisuals = graphVisuals;

        if (getAxisLabelX == null)
        {
            getAxisLabelX = delegate (int _i) { return _i.ToString(); };
        }

        if (getAxisLabelY == null)
        {
            getAxisLabelY = delegate (float _f) { return Mathf.RoundToInt(_f).ToString(); };
        }

        this.getAxisLabelX = getAxisLabelX;
        this.getAxisLabelY = getAxisLabelY;

        if (maxVisibleValues <= 0)
        {
            if (valueLists.Count > 0)
            {
                maxVisibleValues = valueLists[0].Count;
            }
        }

        if (maxVisibleValues < minNumX)
        {
            maxVisibleValues = minNumX;
        }


        if (valueLists.Count > 0 && maxVisibleValues > valueLists[0].Count)
        {
            maxVisibleValues = valueLists[0].Count;
        }

        this.maxVisibleValues = maxVisibleValues;
        this.offset = offset;

        foreach (GameObject go in gameObjectList)
        {
            Destroy(go);
        }
        gameObjectList.Clear();
        yLabels.Clear();

        foreach (IGraphVisualObject igvo in graphVisualObjects)
        {
            igvo.CleanUp();
        }
        graphVisualObjects.Clear();

        float graphWidth = graphContainer.sizeDelta.x;
        float graphHeight = graphContainer.sizeDelta.y;
        this.xSize = graphWidth / (maxVisibleValues + 1);

        CalculateYScale();

        if (valueLists.Count > 0)
        {
            for (int i = Math.Max(valueLists[0].Count - maxVisibleValues - offset, 0), xi = 0; i < valueLists[0].Count - offset; i++, xi++)
            {

                float xPosition = (xi + 1) * this.xSize;
                for (int j = 0; j < valueLists.Count; j++)
                {
                    float yPosition = ((valueLists[j][i] - this.yMin) / (this.yMax - this.yMin)) * graphHeight;

                    string tooltipText = "(" + getAxisLabelX(i) + ", " + getAxisLabelY(valueLists[j][i]) + ")";
                    graphVisualObjects.Add(graphVisuals[j].CreateGraphVisualObject(new Vector2(xPosition, yPosition), this.xSize, tooltipText));
                }

                if (hasXLabel)
                {
                    RectTransform labelX = Instantiate(labelTemplateX);
                    labelX.SetParent(graphContainer);
                    labelX.gameObject.SetActive(true);
                    labelX.anchoredPosition = new Vector2(xPosition, -7f);
                    labelX.GetComponent<Text>().text = getAxisLabelX(i);
                    labelX.localScale = new Vector3(1, 1, 1);

                    gameObjectList.Add(labelX.gameObject);
                }


                if (hasXSep)
                {
                    RectTransform dashX = Instantiate(dashTemplateX);
                    dashX.SetParent(graphContainer);
                    dashX.gameObject.SetActive(true);
                    dashX.anchoredPosition = new Vector2(xPosition, -4f);
                    dashX.localScale = new Vector3(1, 1, 1);

                    gameObjectList.Add(dashX.gameObject);
                }
            }

            int separatorCount = 10;
            for (int i = 0; i <= separatorCount; i++)
            {
                RectTransform labelY = Instantiate(labelTemplateY);
                labelY.SetParent(graphContainer);
                labelY.gameObject.SetActive(true);
                float normalizedValue = i * 1f / separatorCount;
                labelY.anchoredPosition = new Vector2(-7f, normalizedValue * graphHeight);
                labelY.GetComponent<Text>().text = getAxisLabelY(this.yMin + normalizedValue * (this.yMax - this.yMin));
                labelY.localScale = new Vector3(1, 1, 1);

                yLabels.Add(labelY);
                gameObjectList.Add(labelY.gameObject);

                if (hasYSep)
                {
                    RectTransform dashY = Instantiate(dashTemplateY);
                    dashY.SetParent(graphContainer);
                    dashY.gameObject.SetActive(true);
                    dashY.anchoredPosition = new Vector2(-4f, normalizedValue * graphHeight);
                    dashY.localScale = new Vector3(1, 1, 1);

                    gameObjectList.Add(dashY.gameObject);
                }
            }
        }

    }

    public void SetAxisLabelY(Func<float, string> getAxisLabelY)
    {
        this.getAxisLabelY = getAxisLabelY;
    }


    // Intergace definition for showing visual for data point
    private interface IGraphVisual
    {
        IGraphVisualObject CreateGraphVisualObject(Vector2 graphPosition, float graphPositionWidth, string tooltipText);
        void Refresh();
    }

    // Represents a single visual object in graph
    private interface IGraphVisualObject
    {
        void SetGraphVisualObjectInfo(GraphWindow graphWindow, Vector2 graphPosition, float graphPositionWidth, string tooltipText);
        void CleanUp();
    }

    private class BarChartVisual : IGraphVisual
    {
        private GraphWindow graphWindow;
        private RectTransform graphContainer;
        private Color barColor;
        private float barWidthMult;

        public BarChartVisual(GraphWindow graphWindow, RectTransform graphContainer, Color barColor, float barWidthMult)
        {
            this.graphWindow = graphWindow;
            this.graphContainer = graphContainer;
            this.barColor = barColor;
            this.barWidthMult = barWidthMult;
        }

        public void Refresh()
        {
            graphWindow.HideTooltip();
        }

        public IGraphVisualObject CreateGraphVisualObject(Vector2 graphPosition, float graphPositionWidth, string tooltipText)
        {
            GameObject barGameObject = CreateBar(graphPosition, graphPositionWidth);

            BarChartVisualObject barChartVisualObject = new BarChartVisualObject(barGameObject, barWidthMult);
            barChartVisualObject.SetGraphVisualObjectInfo(graphWindow, graphPosition, graphPositionWidth, tooltipText);

            return barChartVisualObject;
        }

        private GameObject CreateBar(Vector2 graphPosition, float barWidth)
        {
            GameObject gameObject = new GameObject("bar", typeof(Image));
            gameObject.transform.SetParent(graphContainer, false);
            gameObject.GetComponent<Image>().color = barColor;

            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(graphPosition.x, 0);
            rectTransform.sizeDelta = new Vector2(barWidthMult * barWidth, graphPosition.y);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.pivot = new Vector2(0.5f, 0);

            Button_UI buttonUI = gameObject.AddComponent<Button_UI>();

            return gameObject;
        }

        public class BarChartVisualObject : IGraphVisualObject
        {
            private GameObject barGameObject;
            private float barWidthMult;

            public BarChartVisualObject(GameObject barGameObject, float barWidthMult)
            {
                this.barGameObject = barGameObject;
                this.barWidthMult = barWidthMult;
            }
            public void SetGraphVisualObjectInfo(GraphWindow graphWindow, Vector2 graphPosition, float graphPositionWidth, string tooltipText)
            {
                RectTransform rectTransform = barGameObject.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(graphPosition.x, 0);
                rectTransform.sizeDelta = new Vector2(barWidthMult * graphPositionWidth, graphPosition.y);

                Button_UI buttonUI = barGameObject.GetComponent<Button_UI>();

                buttonUI.ClickFunc = () =>
                {
                    graphWindow.ShowTooltip(tooltipText, graphPosition);
                };

                buttonUI.MouseOverOnceFunc = () =>
                {
                    graphWindow.ShowTooltip(tooltipText, graphPosition);
                };
            }

            public void CleanUp()
            {
                Destroy(barGameObject);
            }
        }
    }

    private class LineGraphVisual : IGraphVisual
    {
        private GraphWindow graphWindow;
        private RectTransform graphContainer;
        private Sprite dotSprite;
        private LineGraphVisualObject prevVisualGameObject;
        private Color dotColor, dotConnectionColor;

        public LineGraphVisual(GraphWindow graphWindow, RectTransform graphContainer, Sprite dotSprite, Color dotColor, Color dotConnectionColor)
        {
            this.graphWindow = graphWindow;
            this.graphContainer = graphContainer;
            this.dotSprite = dotSprite;
            this.dotColor = dotColor;
            this.dotConnectionColor = dotConnectionColor;
            prevVisualGameObject = null;
        }

        public void Refresh()
        {
            graphWindow.HideTooltip();
            prevVisualGameObject = null;
        }

        public IGraphVisualObject CreateGraphVisualObject(Vector2 graphPosition, float graphPositionWidth, string tooltipText)
        {
            GameObject dotGameObject = CreateDot(graphPosition);

            GameObject dotConnection = null;
            if (prevVisualGameObject != null)
            {
                dotConnection = CreateDotConnection(prevVisualGameObject.GetGraphPosition(), dotGameObject.GetComponent<RectTransform>().anchoredPosition);
            }

            LineGraphVisualObject lineGraphVisualObject = new LineGraphVisualObject(dotGameObject, dotConnection, prevVisualGameObject);
            lineGraphVisualObject.SetGraphVisualObjectInfo(graphWindow, graphPosition, graphPositionWidth, tooltipText);

            prevVisualGameObject = lineGraphVisualObject;

            return lineGraphVisualObject;
        }

        private GameObject CreateDot(Vector2 anchoredPosition)
        {
            if (graphWindow.hasDots)
            {
                GameObject dotGameObject = new GameObject("dot", typeof(Image));
                dotGameObject.transform.SetParent(graphContainer, false);
                dotGameObject.GetComponent<Image>().sprite = dotSprite;
                dotGameObject.GetComponent<Image>().color = dotColor;

                RectTransform rectTransform = dotGameObject.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = anchoredPosition;
                rectTransform.sizeDelta = new Vector2(15, 15);
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(0, 0);

                Button_UI buttonUI = dotGameObject.AddComponent<Button_UI>();

                return dotGameObject;
            }
            else
            {
                GameObject dotGameObject = new GameObject("dot", typeof(RectTransform));
                dotGameObject.transform.SetParent(graphContainer, false);

                RectTransform rectTransform = dotGameObject.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = anchoredPosition;
                rectTransform.sizeDelta = new Vector2(15, 15);
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(0, 0);

                Button_UI buttonUI = dotGameObject.AddComponent<Button_UI>();

                return dotGameObject;
            }
        }

        private GameObject CreateDotConnection(Vector2 dotPosA, Vector2 dotPosB)
        {
            GameObject gameObject = new GameObject("dotConnection", typeof(Image));
            gameObject.transform.SetParent(graphContainer, false);
            gameObject.GetComponent<Image>().color = dotConnectionColor;

            Vector2 dir = (dotPosB - dotPosA).normalized;
            float distance = Vector2.Distance(dotPosA, dotPosB);

            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.sizeDelta = new Vector2(distance, 3f);

            rectTransform.anchoredPosition = dotPosA + dir * distance * 0.5f;
            rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

            return gameObject;
        }

        public class LineGraphVisualObject : IGraphVisualObject
        {
            public event EventHandler OnChangedGraphVisualObjectInfo;
            private GameObject dotGameObject, dotConnectionGameObject;
            private LineGraphVisualObject prevVisualObject;
            public LineGraphVisualObject(GameObject dotGameObject, GameObject dotConnectionGameObject, LineGraphVisualObject prevVisualObject)
            {
                this.dotGameObject = dotGameObject;
                this.dotConnectionGameObject = dotConnectionGameObject;
                this.prevVisualObject = prevVisualObject;

                if (prevVisualObject != null)
                {
                    prevVisualObject.OnChangedGraphVisualObjectInfo += PrevVisualObject_OnChangedGraphVisualObjectInfo;
                }
            }

            private void PrevVisualObject_OnChangedGraphVisualObjectInfo(object sender, EventArgs e)
            {
                UpdateDotConnection();
            }

            public void SetGraphVisualObjectInfo(GraphWindow graphWindow, Vector2 graphPosition, float graphPositionWidth, string tooltipText)
            {
                RectTransform rectTransform = dotGameObject.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = graphPosition;

                UpdateDotConnection();

                Button_UI buttonUI = dotGameObject.GetComponent<Button_UI>();

                buttonUI.ClickFunc += () =>
                {
                    graphWindow.ShowTooltip(tooltipText, graphPosition);
                };

                buttonUI.MouseOverOnceFunc += () =>
                {
                    graphWindow.ShowTooltip(tooltipText, graphPosition);
                };

                if (OnChangedGraphVisualObjectInfo != null)
                {
                    OnChangedGraphVisualObjectInfo(this, EventArgs.Empty);
                }
            }

            private void UpdateDotConnection()
            {
                if (dotConnectionGameObject != null)
                {
                    Vector2 dir = (prevVisualObject.GetGraphPosition() - this.GetGraphPosition()).normalized;
                    float distance = Vector2.Distance(this.GetGraphPosition(), prevVisualObject.GetGraphPosition());

                    RectTransform dotConnectionRectTransform = dotConnectionGameObject.GetComponent<RectTransform>();
                    dotConnectionRectTransform.sizeDelta = new Vector2(distance, 3f);

                    dotConnectionRectTransform.anchoredPosition = this.GetGraphPosition() + dir * distance * 0.5f;
                    dotConnectionRectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
                }
            }

            public void CleanUp()
            {
                Destroy(dotGameObject);
                Destroy(dotConnectionGameObject);
            }

            public Vector2 GetGraphPosition()
            {
                RectTransform rectTransform = dotGameObject.GetComponent<RectTransform>();
                return rectTransform.anchoredPosition;
            }
        }
    }
}
