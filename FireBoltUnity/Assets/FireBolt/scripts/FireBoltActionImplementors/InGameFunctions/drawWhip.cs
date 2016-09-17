using UnityEngine;
using System.Collections;
using LN.Utilities;
using System.Collections.Generic;

namespace Assets.scripts
{
    public class drawWhip : MonoBehaviour
    {

        public LineRenderer lineRenderer;
        public Material whipMaterial;
        private float counter = 0f;
        private float dist;
        private float lineDrawSpeed = 6f;
        private GameObject parent;
        private Transform source;
        private Transform sink;
        private string color;
        private Color brown = new Color(122, 82, 48, 0);
        public float width = .04f;
        private bool addedComponent = false;
        private int delay = 0;
        void Awake()
        {
            
        }



        public void Init(List<Tuple<string, string, string>> paramNames)
        {
            if (!addedComponent)
            {
                foreach (Tuple<string, string, string> tuple in paramNames)
                {
                    if (tuple.Item1.Equals("source"))
                    {
                        Debug.Log("Source Parent: " + tuple.Item3);
                        parent = GameObject.Find(tuple.Item3);
                        //source = getHand(parent);
                        source = parent.transform;
                    }
                    if (tuple.Item1.Equals("sink"))
                    {
                        sink = GameObject.Find(tuple.Item3).transform;
                    }
                    if (tuple.Item1.Equals("color"))
                    {
                        color = tuple.Item3;
                    }
                }

                
                Debug.Log("Init Draw Line: " + source.ToString() + " " + sink.ToString());
                lineRenderer = parent.AddComponent<LineRenderer>();
                lineRenderer.material = whipMaterial;

                lineRenderer.SetPosition(0, source.position);
                lineRenderer.SetColors(brown, brown);
                addedComponent = true;
            }
        }


        public void Undo()
        {
            counter = 0f;
        }

        public void Skip()
        {
            counter = dist;
        }

        public void Execute()
        {

            lineRenderer.SetWidth(width, width);
            source = parent.transform;
            dist = Vector3.Distance(source.position, sink.position);
            if (delay++ > 0) //delay the whip
            {
                if (counter < dist)
                {
                    counter += .1f / lineDrawSpeed;
                    float x = Mathf.Lerp(0, dist, counter);

                    Vector3 pointA = source.position;
                    Vector3 pointB = sink.position;

                    Vector3 pointAlongLine = x * Vector3.Normalize(pointB - pointA) + pointA;

                    lineRenderer.SetPosition(1, pointAlongLine);
                }
                else
                {
                    lineRenderer.SetPosition(0, source.position);
                    lineRenderer.SetPosition(1, sink.position);
                }
            }
            else
            {
                lineRenderer.SetPosition(0, source.position);
                lineRenderer.SetPosition(1, source.position);
            }
        }


    }
}