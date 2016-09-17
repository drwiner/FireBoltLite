using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using LN.Utilities;

namespace Assets.scripts
{
    public class TranslateRelative : Translate
    {
        private string trackedActorName;
        private GameObject trackedActor;
        private Vector3 trackedPositionInit;
        private Vector3 trackedPositionLast;
        private bool[] dimensionLock = {false,false,false};

        public TranslateRelative(string trackedActorName, float startTick, float endTick, string actorName, bool xLock, bool yLock, bool zLock) :
            base(startTick, endTick, actorName, Vector3.zero, new Vector3Nullable(null,null,null))
        {
            this.trackedActorName = trackedActorName;
            dimensionLock[0] = xLock;
            dimensionLock[1] = yLock;
            dimensionLock[2] = zLock;
        }

        public override bool Init()
        {
            if (getActorByName(trackedActorName, out trackedActor))
            {
                trackedPositionInit = trackedActor.transform.position;
                trackedPositionLast = trackedPositionInit;
                if (base.Init())
                {
                    this.origin = actor.transform.position;
                    return true;
                }                              
            }                
            return false;
        }

        public override void Execute(float currentTime)
        {
            Vector3 trackedPositionCurrent = trackedActor.transform.position;
            Vector3 move = trackedPositionCurrent - trackedPositionLast;

            Vector3 actorPosition = this.actor.transform.position;
            actorPosition.x += dimensionLock[0]? 0 : move.x;
            actorPosition.y += dimensionLock[1]? 0 : move.y;
            actorPosition.z += dimensionLock[2]? 0 : move.z;
            this.actor.transform.position = actorPosition;

            trackedPositionLast = trackedPositionCurrent;
        }


        //skipping assumes this action sorts after the move that it mirrors.  fails when we track over multiple actor moves.  
        //this is handled for camera move-withs since camera actions happen after actor actions.  If we want to expose this
        //as a base fireBolt action within the cinematic model, we need to figure out ordering when skipping.  It might be
        //able to carry a reference to the actions on the tracked actor...somehow, and itself make calls to skip those when appropriate

        //should implement sorting on executing actions to put relative movements last || just fix this so it's not so janky
        public override void Skip() 
        {
            this.actor.transform.position = trackedActor.transform.position - trackedPositionInit;
        }

        public override void Undo()
        {
            this.actor.transform.position = this.origin;
        }

        public override void Stop()
        {

        }
    }
}
