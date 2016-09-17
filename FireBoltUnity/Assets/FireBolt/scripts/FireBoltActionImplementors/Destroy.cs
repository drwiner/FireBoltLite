using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM=CinematicModel;

namespace Assets.scripts
{
    /// <summary>
    /// this only marks the object as inactive so it stops rendering.  
    /// we don't actually remove the object from the scene.  
    /// this is going to be problematic when we start using it for projectiles unless 
    /// we check for the existence of an actor already in the scene with the same name
    /// and recycle those game objects...which is do-able...then we have limit 1 of everything
    /// </summary>
    public class Destroy : FireBoltAction
    {
        string actorName;
		GameObject actor;

        public static bool ValidForConstruction(string actorName)
        {
            if (string.IsNullOrEmpty(actorName))
                return false;
            return true;
        }

        public Destroy(float startTick, string actorName) :
            base(startTick,startTick)
        {
            this.actorName = actorName;
        }

        public override bool Init()
        {
			if (actor != null)
			{
				actor.SetActive(false);
				return true;
			}
            
            if (actor == null &&
                !getActorByName(actorName, out actor))
            {
                Debug.LogError(string.Format("actor[{0}] not found for destroy", actorName));
                return false;
            }
            actor.SetActive(false);
            return true;
        }

		public override void Undo()
		{
			if (actor != null)
			    actor.SetActive (true);
		}

        public override void Skip()
        {
            // nothing to skip
        }

        public override void Execute(float currentTime)
        {
            //nothing to do
        }

        public override void Stop()
        {
            //nothing to stop
        }
    }
}
