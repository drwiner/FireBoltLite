using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.scripts
{    
    public class SetStoryTime : FireBoltAction
    {
        private float storyTimeOffset, previousStoryTimeOffset;
        
        /// <summary>
        /// Moves story time pointer.  Jump around...
        /// </summary>
        /// <param name="storyTimeOffset">this value + current discourse time tells you what story events should be playing</param>
        /// <param name="previousStoryTimeOffset">what the last offset was so we can undo our jump</param>
        /// <param name="startTick">when does the block for which we are setting story time start</param>
        /// <param name="endTick">when does the block end? This is necessary for scrubbing backward into the 
        /// set story time action</param>
        public SetStoryTime(float storyTimeOffset, float previousStoryTimeOffset, float startTick, float endTick) :
            base(startTick, endTick)
        {
            this.storyTimeOffset = storyTimeOffset;
            this.previousStoryTimeOffset = previousStoryTimeOffset;            
        }

        public override bool Init()
        {
            //normally we allow the skip to handle any instantaneous executions, but here we need to have the 
            //full block time as the duration for undoing and redoing over the edges
            ElPresidente.Instance.goToStoryTime(ElPresidente.Instance.CurrentDiscourseTime + storyTimeOffset);
            return true;
        }

        public override void Execute(float currentTime)
        {
            if (Mathf.Abs(ElPresidente.Instance.CurrentStoryTime - (ElPresidente.Instance.CurrentDiscourseTime + storyTimeOffset)) > ElPresidente.MILLIS_PER_FRAME)
            {
                ElPresidente.Instance.goToStoryTime(ElPresidente.Instance.CurrentDiscourseTime + storyTimeOffset);
            }
        }

        public override void Stop()
        {
            //nothin
        }

        public override void Undo()
        {
            Debug.Log(string.Format("set story time[{0}] ", startTick + previousStoryTimeOffset));
            ElPresidente.Instance.goToStoryTime(startTick + previousStoryTimeOffset);
        }

        public override void Skip()
        {
            Debug.Log(string.Format("set story time[{0}] ", endTick + storyTimeOffset));
            ElPresidente.Instance.goToStoryTime(endTick + storyTimeOffset);
        }
    }
}
