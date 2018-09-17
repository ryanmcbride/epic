using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChartsAndGraphs3D
{
    public delegate Vector3 Vector3Method();

    public class TextDisplay : MonoBehaviour
    {
        public enum RowVerticalAlignment {
            Center,
            Top,
            Bottom
        }
        
        internal List<TextRow> Rows = new List<TextRow>();

        List<TextRowInfo> LastInfos = new List<TextRowInfo>();

        public GameObject RowPrefab;
        TextRowInfo Info;

        internal TextDisplayCreator creator;

        /// <summary> 
        /// Pre- and Post-Text will most of the time just reference to the Pre / Posttext set in Unity (TextDisplayCreator)
        /// </summary>
        /// <returns>A list of all data to display</returns>
        public delegate List<TextRowInfo> InfoGetter();

        public event InfoGetter InfoGetterEvent;

        public RowVerticalAlignment rowVerticalAlignment = RowVerticalAlignment.Center;

        public Vector3 Position;

        private void Update()
        {
            transform.localPosition = Position;

            //adjust the empty parent between Chart and TextDisplay
            //if you dont do this the Text will stretch and will eventually be unreadable
            if (transform.parent.parent.lossyScale.x == 0 || transform.parent.parent.lossyScale.y == 0 || transform.parent.parent.lossyScale.z == 0)
                transform.parent.localScale = Vector3.zero;
            else
                transform.parent.localScale = new Vector3(1 / transform.parent.parent.lossyScale.x, 1 / transform.parent.parent.lossyScale.y, 1 / transform.parent.parent.lossyScale.z);

            transform.parent.localScale /= transform.parent.localScale.magnitude;

            transform.LookAt(transform.position + creator.LookAtMethod());
            ////look same direction as chart
            //if (creator.LookAtSameDirectionAsChart)
            //{
            //    transform.LookAt(transform.position + transform.parent.parent.forward);
            //}
            //else            //it s a PieChart
            //{
            //    //transform.LookAt(transform.position + Vector3.Cross(creator.WhereIsYourChartLooking, Vector3.up));
            //    transform.LookAt(transform.position + Vector3.Cross(transform.position - transform.parent.parent.position, Vector3.up));
            //}

            //get the current Infos to Display and check if you have to change anything
            List<TextRowInfo> newInfos = InfoGetterEvent();
            if (!LastInfos.SequenceEqual(newInfos))
            {
                LastInfos = newInfos;
                //print(LastInfos.Sum(x => x.Value) + "  " + LastInfos.Count);
                ResetRows();
                AddRows(LastInfos);
            }
        }

        public void SetTexts(string pre, string post)
        {
            Info.PreText = pre;
            Info.PostText = post;
        }

        void AddRows(List<TextRowInfo> infos)
        {
            for (int i = 0; i < infos.Count; i++)
            {
                AddRow(infos[i]);
            }
        }

        void AddRows(List<float> vals, List<Color> c)
        {
            for (int i = 0; i < vals.Count; i++)
            {
                AddRow(vals[i], c[i]);
            }
        }

        public int AddRow(float val, Color c)
        {
            Info.Value = val;
            Info.c = c;

            return AddRow(Info);
        }

        public int AddRow(TextRowInfo info)
        {
            if (Rows.Count == 0)
                info.ID = 0;
            else
                info.ID = Rows.Max(x => x.info.ID) + 1;

            Transform tr = Instantiate(RowPrefab, transform).transform;
            TextRow row = tr.GetComponent<TextRow>();

            row.SetValues(info);

            Rows.Add(row);

            CheckPositions();

            return info.ID;
        }

        private void CheckPositions()
        {
            var totalHeight = Rows.Count * creator.TextRowOffset;
            var num = Rows.Count - 1;
            for (int i = 0; i < Rows.Count; i++)
            {
                float y = 0;
                switch (rowVerticalAlignment) {
                    case RowVerticalAlignment.Top:    y = i * -creator.TextRowOffset;                break;
                    case RowVerticalAlignment.Center: y = (i - (num / 2f)) * -creator.TextRowOffset; break;
                    case RowVerticalAlignment.Bottom: y = (i - num) * -creator.TextRowOffset;        break;
                }
                Rows[i].transform.localPosition = new Vector3(0f, y, 0f);
             }
        }

        public void ResetRows()
        {
            foreach (var item in Rows)
            {
                Destroy(item.gameObject);
            }
            Rows.Clear();
        }

        public void UpdateRow(int id, float val, Color c)
        {
            Info.Value = val;
            Info.c = c;
            TextRow row = Rows.Where(x => x.info.ID == id).FirstOrDefault();
            row.SetValues(Info);
        }

    }

    [Serializable]
    public class TextDisplayCreator
    {
        public bool TextDisplayActive = true;

        /// <summary>
        /// Relative to parent
        /// </summary>
        public Vector3 Position = new Vector3(3, 4, 0);
        /// <summary>
        /// The text coming before the Value => Price:  | Height: 
        /// </summary>
        public string PreText;

        public float something = 123.0f;

        public float Scale = 1f;

        public float TextRowOffset = 0.8f;
        /// <summary>
        /// The text coming after the Value =>  €   |   cm
        /// </summary>
        public string PostText;
        
        public TextDisplay.RowVerticalAlignment rowVerticalAlignment = TextDisplay.RowVerticalAlignment.Center;

        internal TextDisplay TextDisplay;
        internal GameObject go;

        internal Vector3Method LookAtMethod;

        /// <summary>
        /// Create the TextDisplay (TextDisplayActive has to be set)
        /// </summary>
        /// <param name="parent">Most of the time this is your Chart</param>
        /// <param name="pos">Position relative to the Chart</param>
        /// <param name="InfoGetterMethod">The Method where all Infos are coming from (see Examples)</param>
        public void Create(Transform parent, Vector3 pos, TextDisplay.InfoGetter InfoGetterMethod, Vector3Method lookAtMethod)
        {
            if (!TextDisplayActive || !Application.isPlaying)
                return;

            //create an empty parent between Chart and TextDisplay
            //if you dont do this the Text will stretch and will eventually be unreadable
            Transform nParent = (GameObject.Instantiate(Resources.Load("ChartsAndGraphs3D/Empty"), parent) as GameObject).transform;
            nParent.localScale = new Vector3(1 / parent.lossyScale.x, 1 / parent.lossyScale.y, 1 / parent.lossyScale.z);
            nParent.localScale /= nParent.localScale.magnitude;

            //create the TextDisplay
            go = GameObject.Instantiate(Resources.Load("ChartsAndGraphs3D/TextDisplay"), nParent) as GameObject;

            //adjust and give infos to it
            go.transform.localPosition = pos;
            go.transform.localScale = new Vector3(Scale, Scale, Scale);
            TextDisplay = go.GetComponent<TextDisplay>();
            TextDisplay.creator = this;
            TextDisplay.Position = pos;
            TextDisplay.rowVerticalAlignment = rowVerticalAlignment;
            TextDisplay.InfoGetterEvent += InfoGetterMethod;
            LookAtMethod = lookAtMethod;
        }
    }


    [Serializable]
    public class TitleText
    {
        public bool Active = true;

        public Vector3 PositionOffset;

        public string Text;
        public float Scale = 0.18f;
        internal Transform go;
        public TextAnchor textAnchor = TextAnchor.MiddleLeft;

        public void Create(Transform parent, Vector3Method posMethod, Vector3Method directionMethod)
        {
            if (Active && Application.isPlaying)
            {
                Transform nParent;
                GameObject text3Dobj;

                nParent = (GameObject.Instantiate(Resources.Load("ChartsAndGraphs3D/Empty"), parent) as GameObject).transform;
                text3Dobj = GameObject.Instantiate(Resources.Load("ChartsAndGraphs3D/3DText"), nParent) as GameObject;


                nParent.transform.localPosition = posMethod();
                nParent.localScale = new Vector3(1 / parent.lossyScale.x, 1 / parent.lossyScale.y, 1 / parent.lossyScale.z);
                nParent.localScale /= nParent.localScale.magnitude;

                go = nParent;

                text3Dobj.GetComponent<TextMesh>().text = Text;
                text3Dobj.GetComponent<TextMesh>().anchor = textAnchor;
                text3Dobj.transform.localScale = new Vector3(Scale, Scale, Scale);
                text3Dobj.transform.LookAt(text3Dobj.transform.position + directionMethod());
            }
        }
    }
}