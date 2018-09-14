using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChartsAndGraphs3D
{
    public class BarChart : MonoBehaviour
    {
        public TitleText TitleText;

        public List<float> values = new List<float>();

        /// <summary>
        /// Assign colors for each bar
        /// If you want only one color for all bars -> only set one
        /// </summary>
        public List<Color> BarColors = new List<Color>() { Color.red, Color.blue, Color.green, Color.magenta, new Color(0.8f, 0.27f, 0) };

        public Color SeperatorColor;

        public float BarWidthRelativeToSpace = 0.8f;

        public float GraphDepth = 0.2f;

        public float AxisAmount;

        public bool Glow;

        public TextDisplayCreator TextDisplay;

        public BarAxisTextHelper AxisText;




        public GameObject Bar;
        public GameObject Background;
        public GameObject BasicAxis;


        List<Transform> CurrBars = new List<Transform>();

        List<Transform> CurrAxis = new List<Transform>();

        public List<float> Values
        {
            get
            {
                return values;
            }

            set
            {
                values = value;
                change = true;
            }
        }
        /// <summary>
        /// Only change if you realy need to
        /// else just resize the charts transform
        /// </summary>
        internal float BaseWidth = 2, BaseHeight = 4;

        float currMaxY;

        bool change;

        // Use this for initialization
        void Start()
        {
            //PlaceHelper.SetActive(false);
            currMaxY = BaseHeight;
            change = true;
            UpdateBarChart();
            HandleGlows(CurrBars, !Glow);       //To make sure it s set correct
            HandleGlows(CurrBars, Glow);        //To make sure it s set correct

            TextDisplay.Create(transform, new Vector3(TextDisplay.Position.x * (BaseWidth), TextDisplay.Position.y, TextDisplay.Position.z), InfoGetterMethod, LookAtMethod);
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

        private List<TextRowInfo> InfoGetterMethod()
        {
            List<TextRowInfo> infos = new List<TextRowInfo>();
            for (int i = 0; i < Values.Count; i++)
            {
                infos.Add(new TextRowInfo() { Value = Values[i], c = BarColors[Mathf.Min(i, BarColors.Count - 1)], PreText = TextDisplay.PreText, PostText = TextDisplay.PostText });
            }
            return infos;
        }

        // Update is called once per frame
        void Update()
        {
            UpdateBarChart();

            HandleGlows(CurrBars, Glow);
        }


        private void HandleGlows(List<Transform> list, bool glow)
        {
            for (int i = 0; i < list.Count(); i++)
            {
                HandleGlow(list[i].GetComponent<Renderer>(), glow, BarColors[Mathf.Min(i, BarColors.Count() - 1)]);
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

        public void ChangeBarValue(int BarID, float val)
        {
            if (Values.Count <= BarID)
                AddBar(val);
            else
                Values[BarID] = val;
            change = true;
        }

        public int AddBar(float val)
        {
            Values.Add(val);
            change = true;
            return Values.Count - 1;
        }

        public void RemoveBar(int BarID)
        {
            Values.RemoveAt(BarID);
            change = true;
        }

        List<float> LastValues = new List<float>();
        void UpdateBarChart()
        {
            if (!change && LastValues.SequenceEqual(Values))
                return;

            LastValues = Values.ToList();
            change = false;

            if (TextDisplay.TextDisplayActive && TextDisplay.go != null)
                TextDisplay.go.transform.localPosition = new Vector3(TextDisplay.Position.x * (BaseWidth / 2f), TextDisplay.Position.y, TextDisplay.Position.z);

            //foreach (Transform item in CurrBars)
            //{
            //    Destroy(item.gameObject);
            //}

            //urrBars.Clear();
            //currMaxY = Mathf.Max(currMaxY, Values.Max());
            if (Values.Max() < currMaxY * 0.7f)
            {
                currMaxY *= 0.7f;
                change = true;
            }
            else if (Values.Max() > currMaxY * 1.01f)
            {
                currMaxY *= 1.3f;
                change = true;
            }

            //round currmaxy to the next multiple of axisamount
            //then the axis are always whole numbers
            if (AxisText.onlyWholeNumbers)
                currMaxY = (float)(AxisAmount * Math.Ceiling(currMaxY / AxisAmount));

            if (CurrBars.Count > values.Count)
            {
                for (int i = CurrBars.Count - 1; i >= Values.Count; i--)
                {
                    Destroy(CurrBars[i].gameObject);
                    CurrBars.RemoveAt(i);
                }
            }

            float OneScaleX = BaseWidth / Values.Count;
            float OneScaleY = BaseHeight / currMaxY;
            for (int i = 0; i < Values.Count; i++)
            {
                Transform b;
                if (i >= CurrBars.Count)
                {
                    b = Instantiate(Bar, transform).transform;
                    b.localScale = new Vector3();
                    CurrBars.Add(b);
                    SetColor();
                }
                else
                {
                    b = CurrBars[i];
                }

                b.GetComponent<BarBuildUpScript>().UpdateValues(new Vector3((i + 0.5f) * OneScaleX, (Values[i] * OneScaleY) / 2, 0f), new Vector3(0.5f * OneScaleX * BarWidthRelativeToSpace, Values[i] * OneScaleY, GraphDepth));
            }

            UpdateAxis();
        }


        float lastWidth, lastHeight, lastMaxY;                   //yeah here
        private void UpdateAxis()
        {
            if (lastWidth == BaseWidth && lastHeight == BaseHeight && currMaxY == lastMaxY)
                return;
            lastWidth = BaseWidth;
            lastHeight = BaseHeight;
            lastMaxY = currMaxY;

            foreach (Transform item in CurrAxis)
            {
                Destroy(item.gameObject);
            }

            CurrAxis.Clear();

            float AxisDiff_Y = BaseHeight / AxisAmount;

            if (AxisText.AxisTextActive)
            {
                if (AxisText.currentObjects.Count != 0)
                {
                    foreach (var item in AxisText.currentObjects)
                    {
                        Destroy(item);
                    }
                    AxisText.currentObjects.Clear();
                }
            }

            for (int i = 0; i < AxisAmount + 1; i++)
            {
                Transform a = Instantiate(BasicAxis, transform).transform;



                a.localPosition = new Vector3(BaseWidth / 2, AxisDiff_Y * i, 0);
                a.localScale = new Vector3(BaseWidth * 1f, 0.02f, 0.02f);
                if (i == 0)
                {
                    a.localScale = new Vector3(BaseWidth * 1.1f, 0.04f, GraphDepth * 1.4f);
                    a.GetComponent<Renderer>().material = Background.GetComponent<Renderer>().material;
                }
                else
                {
                    a.GetComponent<Renderer>().materials[0].color = SeperatorColor;
                    a.GetComponent<Renderer>().materials[0].SetColor("_EmissionColor", SeperatorColor);

                    if (AxisText.AxisTextActive)
                    {

                        Transform nParent = (GameObject.Instantiate(Resources.Load("ChartsAndGraphs3D/Empty"), a) as GameObject).transform;

                        nParent.transform.localPosition = new Vector3(a.localScale.x * 0.33f, 0, 0);
                        nParent.localScale = new Vector3(1 / a.lossyScale.x, 1 / a.lossyScale.y, 1 / a.lossyScale.z);
                        nParent.localScale /= nParent.localScale.magnitude;

                        GameObject text3Dobj = Instantiate(Resources.Load("ChartsAndGraphs3D/3DText"), nParent) as GameObject;
                        float valueCurrBar = (currMaxY / AxisAmount) * i;
                        text3Dobj.GetComponent<TextMesh>().text = AxisText.AxisPreText + System.Math.Round(valueCurrBar, 2) + AxisText.AxisPostText;
                        text3Dobj.transform.localScale = new Vector3(AxisText.AxisTextScale, AxisText.AxisTextScale, AxisText.AxisTextScale);
                        text3Dobj.transform.LookAt(text3Dobj.transform.position + transform.forward);
                        AxisText.currentObjects.Add(nParent.gameObject);
                    }
                }

                CurrAxis.Add(a);
            }

            Background.transform.localScale = new Vector3(BaseWidth * 1.1f, BaseHeight * 1.05f, 0.05f);
            Background.transform.localPosition = new Vector3(BaseWidth / 2, (BaseHeight * 1.05f) / 2 - 0.02f, GraphDepth * 0.7f);
        }

        public void SetColor()
        {

            for (int i = 0; i < CurrBars.Count; i++)
            {
                Renderer Renderer = CurrBars[i].GetComponent<Renderer>();
                //Color c = Renderer.materials[0].GetColor("_EmissionColor");

                //c.r += i * 0.3f;
                Color c = BarColors[Mathf.Clamp(i, 0, BarColors.Count() - 1)];


                Renderer.materials[0].color = c;
                Renderer.materials[0].SetColor("_EmissionColor", c);
            }
        }


    }

    [Serializable]
    public class BarAxisTextHelper
    {
        /// <summary>
        /// Should there be a text, displaying the current value of that height, next to the seperator-lines?
        /// </summary>
        public bool AxisTextActive = true;

        public string AxisPreText;

        public string AxisPostText;

        public float AxisTextScale = 3.5f;

        public bool onlyWholeNumbers = true;

        internal List<GameObject> currentObjects = new List<GameObject>();
    }
}