using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ChartsAndGraphs3D
{
    public class ValueDisplay : MonoBehaviour
    {
        public TitleText TitleText;

        public Color BorderColor = new Color(0f, 0.4206896f, 1f), ValueBarColor = Color.blue, ValueBarNotActiveColor = new Color(0.2f, 0.2f, 0.2f);

        public float Val = 5;

        public float MaxVal;

        public bool Glow = true;

        public ValueCurrentTextUnityHelper CurrentValueText;
        public ValueBarMaxTextUnityHelper MaxValueText;



        public GameObject Border;
        public List<Transform> ValueBars = new List<Transform>();


        List<GlowBehaivior> ValueBarsGlow = new List<GlowBehaivior>();
        private float Value
        {
            get
            {
                return Val;
            }

            set
            {
                if (Val == value)
                    return;

                Val = Mathf.Clamp(value, 0, MaxVal);

                Changed = true;
            }
        }

        bool Changed = true;

        // Use this for initialization
        void Start()
        {
            TitleText.Create(transform, TitlePositionMethod, LookAtMethod);
            UpdateTexts();
        }

        private Vector3 TitlePositionMethod()
        {
            return TitleText.PositionOffset;
        }

        private Vector3 LookAtMethod()
        {
            return transform.forward;
        }

        void ToggleGlow(bool Glow, Renderer r)
        {
            if (Glow)
                r.materials[0].EnableKeyword("_EMISSION");
            else
                r.materials[0].DisableKeyword("_EMISSION");
        }

        public void SetValue(float val)
        {
            Value = val;
        }

        public void IncreaseValue(float val)
        {
            Value += val;
        }

        public void DecreaseValue(float val)
        {
            IncreaseValue(-val);
        }
        // Update is called once per frame
        void Update()
        {
            if (!Changed)
                return;

            UpdateTexts();

            if (Border.GetComponent<Renderer>() != null)
            {
                ToggleGlow(Glow, Border.GetComponent<Renderer>());
            }

            foreach (var item in Border.GetComponentsInChildren<Renderer>())
            {
                ToggleGlow(Glow, item);
            }

            if (Border.GetComponent<Renderer>() != null)
            {
                Border.GetComponent<Renderer>().materials[0].color = BorderColor;
                Border.GetComponent<Renderer>().materials[0].SetColor("_EmissionColor", BorderColor);
            }
            else
            {
                foreach (var item in Border.GetComponentsInChildren<Renderer>())
                {
                    item.materials[0].color = BorderColor;
                    item.materials[0].SetColor("_EmissionColor", BorderColor);
                }
            }

            for (int i = 0; i < ValueBars.Count; i++)
            {
                ValueBarsGlow.Add(ValueBars[i].GetComponent<GlowBehaivior>());
                ValueBarsGlow[i].SetEmissionColor(i, ValueBarColor);
            }

            for (int i = 0; i < 20; i++)
            {
                if (i > 20 * (Value / MaxVal))
                {
                    ValueBarsGlow[i].Glow(false, ValueBarNotActiveColor);
                }
                else
                {
                    ValueBarsGlow[i].Glow(true && Glow, ValueBarColor);
                }
            }

        }

        void UpdateTexts()
        {

            if (MaxValueText.Active)
            {
                if (MaxValueText.currentObject != null)
                {
                    Destroy(MaxValueText.currentObject);
                    MaxValueText.currentObject = null;
                }

                Transform nParent = (GameObject.Instantiate(Resources.Load("ChartsAndGraphs3D/Empty"), transform) as GameObject).transform;

                nParent.transform.localPosition = MaxValueText.position;
                nParent.localScale = new Vector3(1 / transform.lossyScale.x, 1 / transform.lossyScale.y, 1 / transform.lossyScale.z);
                nParent.localScale /= nParent.localScale.magnitude;


                GameObject text3Dobj = Instantiate(Resources.Load("ChartsAndGraphs3D/3DText"), nParent) as GameObject;
                float MaxValue = MaxVal;
                text3Dobj.GetComponent<TextMesh>().text = MaxValueText.preText + System.Math.Round(MaxValue, 2) + MaxValueText.postText;
                text3Dobj.transform.localScale = new Vector3(MaxValueText.Scale, MaxValueText.Scale, MaxValueText.Scale);
                text3Dobj.transform.LookAt(text3Dobj.transform.position + transform.forward);
                MaxValueText.currentObject = nParent.gameObject;

            }

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
                }
                else
                {
                    text3Dobj = CurrentValueText.currentTextMesh;
                }


                nParent.transform.localPosition = CurrentValueText.position;
                nParent.localScale = new Vector3(1 / transform.lossyScale.x, 1 / transform.lossyScale.y, 1 / transform.lossyScale.z);
                nParent.localScale /= nParent.localScale.magnitude;





                text3Dobj.GetComponent<TextMesh>().text = CurrentValueText.preText + System.Math.Round(Value, 2) + CurrentValueText.postText;
                text3Dobj.transform.localScale = new Vector3(CurrentValueText.Scale, CurrentValueText.Scale, CurrentValueText.Scale);
                text3Dobj.transform.LookAt(text3Dobj.transform.position + transform.forward);
                CurrentValueText.currentObject = nParent.gameObject;

            }
        }

    }

    [Serializable]
    public class ValueBarMaxTextUnityHelper
    {
        public bool Active = true;
        public string preText;
        public string postText;
        public float Scale = 0.1f;
        internal GameObject currentObject;
        public Vector3 position;
    }

    [Serializable]
    public class ValueCurrentTextUnityHelper
    {
        public bool Active = true;
        public string preText;
        public string postText;
        public float Scale = 0.1f;
        internal GameObject currentObject;
        internal GameObject currentTextMesh;
        public Vector3 position;
    }
}