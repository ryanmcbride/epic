using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChartsAndGraphs3D
{
    public class LineChart : MonoBehaviour
    {
        public TitleText TitleText;

        public Color LineColor;
        public Color AxisColor;

        /// <summary>
        /// Used if there are no values assigned via script
        /// Leave empty if you want a clean chart
        /// </summary>
        public List<float> StaticValues = new List<float>();

        /// <summary>
        /// Only change if you realy need to
        /// else just resize the charts transform
        /// </summary>
        internal float BaseWidth = 8, BaseHeight = 4;

        /// <summary>
        /// The maximum value on the display (Y-Axis) => if 0 it is dynamic
        /// </summary>
        public float FixMaxYValue = 0;

        /// <summary>
        /// If reached it will delete the first values (FIFO) and "scroll"
        /// </summary>
        public float MaxValuesAmount = 30;


        /// <summary>
        /// When activated it thinns the lines on amount>50 -> so it is more readable
        /// </summary>
        public bool ThinLineIfManyValues = false;

        public bool Glow;

        public Lines_Y_AxisTextUnityHelper YAxis;
        public Lines_X_AxisTextUnityHelper XAxis;

        public LinesCurrentTextUnityHelper CurrentValueText;


        public GameObject Line, Point;

        List<Transform> currLines = new List<Transform>();
        List<Transform> currPoints = new List<Transform>();


        public Transform Background;
        public Transform AxisY, AxisX;

        float currMaxY;

        bool change;

        List<float> values = new List<float>();
        public List<float> Values
        {
            get
            {
                if (values == null || values.Count == 0)
                    return StaticValues;

                return values;
            }

            set
            {
                values = value;
                change = true;
            }
        }

        // Use this for initialization
        void Start()
        {
            currMaxY = Values.Max();
            UpdateAxis();
            change = true;
            UpdateLineChart();
            HandleGlows(new List<Transform>() { AxisX, AxisY }, !Glow, AxisColor);     //To make sure it s set correct
            HandleGlows(new List<Transform>() { AxisX, AxisY }, Glow, AxisColor);      //To make sure it s set correct
            TitleText.Create(transform, TitlePositionMethod, LookAtMethod);

        }

        private Vector3 TitlePositionMethod()
        {
            return new Vector3(0, BaseHeight * 1.1f, 0) + TitleText.PositionOffset;
        }

        private Vector3 LookAtMethod()
        {
            return transform.forward;
        }

        float lastHeight, lastWidth, lastMaxY, LastMaxX;
        private void UpdateAxis()
        {
            UpdateCurrentTextHeight();

            if (lastHeight == BaseHeight && lastWidth == BaseWidth && lastMaxY == currMaxY && LastMaxX == XAxis.MarkMax)
                return;

            lastHeight = BaseHeight;
            lastWidth = BaseWidth;
            lastMaxY = currMaxY;

            AxisY.localScale = new Vector3(AxisY.localScale.x, BaseHeight, AxisY.localScale.z);
            AxisY.localPosition = new Vector3(AxisY.localPosition.x, BaseHeight / 2, AxisY.localPosition.z);

            AxisX.localScale = new Vector3(BaseWidth, AxisX.localScale.y, AxisX.localScale.z);
            AxisX.localPosition = new Vector3(BaseWidth / 2, AxisX.localPosition.y, AxisX.localPosition.z);

            Background.localScale = new Vector3(BaseWidth, BaseHeight, 0.05f);
            Background.localPosition = new Vector3(BaseWidth / 2, BaseHeight / 2, 0.05f);

            UpdateTexts();
        }

        private void UpdateCurrentTextHeight()
        {
            if (CurrentValueText.currentObject != null && (CurrentValueText.AimPos - CurrentValueText.currentObject.transform.localPosition).magnitude > 0.2f)
                CurrentValueText.currentObject.transform.localPosition += (CurrentValueText.AimPos - CurrentValueText.currentObject.transform.localPosition) * Time.deltaTime;
        }

        bool lastGlow;          //Only used in the Method below
        Color lastAxisColor;
        // Update is called once per frame
        void Update()
        {
            UpdateAxis();
            UpdateLineChart();

            HandleGlows(currLines, Glow, LineColor);
            HandleGlows(currPoints, Glow, LineColor);

            if (lastGlow == Glow && lastAxisColor == AxisColor)
                return;

            lastGlow = Glow;
            lastAxisColor = AxisColor;

            HandleGlows(new List<Transform>() { AxisX, AxisY }, Glow, AxisColor);

        }

        void UpdateTexts()
        {
            //max y text
            if (YAxis.AxisText != "")
            {
                Transform nParent;
                GameObject text3Dobj;
                if (YAxis.currentObject != null)
                {
                    nParent = YAxis.currentObject.transform;
                    text3Dobj = nParent.Find("3DText(Clone)").gameObject;
                }
                else
                {
                    nParent = (GameObject.Instantiate(Resources.Load("ChartsAndGraphs3D/Empty"), transform) as GameObject).transform;
                    text3Dobj = Instantiate(Resources.Load("ChartsAndGraphs3D/3DText"), nParent) as GameObject;
                }


                nParent.transform.localPosition = new Vector3(AxisY.localPosition.x * 1.05f, BaseHeight * 1.05f, 0);
                nParent.localScale = new Vector3(1 / transform.lossyScale.x, 1 / transform.lossyScale.y, 1 / transform.lossyScale.z);
                nParent.localScale /= nParent.localScale.magnitude;



                text3Dobj.GetComponent<TextMesh>().text = YAxis.AxisText;
                text3Dobj.transform.localScale = new Vector3(YAxis.Scale, YAxis.Scale, YAxis.Scale);
                text3Dobj.transform.LookAt(text3Dobj.transform.position + Background.forward);
                //text3Dobj.GetComponent<TextMesh>().anchor = TextAnchor.LowerCenter;
                YAxis.currentObject = nParent.gameObject;

            }

            //max x text
            if (XAxis.AxisText != "")
            {
                Transform nParent;
                GameObject text3Dobj;
                if (XAxis.currentObject != null)
                {
                    nParent = XAxis.currentObject.transform;
                    text3Dobj = nParent.Find("3DText(Clone)").gameObject;
                }
                else
                {
                    nParent = (GameObject.Instantiate(Resources.Load("ChartsAndGraphs3D/Empty"), transform) as GameObject).transform;
                    text3Dobj = Instantiate(Resources.Load("ChartsAndGraphs3D/3DText"), nParent) as GameObject;
                }

                nParent.transform.localPosition = new Vector3(BaseWidth * 1.02f, 0, 0);
                nParent.localScale = new Vector3(1 / transform.lossyScale.x, 1 / transform.lossyScale.y, 1 / transform.lossyScale.z);
                nParent.localScale /= nParent.localScale.magnitude;


                text3Dobj.GetComponent<TextMesh>().text = XAxis.AxisText;
                text3Dobj.transform.localScale = new Vector3(XAxis.Scale, XAxis.Scale, XAxis.Scale);
                text3Dobj.transform.LookAt(text3Dobj.transform.position + Background.forward);
                XAxis.currentObject = nParent.gameObject;
            }


            //x axis marks
            LastMaxX = XAxis.MarkMax;
            foreach (var item in XAxis.MarkList)
            {
                Destroy(item.gameObject);
            }
            XAxis.MarkList.Clear();

            float xDiffPerMark = BaseWidth / XAxis.MarkCount;


            for (int i = 1; i < XAxis.MarkCount + 1; i++)
            {
                Transform nParent;
                GameObject text, mark;
                nParent = (GameObject.Instantiate(Resources.Load("ChartsAndGraphs3D/Empty"), transform) as GameObject).transform;
                text = Instantiate(Resources.Load("ChartsAndGraphs3D/3DText"), nParent) as GameObject;
                mark = Instantiate(Resources.Load("ChartsAndGraphs3D/AxisMark"), nParent) as GameObject;


                nParent.transform.localPosition = new Vector3(i * xDiffPerMark, 0, 0);
                nParent.localScale = new Vector3(1 / transform.lossyScale.x, 1 / transform.lossyScale.y, 1 / transform.lossyScale.z);
                nParent.localScale /= nParent.localScale.magnitude;

                text.GetComponent<TextMesh>().text = Math.Round((XAxis.MarkMax / XAxis.MarkCount) * i, 2) + XAxis.MarkPostText;
                mark.transform.localScale *= XAxis.MarkScale;
                text.transform.localScale *= XAxis.MarkTextScale;

                text.transform.localPosition = new Vector3(0, -mark.transform.localScale.y * 1.12f, 0);
                text.GetComponent<TextMesh>().anchor = TextAnchor.UpperCenter;
                XAxis.MarkList.Add(nParent);
            }

            //yaxis marks
            foreach (var item in YAxis.MarkList)
            {
                Destroy(item.gameObject);
            }
            YAxis.MarkList.Clear();

            float yDiffPerMark = BaseHeight / YAxis.MarkCount;


            for (int i = 1; i < YAxis.MarkCount + 1; i++)
            {
                Transform nParent;
                GameObject text, mark;
                nParent = (GameObject.Instantiate(Resources.Load("ChartsAndGraphs3D/Empty"), transform) as GameObject).transform;
                text = Instantiate(Resources.Load("ChartsAndGraphs3D/3DText"), nParent) as GameObject;
                mark = Instantiate(Resources.Load("ChartsAndGraphs3D/AxisMark"), nParent) as GameObject;


                nParent.transform.localPosition = new Vector3(0, i * yDiffPerMark, 0);
                nParent.localScale = new Vector3(1 / transform.lossyScale.x, 1 / transform.lossyScale.y, 1 / transform.lossyScale.z);
                nParent.localScale /= nParent.localScale.magnitude;

                text.GetComponent<TextMesh>().text = Math.Round((currMaxY / YAxis.MarkCount) * i, 2) + YAxis.MarkPostText;
                mark.transform.localScale *= YAxis.MarkScale;
                mark.transform.Rotate(new Vector3(0, 0, 90), Space.Self);
                text.transform.localScale *= YAxis.MarkTextScale;

                text.transform.localPosition = new Vector3(-mark.transform.localScale.y * 1.3f, 0, 0);
                text.GetComponent<TextMesh>().anchor = TextAnchor.MiddleRight;
                YAxis.MarkList.Add(nParent);
            }



        }

        private void HandleGlows(List<Transform> list, bool glow, Color c)
        {

            foreach (var item in list)
            {
                HandleGlow(item.GetComponent<Renderer>(), glow, c);
            }
        }

        void HandleGlow(Renderer r, bool on, Color c)
        {

            if (on)
                r.material.EnableKeyword("_EMISSION");
            else
                r.material.DisableKeyword("_EMISSION");


            r.material.color = c;
            r.material.SetColor("_EmissionColor", c);
        }

        public void AddIncreaseValue(float increase)
        {
            if (Values.LastOrDefault() + increase < 0)
                Values.Add(0);
            else
                Values.Add(Values.LastOrDefault() + increase);


            change = true;
        }

        public void AddValue(float val)
        {
            Values.Add(val);

            change = true;
        }

        public void Reset()
        {
            Values.Clear();

            change = true;
        }

        List<float> LastValues;
        void UpdateLineChart()
        {
            if (!change || Values == null || Values == LastValues)
                return;

            LastValues = Values.ToList();

            change = false;

            foreach (Transform item in currLines)
            {
                Destroy(item.gameObject);
            }
            foreach (Transform item in currPoints)
            {
                Destroy(item.gameObject);
            }

            currLines.Clear();
            currPoints.Clear();



            while (Values.Count > MaxValuesAmount)
            {
                Values.RemoveAt(0);
            }

            float AmountScaleX;

            if (Values.Count < 2)
                AmountScaleX = BaseWidth;
            else
                AmountScaleX = BaseWidth / (Values.Count - 1);

            if (FixMaxYValue != 0)
                currMaxY = FixMaxYValue;
            else
            {
                //currMaxY = Mathf.Max(currMaxY, Values.Max());

                if (Values.Max() > currMaxY * 0.9f)
                {
                    currMaxY *= 1.05f;
                    change = true;
                }
                if (Values.Max() < currMaxY * 0.4f)
                {
                    currMaxY *= 0.95f;
                    change = true;
                }
            }

            if (YAxis.MarkCount != 0)
                currMaxY = YAxis.MarkCount * (float)(Math.Ceiling(currMaxY / YAxis.MarkCount));

            currMaxY = Mathf.Max(currMaxY, 1);

            float AmountScaleY = BaseHeight / currMaxY;

            float ThinScaler = 1;
            if (ThinLineIfManyValues)
                ThinScaler = Mathf.Clamp(2 - (0.01f * (Values.Count() + 50)), 0.2f, 1f);

            for (int i = 0; i < Values.Count; i++)
            {

                //text
                if (i == Values.Count - 1)
                {
                    if (CurrentValueText.Active)
                    {
                        //load or create the parent
                        Transform nParent;
                        if (CurrentValueText.currentObject == null)
                        {
                            nParent = (GameObject.Instantiate(Resources.Load("ChartsAndGraphs3D/Empty"), transform) as GameObject).transform;
                            CurrentValueText.currentObject = nParent.gameObject;
                        }
                        else
                        {
                            nParent = CurrentValueText.currentObject.transform;
                        }

                        //load or create the textObj
                        GameObject text3Dobj;

                        if (CurrentValueText.currentTextMesh == null)
                        {
                            text3Dobj = Instantiate(Resources.Load("ChartsAndGraphs3D/3DText"), nParent) as GameObject;
                            CurrentValueText.currentTextMesh = text3Dobj;
                            nParent.transform.localPosition = new Vector3(BaseWidth * 1.02f, BaseHeight * 0.5f, 0);
                        }
                        else
                        {
                            text3Dobj = CurrentValueText.currentTextMesh;
                        }


                        nParent.localScale = new Vector3(1 / transform.lossyScale.x, 1 / transform.lossyScale.y, 1 / transform.lossyScale.z);
                        nParent.localScale /= nParent.localScale.magnitude;




                        text3Dobj.GetComponent<TextMesh>().text = CurrentValueText.preText + System.Math.Round(Values.LastOrDefault(), 2) + CurrentValueText.postText;
                        text3Dobj.transform.localScale = new Vector3(CurrentValueText.Scale, CurrentValueText.Scale, CurrentValueText.Scale);
                        text3Dobj.transform.LookAt(text3Dobj.transform.position + Background.forward);
                        CurrentValueText.currentObject = nParent.gameObject;
                        CurrentValueText.AimPos = new Vector3(BaseWidth * 1.02f, Values[i] * AmountScaleY, 0);

                    }
                }

                //points
                Transform p = Instantiate(Point, transform).transform;
                p.localPosition = new Vector3(i * AmountScaleX, Values[i] * AmountScaleY, 0f);

                if (ThinLineIfManyValues)
                    p.localScale *= ThinScaler;

                //assign color
                Renderer renderer = p.GetComponent<Renderer>();
                renderer.materials[0].color = LineColor;
                renderer.materials[0].SetColor("_EmissionColor", LineColor);

                currPoints.Add(p);

                //lines
                if (i == 0)
                    continue;
                Transform l = Instantiate(Line, transform).transform;
                l.localPosition = new Vector3(((i - 0.5f) * AmountScaleX), (Values[i] + Values[i - 1]) * AmountScaleY / 2f, 0f);
                l.localRotation = Quaternion.Euler(new Vector3(0, 0, Mathf.Rad2Deg * Mathf.Atan2((Values[i] - Values[i - 1]) * AmountScaleY, 1 * AmountScaleX)));
                l.localScale = new Vector3(Mathf.Sqrt((Values[i] - Values[i - 1]) * AmountScaleY * (Values[i] - Values[i - 1]) * AmountScaleY + AmountScaleX * AmountScaleX), 0.1f, 0.1f);


                if (ThinLineIfManyValues)
                    l.localScale = new Vector3(l.localScale.x, l.localScale.y * ThinScaler, l.localScale.z * ThinScaler);

                //assign color
                renderer = l.GetComponent<Renderer>();
                renderer.materials[0].color = LineColor;
                renderer.materials[0].SetColor("_EmissionColor", LineColor);
                currLines.Add(l);

            }
        }
    }

    [Serializable]
    public class Lines_X_AxisTextUnityHelper
    {
        /// <summary>
        /// Leave empty for no Text
        /// </summary>
        public string AxisText;
        public float Scale = 0.1f;
        internal GameObject currentObject;


        public int MarkCount = 4;
        public float MarkMax = 10;
        public float MarkScale = 1f;
        public float MarkTextScale = 0.1f;
        public string MarkPostText;
        internal List<Transform> MarkList = new List<Transform>();
    }

    [Serializable]
    public class Lines_Y_AxisTextUnityHelper
    {
        public string AxisText;
        public float Scale = 0.1f;
        internal GameObject currentObject;


        public int MarkCount = 4;
        public float MarkScale = 1f;
        public float MarkTextScale = 0.1f;
        public string MarkPostText;
        internal List<Transform> MarkList = new List<Transform>();
    }

    [Serializable]
    public class LinesCurrentTextUnityHelper
    {
        public bool Active = true;
        public string preText;
        public string postText;
        public float Scale = 0.1f;
        internal GameObject currentObject;
        internal GameObject currentTextMesh;
        internal Vector3 AimPos;
    }
}