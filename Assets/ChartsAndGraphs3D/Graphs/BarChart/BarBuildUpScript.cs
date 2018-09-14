using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChartsAndGraphs3D
{
    public class BarBuildUpScript : MonoBehaviour
    {

        public float speed, scale_speed;

        public Vector3 AimPos, AimScale;
        bool pos_fix, scale_fix;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if ((AimScale - transform.localScale).magnitude > 0.01f)
            {
                transform.localScale += (AimScale - transform.localScale) * scale_speed * Mathf.Min(0.1f, Time.deltaTime);
            }
            else if (!scale_fix)
            {
                scale_fix = true;
                transform.localScale = AimScale;
            }

            if ((AimPos - transform.localPosition).magnitude > 0.01f)
            {
                transform.localPosition += (AimPos - transform.localPosition) * speed * Mathf.Min(0.1f, Time.deltaTime);
            }
            else if (!pos_fix)
            {
                pos_fix = true;
                transform.localPosition = AimPos;
            }
        }

        internal void UpdateValues(Vector3 pos, Vector3 scale)
        {
            pos_fix = false;
            scale_fix = false;
            AimPos = pos;
            AimScale = scale;
            transform.localPosition = new Vector3(AimPos.x, transform.localPosition.y, transform.localPosition.z);
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, AimScale.z);
        }
    }
}