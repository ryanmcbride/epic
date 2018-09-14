using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChartsAndGraphs3D
{
    public class TextRow : MonoBehaviour
    {
        public TextRowInfo info;

        public TextMesh DisplayText;
        public Renderer Cube;


        public void SetValues(TextRowInfo i)
        {
            info = i;

            DisplayText.text = info.PreText + info.Value.ToString() + info.PostText;
            Cube.material.color = info.c;
        }


        // Update is called once per frame
        void Update()
        {

        }
    }


    public struct TextRowInfo
    {
        public int ID;
        public string PreText;
        public float Value;
        public string PostText;
        public Color c;
    }
}