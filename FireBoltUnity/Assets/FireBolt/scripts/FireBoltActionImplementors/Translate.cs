using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM=CinematicModel;
using LN.Utilities.Collections;
using LN.Utilities;

namespace Assets.scripts
{
    public class Translate : FireBoltAction
    {
        string actorName;
        protected GameObject actor;
        /// <summary>
        /// intended position of the actor when the interval begins
        /// </summary>
        protected Vector3 origin;
        /// <summary>
        /// intended position of the actor when the interval ends
        /// </summary>
        Vector3Nullable destination;

        Vector3? possibleOrigin;
        bool initialized = false;

        public static bool ValidForConstruction(string actorName)
        {
            if (string.IsNullOrEmpty(actorName))
                return false;
            return true;
        }

        public Translate(float startTick, float endTick, string actorName,  Vector3? possibleOrigin, Vector3Nullable destination) :
            base(startTick, endTick)
        {
            this.actorName = actorName;
            this.possibleOrigin = possibleOrigin;
            this.destination = destination;
        }

        public override bool Init()
        {
            if (initialized)
            {
                actor.SetActive(true);
                return true;
            }
                            
            if(actor == null && 
                !getActorByName(actorName, out actor))
            {
                Debug.LogError("actor name [" + actorName + "] not found. cannot move");
                return false;
            }

            origin = possibleOrigin.HasValue ? possibleOrigin.Value : actor.transform.position;

            Debug.Log(string.Format("translate init [{0}] from [{1}] to [{2}] d:s[{3}:{4}]",actorName,origin,destination,ElPresidente.currentDiscourseTime,ElPresidente.currentStoryTime));
            initialized = true;
            return true;
        }

        public override void Execute(float currentTime)
        {
            if (endTick - startTick < 1)
                return;
            float lerpPercent = (currentTime - startTick)/(endTick-startTick);
            Vector3 lerpd;
            lerpd.x = destination.X.HasValue ? Mathf.Lerp(origin.x,destination.X.Value, lerpPercent) : actor.transform.position.x;
            lerpd.y = destination.Y.HasValue ? Mathf.Lerp(origin.y, destination.Y.Value, lerpPercent) : actor.transform.position.y;
            lerpd.z = destination.Z.HasValue ? Mathf.Lerp(origin.z, destination.Z.Value, lerpPercent) : actor.transform.position.z;
            //Debug.Log(string.Format("translate execute [{0}] from [{1}] to [{2}] d:s[{3}:{4}]", actorName, actor.transform.position, lerpd, 
              //  ElPresidente.currentDiscourseTime, ElPresidente.currentStoryTime));

            actor.transform.position = lerpd;
        }

        public override void Undo()
		{
			if (actor != null)
            {
                actor.transform.position = origin;
            }
		}

        public override void Skip()
        {
            //Debug.Log(string.Format("skipping translate [{0}]-[{1}] d:s[{2}:{3}]", 
              //  origin, destination, ElPresidente.currentDiscourseTime, ElPresidente.currentStoryTime));
            Vector3 newPosition;
            newPosition.x = destination.X ?? actor.transform.position.x;
            newPosition.y = destination.Y ?? actor.transform.position.y;
            newPosition.z = destination.Z ?? actor.transform.position.z;
            actor.transform.position = newPosition;
        }

        public override void Stop()
        {
            //nothing to stop
        }
    }
}
