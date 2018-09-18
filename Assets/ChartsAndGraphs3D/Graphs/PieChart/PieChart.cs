using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChartsAndGraphs3D
{
    [ExecuteInEditMode]
    public class PieChart : MonoBehaviour
    {
        public TitleText TitleText;

        public List<Color> PartColors = new List<Color>() { Color.blue, Color.red, Color.green };

        public GameObject middleSlice, startSlice, endSlice, fullSlice;

        public List<Part> Parts = new List<Part>();

        public float BuildUpSpeed, HeightOffsetPerPart;

        public bool instantBuildUp;

        public float FullValue { get { return Parts.Sum(x => x.Value); } }
        public bool Flat;
        public bool Glow;

        //An easy to use Object for TextDisplay Creation
        //just reference to it like this and call Create in Start (or somewhere else)
        //if TextDisplayActive IS NOT SET -> Create will abort and wont create a TextDisplay
        public TextDisplayCreator TextDisplayCreator;

        bool PartsNeedUpdate = true;
        List<int> pParts = new List<int>();      //values to %
        internal List<Slice> Slices = new List<Slice>();

        public Transform SlicesParent;


        List<float> PartsEditor = new List<float>();      //values to %


        Color GetPartColor(int index) {
          return PartColors.Count > 0 ? PartColors[index % PartColors.Count] : Color.magenta;
        }
        int GetPartPercentage(int index) {
          return pParts.Count > 0 ? pParts[index % pParts.Count] : 0;
        }

        // Use this for initialization
        void Start()
        {
            InitIDs();
            TextDisplayCreator.Create(transform, TextDisplayCreator.Position, InfoGetterMethod, LookAtMethod);
            TitleText.Create(transform, TitlePositionMethod, LookAtMethod);
            if(SlicesParent == null) SlicesParent = transform;
        }

        private Vector3 TitlePositionMethod()
        {
            return TitleText.PositionOffset;
        }

        //if you turn your gameobject by for example 90 degrees you might want to modify this
        //else you want to leave it as it is
        private Vector3 LookAtMethod()
        {
            return transform.forward;
        }

        //An easy example for the InfoGetter Method
        private List<TextRowInfo> InfoGetterMethod()
        {
            List<TextRowInfo> list = new List<TextRowInfo>();
            for (int i = 0; i < Parts.Count; i++)
            {
                var partColor = GetPartColor(i);
                list.Add(new TextRowInfo() { PreText = Parts[i].Text, PostText = TextDisplayCreator.PostText, Value = GetPartPercentage(i), c = partColor });
            }
           return list;
        }

        bool LastGlow;  //Only used below

        void InitIDs()
        {
            for (int i = 0; i < Parts.Count(); i++)
            {
                Parts[i].ID = i;
            }
        }

        void Update()
        {
            if (PartsNeedUpdate)
            {
                PartsNeedUpdate = false;
                UpdatepParts();
                UpdateSlices(instantBuildUp);
            }
            else if ((!Application.isPlaying && (PartsEditor.Sum() != Parts.Sum(x => x.Value)) || LastGlow != Glow))
            {
                LastGlow = Glow;
                PartsEditor.Clear();
                foreach (var item in Parts)
                {
                    PartsEditor.Add(item.Value);
                }
                UpdatepParts();
                UpdateSlices(true);
            }
            else if ((Application.isEditor && PartsEditor.Sum() != Parts.Sum(x => x.Value)))
            {
                PartsEditor.Clear();
                foreach (var item in Parts)
                {
                    PartsEditor.Add(item.Value);
                }
                UpdatepParts();
                UpdateSlices(instantBuildUp);
            }
        }


        private void UpdateSlices(bool instant)
        {
            foreach (Slice item in Slices)
            {
                if (!Application.isPlaying)
                    DestroyImmediate(item.go);
                else
                    Destroy(item.go);
            }

            Slices.Clear();

            for (int i = 0; i < Parts.Count(); i++)
            {
                if (pParts[i] == 0)
                    continue;
                else if (pParts[i] == 1)
                {
                    CreateSlice(i, fullSlice, instant);
                }
                else
                {
                    for (int j = 0; j < pParts[i]; j++)
                    {
                        if (j == 0)
                            CreateSlice(i, startSlice, instant);
                        else if (j == pParts[i] - 1)
                            CreateSlice(i, endSlice, instant);
                        else
                            CreateSlice(i, middleSlice, instant);
                    }
                }
            }
        }

        private void CreateSlice(int i, GameObject prefab, bool instant)
        {
            GameObject g = Instantiate(prefab, SlicesParent);
            g.transform.localRotation = Quaternion.Euler(-90,0,0);

            //set height -> different Parts are different high
            float h = i * HeightOffsetPerPart;

            //flatten
            if (Flat)
                h = 0;

            Slices.Add(new Slice() { ID = Slices.Count(), PartID = Parts[i].ID, Height = h, color = GetPartColor(i), go = g, BuildUpSpeed = BuildUpSpeed, parent = this });

            g.GetComponent<SliceBehaivior>().info = Slices.LastOrDefault();


            if (Application.isPlaying)
                HandleGlow(g.GetComponent<Renderer>().material, Glow, GetPartColor(i));
            else
            {
                HandleGlow(g.GetComponent<Renderer>().sharedMaterial, Glow, GetPartColor(i));
                g.GetComponent<SliceBehaivior>().CreatedInEditMode = true;
            }


            if (instant)
                g.GetComponent<SliceBehaivior>().FixAll();
        }

        void HandleGlow(Material r, bool on, Color c)
        {

            if (on)
                r.EnableKeyword("_EMISSION");
            else
                r.DisableKeyword("_EMISSION");


            r.color = c;
            r.SetColor("_EmissionColor", c);
        }

        private void UpdatepParts()
        {
            pParts.Clear();
            if(Parts.Count > 0) {
                foreach (Part item in Parts)
                {
                    pParts.Add(Math.Max((int)((item.Value / FullValue) * 100), 1));         //At least 1
                }
                pParts[pParts.IndexOf(pParts.Max())] += 100 - pParts.Sum();         //modify the biggest => make it full 100%
           }
        }

        /// <summary>
        /// Add a Part
        /// </summary>
        /// <returns>Returns the ID of the created Part</returns>
        public int AddPart(float value, string text = "")
        {
            int id = (Parts.Max(x => x.ID) + 1);
            Parts.Add(new Part() { ID = id, Value = value, Text = text });
            PartsNeedUpdate = true;
            return id;
        }

        public void UpdatePart(int id, float value, string text = "")
        {
            PartsNeedUpdate = true;
            Parts.Where(x => x.ID == id).FirstOrDefault().Value = value;
            Parts.Where(x => x.ID == id).FirstOrDefault().Text = text;
        }

        public void RemovePart(int id)
        {
            PartsNeedUpdate = true;
            Parts.Remove(Parts.Where(x => x.ID == id).FirstOrDefault());
        }
    }

    [Serializable]
    public class Part
    {
        internal int ID;
        public float Value;
        public string Text;
    }
    [Serializable]
    public class Slice
    {
        public int ID;
        public int PartID;
        public Color color;
        public GameObject go;
        public float Height;
        public float BuildUpSpeed;
        public bool isBuildUp;
        public PieChart parent;
    }
}