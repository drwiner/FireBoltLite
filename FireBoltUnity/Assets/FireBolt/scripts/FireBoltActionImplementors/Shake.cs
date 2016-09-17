using UnityEngine;

namespace Assets.scripts
{
    public class Shake : FireBoltAction
    {
        float shakeValue;
        string cameraName;
        GameObject actor;
        ShakeCam shakeCam;

        private Vector3 localPosition;
        private Quaternion localRotation;

        public static bool ValidForConstruction(string actorName)
        {
            if (string.IsNullOrEmpty(actorName))
                return false;
            return true;
        }

        public Shake(float startTick, float endTick, string cameraName, float shakeValue) :
            base(startTick,endTick)
        {
            this.cameraName = cameraName;
            this.shakeValue = shakeValue;
        }

        public override bool Init()
        {

            if (actor == null &&
                !getActorByName(cameraName, out actor))
            {
                Debug.LogError("actor name [" + cameraName + "] not found. cannot shake");
                return false;
            }

            return true;
        }

        public override void Execute(float currentTime)
        {
            var rotation = Quaternion.Euler(localRotation.eulerAngles + Vector3.Scale(SmoothRandom.GetVector3(shakeValue), new Vector3(4.0f, 4.0f, 4.0f)));
            actor.transform.localRotation = rotation;

            actor.transform.localPosition = localPosition + Vector3.Scale(SmoothRandom.GetVector3(shakeValue), new Vector3(0.1f, 0.1f, 0.1f));
        }

		public override void Undo()
		{
            //intentionally blank
        }

        public override void Skip()
        {
            //intentionally blank
        }

        public override void Stop()
        {
            //intentionally blank
        }
    }
}
