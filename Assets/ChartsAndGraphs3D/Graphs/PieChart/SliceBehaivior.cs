using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChartsAndGraphs3D
{
    [ExecuteInEditMode]
    public class SliceBehaivior : MonoBehaviour
    {
        public Slice info;

        public bool CreatedInEditMode;

        float AimRotation
        {
            get
            {
                return info.ID * 3.6f;
            }
        }
        float AimHeight
        {
            get
            {
                return info.Height * 0.1f;
            }
        }
        bool fixPosRot;

        Vector3 startScale = new Vector3(1, 1, 1);

        // Use this for initialization
        void Start()
        {

            //Workaround -> so you can use colors in the Unity Editor
            if (!Application.isPlaying)
            {
                var tempMaterial = new Material(GetComponent<Renderer>().sharedMaterial);
                tempMaterial.color = info.color;
                tempMaterial.SetColor("_EmissionColor", info.color);
                GetComponent<Renderer>().sharedMaterial = tempMaterial;
            }
            else
            {
                GetComponent<Renderer>().material.color = info.color;
                GetComponent<Renderer>().material.SetColor("_EmissionColor", info.color);
            }
            startScale = transform.localScale;
        }

        // Update is called once per frame
        void Update()
        {
            if (CreatedInEditMode && !info.parent.Slices.Contains(info))
            {
                DestroyImmediate(gameObject);
                return;
            }

            if (fixPosRot)
                return;


            if (info.parent.Slices.Where(s => s.PartID == info.PartID).All(s => s.isBuildUp))
            {
                if (AimHeight > transform.localPosition.y)
                {
                    transform.localPosition += new Vector3(0, (AimHeight - transform.localPosition.z) * info.BuildUpSpeed * Mathf.Min(0.1f, Time.deltaTime), 0);
                    transform.localScale += new Vector3(0, 0, (AimHeight - transform.localPosition.z) * info.BuildUpSpeed * Mathf.Min(0.1f, Time.deltaTime));
                }
                else if (!fixPosRot)
                {
                    FixAll();
                }
            }

            if (Mathf.Abs(AimRotation - transform.localRotation.eulerAngles.y) > 3.6f)
            {
                transform.Rotate(new Vector3(0, 0, Mathf.Max((AimRotation - transform.localRotation.eulerAngles.y), 30) * info.BuildUpSpeed * Mathf.Min(0.1f, Time.deltaTime)), Space.Self);
            }
            else
            {
                info.parent.Slices[info.ID].isBuildUp = true;
                transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, AimRotation, transform.localRotation.eulerAngles.z);
            }
        }

        public void FixAll()
        {
            fixPosRot = true;
            transform.localPosition = new Vector3(0, AimHeight, 0);
            transform.localScale = startScale + new Vector3(0, 0, AimHeight);
            transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, AimRotation, transform.localRotation.eulerAngles.z);

        }
    }
}