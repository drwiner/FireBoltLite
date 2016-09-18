using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.scripts
{
    public abstract class FireBoltAction {

        protected float startTick;
        protected float endTick;

        public FireBoltAction(float startTick, float endTick)
        {
            this.startTick = startTick;
            this.endTick = endTick;
        }

        public float StartTick()
        {
            return startTick;
        }
        public float EndTick()
        {
            return endTick;
        }

        public virtual bool Init() { return false; }
        public virtual void Execute(float currentTime) { }
        public virtual void Stop() { }
        public virtual void Undo() { }
        public virtual void Skip() { }

        public abstract string GetMainActorName();

        /// <summary>
        /// finds requested actor and sets it active
        /// </summary>
        /// <param name="actorName"></param>
        /// <param name="actor"></param>
        /// <returns></returns>
        protected bool getActorByName(string actorName, out GameObject actor)
        {
            actor = null;
            if(!ElPresidente.createdGameObjects.TryGet(actorName, out actor))
            {
                return false;
            }
            if (!actor.activeSelf)
                actor.SetActive(true);
            return true;
        }
    }

}
