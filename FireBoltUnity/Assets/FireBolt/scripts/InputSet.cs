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
        private static readonly string ACTORS_AND_ANIMATIONS_BUNDLE_PATH_DEFAULT = "AssetBundles/actorsandanimations";
        private static readonly string TERRAIN_BUNDLE_PATH_DEFAULT = "AssetBundles/terrain";
       // private static readonly string ACTORS_AND_ANIMTATIONS2 = "AssetBundles/actorsandanimations2";

        public InputSet()
        {
            this.StoryPlanPath = STORY_PLAN_PATH_DEFAULT;
            this.CameraPlanPath = CAMERA_PLAN_PATH_DEFAULT;
            this.CinematicModelPath = CINEMATIC_MODEL_PATH_DEFAULT;
            this.ActorsAndAnimationsBundlePath = ACTORS_AND_ANIMATIONS_BUNDLE_PATH_DEFAULT;
            this.TerrainBundlePath = TERRAIN_BUNDLE_PATH_DEFAULT;
        }

        public string StoryPlanPath { get; set; }
        public string CameraPlanPath { get; set; }
        public string CinematicModelPath { get; set; }
        public string ActorsAndAnimationsBundlePath { get; set; }
        public string TerrainBundlePath { get; set; }

    }
}
