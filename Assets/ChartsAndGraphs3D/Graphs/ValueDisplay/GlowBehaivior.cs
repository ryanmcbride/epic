using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChartsAndGraphs3D
{
    public class GlowBehaivior : MonoBehaviour
    {
        Renderer Renderer;

        // Update is called once per frame
        void Update()
        {

        }

        public void Glow(bool glow, Color c)
        {
            if (glow)
            {
                Renderer.materials[0].EnableKeyword("_EMISSION");
                Renderer.materials[0].SetColor("_EmissionColor", c);
                Renderer.materials[0].color = c;
            }
            else
            {
                Renderer.materials[0].DisableKeyword("_EMISSION");
                Renderer.materials[0].SetColor("_EmissionColor", c);
                Renderer.materials[0].color = c;
            }
        }

        internal void SetEmissionColor(int i, Color c)
        {
            if (Renderer == null)
                Renderer = GetComponentInChildren<Renderer>();

            c.b += i / 40f;

            Renderer.materials[0].SetColor("_EmissionColor", c);
        }
    }
}