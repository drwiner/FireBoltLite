using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM=CinematicModel;
using LN.Utilities.Collections;

namespace Assets.scripts
{
    public class Focus : FireBoltAction
    {
        string cameraName;
        string targetName;
        GameObject target;
        CameraBody cameraBody;
        bool tracking, executed = false;

        public static bool ValidForConstruction(string actorName)
        {
            if (string.IsNullOrEmpty(actorName))
                return false;
            return true;
        }

        public Focus(float startTick, float endTick, string cameraName, string targetName, bool tracking=false) :
            base(startTick, endTick)
        {
            this.cameraName = cameraName;
            this.targetName = targetName;
            this.tracking = tracking;
        }

        public override bool Init()
        {
            //get camera
            
            GameObject camera;
            if (!getActorByName(cameraName, out camera))
            {
                Debug.LogError("actor name [" + cameraName + "] not found. cannot change focus");
                return false;
            }
            cameraBody = camera.GetComponent<CameraBody>() as CameraBody;

            //try to parse target as a coordinate
            Vector3 focusPosition;
            if (targetName.TryParseVector3(out focusPosition))
            {
                Debug.Log("focus @" + focusPosition);
                return true;
            }

            //try to find the target as an actor
            if (target == null &&
                !getActorByName(targetName, out target))
            {
                Debug.Log("actor name [" + targetName + "] not found. cannot change focus");
                return false;
            }
            Debug.Log(string.Format("focus target[{0}] @{1} tracking[{2}]", targetName, target.transform.position, tracking));
            return true;
        }

        public override void Execute(float currentTime)
        {
            if (target == null)
            {
                target = GameObject.Find(targetName);
            }

            if (tracking || !executed)
            {
                Vector3 focusPosition;
                if (target != null)
                {
                    cameraBody.FocusDistance = Vector3.Distance(cameraBody.NodalCamera.transform.position, target.transform.position);
                }
                else if(targetName.TryParseVector3(out focusPosition))
                {
                    cameraBody.FocusDistance = Vector3.Distance(cameraBody.NodalCamera.transform.position, focusPosition);                    
                }                
                executed = true;
            }        
        }

		public override void Undo()
		{
            executed = false;
		}

        public override void Skip()
        {

        }

        public override void Stop()
        {
            //nothing to stop
        }

    }
}
