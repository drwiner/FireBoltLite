using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM=CinematicModel;
using LN.Utilities;

namespace Assets.scripts
{
    public class Rotate : FireBoltAction
    {
        string actorName;
        private GameObject actor;

        //where was the actor facing at start
        private Quaternion startOrientation;

        //where should the actor be facing when this is done
        Vector3Nullable targetOrientation;
        Quaternion endOrientation;

        public static bool ValidForConstruction(string actorName, Vector3Nullable targetRotation, Vector2? targetPoint)
        {
            if (string.IsNullOrEmpty(actorName))
                return false;
            if ((targetRotation.X.HasValue || targetRotation.Y.HasValue || targetRotation.Z.HasValue) && //can't define an angle and a point
                targetPoint.HasValue)
                return false;
            return true;
        }

        public override string ToString()
        {
            return "Rotate " + actorName + " from " + startOrientation + " to " + targetOrientation;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startTick"></param>
        /// <param name="endTick"></param>
        /// <param name="actorName"></param>
        /// <param name="targetRotation"></param>
        /// <param name="targetPoint">x,z</param>
        public Rotate(float startTick, float endTick, string actorName, Vector3Nullable targetRotation, Vector2? targetPoint) :
            base(startTick, endTick)
        {
            this.actorName = actorName;

            //TODO this smells.  but generality in all dimensions will take too long to figure out
            //if the targetPoint is specified, prefer to use it over a bare rotation value
            if (targetPoint.HasValue)
                this.targetOrientation = new Vector3Nullable(null, Mathf.Atan2(targetPoint.Value.x, targetPoint.Value.y) * Mathf.Rad2Deg, null);
            else
                this.targetOrientation = targetRotation;
        }

        public override bool Init()
        {
            if (actor != null)
            {
                startOrientation = actor.transform.rotation;
                actor.SetActive(true);
                return true;
            }

            if (actor == null &&
               !getActorByName(actorName, out actor))
            {
                Debug.LogError("actor name [" + actorName + "] not found. cannot rotate");
                return false;
            }

            startOrientation = actor.transform.rotation;

            // i must away to el Presidente and discover what happens when I scrub back to the middle of an action
            //the verdict: all actions that have not yet completed at the target time will be undone and then reinitialized and executed

            //let's figure out how far we need to go
            //first we need to convert all the rotation values we have into a standard form.
            endOrientation = Quaternion.Euler(new Vector3(targetOrientation.X ?? startOrientation.x,
                                                          targetOrientation.Y ?? startOrientation.y,
                                                          targetOrientation.Z ?? startOrientation.z));

            Debug.Log(this.ToString());
            return true;
        }

        public override void Execute(float currentTime)
        {
            if (endTick - startTick < 1)
                return;

            //how much of our rotate duration has elapsed?
            float percentCompleted = (currentTime - startTick) / (endTick - startTick);

            actor.transform.rotation = Quaternion.Slerp(startOrientation, endOrientation, percentCompleted);

            //Debug.DrawRay(actor.transform.position + Vector3.up, actor.transform.forward,Color.magenta);
        }

        public override void Undo()
        {
            if (actor != null)
            {
                actor.transform.rotation = startOrientation;
            }
        }

        public override void Skip()
        {
            actor.transform.rotation = endOrientation;
        }

        public override void Stop()
        {
            //nothing to stop
        }

        /// <summary>
        /// kind of a second constructor for mashing up a pan and tilt to operation at the same time.
        /// only allows merging if both tracked and actor are the same and there isn't already a rotation 
        /// on the requested axis within this rotate action.
        /// </summary>
        /// <param name="pan"></param>
        /// <param name="tilt"></param>
        /// <returns></returns>
        public void AppendAxisX(float xOrientation)
        {
            if (targetOrientation.X.HasValue)
            {
                Debug.Log(string.Format("Cannot merge multiple x axis rotations. Attempt to append rotate to tilt[{0}] to exsisting rotate with tilt[{1}].",
                                        xOrientation, targetOrientation.X.Value));
                return;
            }
            targetOrientation.X = xOrientation;
        }

        public void AppendAxisY(float yOrientation)
        {
            if (targetOrientation.Y.HasValue)
            {
                Debug.Log(string.Format("Cannot merge multiple y axis rotations. Attempt to append rotate to pan[{0}] to exsisting rotate with pan[{1}].",
                                        yOrientation, targetOrientation.Y.Value));
                return;
            }
            targetOrientation.Y = yOrientation;
        }
    }
}
