using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.scripts
{
    public class InputSet
    {
        private static readonly string STORY_PLAN_PATH_DEFAULT = "storyPlans/defaultStory.xml";
        private static readonly string CAMERA_PLAN_PATH_DEFAULT = "cameraPlans/defaultCamera.xml";
        private static readonly string CINEMATIC_MODEL_PATH_DEFAULT = "cinematicModels/defaultModel.xml";

        public InputSet()
        {
            this.StoryPlanPath = STORY_PLAN_PATH_DEFAULT;
            this.CameraPlanPath = CAMERA_PLAN_PATH_DEFAULT;
            this.CinematicModelPath = CINEMATIC_MODEL_PATH_DEFAULT;

        }

        public string StoryPlanPath { get; set; }
        public string CameraPlanPath { get; set; }
        public string CinematicModelPath { get; set; }
    }
}
