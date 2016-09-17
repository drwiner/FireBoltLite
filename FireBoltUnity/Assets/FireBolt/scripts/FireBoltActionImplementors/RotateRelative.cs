using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using LN.Utilities;

namespace Assets.scripts
{
    //used to override rotate, but we minimally used underlying code, and it was just confusing.
    public class RotateRelative : FireBoltAction
    {
        private string trackedActorName;
        private GameObject trackedActor;      

        private string actorName;
        private GameObject actor;
        private Vector3 startOrientation;

        private bool pan;
        private bool tilt;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trackedActorName"></param>
        /// <param name="startTick"></param>
        /// <param name="endTick"></param>
        /// <param name="actorName"></param>
        /// <param name="xLock">rotate about x axis</param>
        /// <param name="yLock">rotate about y axis</param>
        /// <param name="rotateZ">rotate about z axis</param>
        public RotateRelative(string trackedActorName, float startTick, float endTick, string actorName, bool pan, bool tilt) :
            base(startTick, endTick)
        {
            this.trackedActorName = trackedActorName;
            this.actorName = actorName;
            this.pan = pan;
            this.tilt = tilt;
        }

        public override bool Init()
        {
            if (trackedActor && actor)
                return true;                

            //get our actor
                                                     
            if (actor == null &&
                !getActorByName(actorName, out actor))
            {
                Debug.LogError("actor name [" + actorName + "] not found. cannot rotate");
                return false;
            }

            startOrientation = actor.transform.rotation.eulerAngles;

            //find actor that should be tracked
            if (trackedActor == null &&
                !getActorByName(trackedActorName, out trackedActor))
            {
                Debug.LogError(string.Format("actor to track [{0}] by actor [{1}] not found.  cannot rotate",trackedActorName, actorName));
                return false;
            }

            return true;
        }

        public override void Execute(float currentTime)
        {
            setRotation();
        }
        
        //assumes this action sorts after the move that it mirrors.  this works well for cameras tracking things and 
        //less well for actors tracking other actors since camera actions are forced to play second        
        //assumes we will land on the actor when this is over
        public override void Skip()
        {
            setRotation();
        }

        public override void Undo()
        {
            this.actor.transform.rotation = Quaternion.Euler(startOrientation);
        }

        public override void Stop()
        {
            
        }

        private void setRotation()
        {
            //capture updated tracked position
            Vector3 trackedPositionCurrent = trackedActor.transform.position;

            //find the shortest way to get to him
            //get current position for our actor
            Vector3 actorPosition = actor.transform.position;

            Vector3 actorRotation = actor.transform.rotation.eulerAngles;

            //get the direction from our actor to the tracked actor.  this is the vector along which we are trying to align
            //apply no direction along axes we are not tracking
            Vector3 targetRotation = Vector3.zero;
            targetRotation = Quaternion.LookRotation(trackedPositionCurrent - actorPosition).eulerAngles;

            //remove roll 
            targetRotation.z = actorRotation.z;
            if (!pan)
            {
                targetRotation.y = actorRotation.y;
            }
            if (!tilt)
            {
                targetRotation.x = actorRotation.x;
            }

            //i want to use slerp here, but when manipulating time, it doesn't work.  if i scrub back into the very tail end of a 
            //camera action, it gets undone, so the camera rig orientation is at start, then my tracked actor gets set to a point 
            //where his move is almost completed, then i need to catch up to being close and then slerp.  or just force myself to 
            //face the tracked actor.  this is unfortunate b/c it limits my ability to cap rotation speeds on the camera
            actor.transform.rotation = Quaternion.Euler(targetRotation);
        }

        /// <summary>
        /// kind of a second constructor for mashing up a pan and tilt with operation at the same time.
        /// only allows merging if both tracked and actor are the same and there isn't already a rotation 
        /// on the requested axis within this rotate action.
        /// </summary>
        /// <param name="trackedActorName"></param>
        /// <param name="actorName"></param>
        /// <param name="pan"></param>
        /// <param name="tilt"></param>
        /// <returns></returns>
        public void AppendAxis(string trackedActorName, string actorName, bool pan, bool tilt)
        {
            if (trackedActorName != this.trackedActorName)
            {
                Debug.Log(string.Format("Cannot merge on multiple tracking targets. Attempt to append rotate relative with tracked actor[{0}], to existing rotate with tracked actor[{1}].",
                                        trackedActorName, this.trackedActorName));                
                return;
            }
            if (actorName != this.actorName)
            {
                Debug.Log(string.Format("Cannot merge on multiple actors. Attempt to append rotate relative with actor[{0}], to existing rotate with actor[{1}].",
                                        actorName, this.actorName));
                return;
            }
            if(pan && this.pan)
            {
                Debug.Log(string.Format("Cannot merge multiple same axis rotations. Attempt to append rotate relative with pan[{0}] to exsisting rotate with pan[{1}].",
                                        pan, this.pan));
                return;
            }
            else if (pan && !this.pan)
            {
                this.pan = true;
            }
            if (tilt && this.tilt)
            {
                Debug.Log(string.Format("Cannot merge multiple same axis tilts. Attempt to append rotate relative with tilt[{0}] to exsisting rotate with tilt[{1}].",
                                        tilt, this.tilt));
                return;
            }
            else if (tilt && !this.tilt)
            {
                this.tilt = true;
            }
            
        }
    }
}
