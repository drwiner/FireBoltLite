using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.scripts
{
   
    public class InputSpecifier : MonoBehaviour
    {
        

        public InputField storyField;
        public InputField modelField;
        public InputField cameraField;

        void Start()
        {
            setNewDefaults();
        }

        public void StartFireBolt()
        {
            InputSet input = new InputSet();
            if (!string.IsNullOrEmpty(storyField.text))
            {
                input.StoryPlanPath = storyField.text;
            }

            if (!string.IsNullOrEmpty(modelField.text))
            {
                input.CinematicModelPath = modelField.text;
            }

            if (!string.IsNullOrEmpty(cameraField.text))
            {
                input.CameraPlanPath = cameraField.text;
            }

            ElPresidente.Instance.Init(input, null, false, true);
        }

        void setNewDefaults()
        {
            storyField.text = "storyPlans/shakespeare.xml";
            modelField.text = "cinematicModels/DotaHierarchyModel.xml";
            cameraField.text = "cameraPlans/dotaCamera.xml";
        }

        
    }
}
