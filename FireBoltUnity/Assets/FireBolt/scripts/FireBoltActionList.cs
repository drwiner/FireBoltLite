using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LN.Utilities.Collections;

namespace Assets.scripts
{
    public class ActionTypeComparer : IComparer<FireBoltAction>
    {
        public int Compare(FireBoltAction x, FireBoltAction y)
        {
            if (x.Equals(y)) return 0;
            if (x is SetStoryTime) return -1;
            if (y is SetStoryTime) return 1;
            if (x is Destroy) return 1;
            if (y is Destroy) return -1;
            if (x is Create) return -1;
            if (y is Create) return 1;

            if (x is Translate && y is Rotate) return -1;
            if (x is Rotate && y is Translate) return 1;
            if ((x is Translate || x is Rotate) && y is Focus) return -1;
            if (x is Focus && (y is Translate || y is Rotate)) return 1;
            return -1;

        }
    }

    public class StartTickComparer : IComparer<FireBoltAction>
    {
        public int Compare(FireBoltAction x, FireBoltAction y)
        {
            if (x.StartTick() > y.StartTick())
            {
                return 1;
            }
            else if (x.StartTick() == y.StartTick())
            {
                if (x is Create) return -1;
                if (y is Create) return 1;
            }
            return -1;
        }
    }

    public class FireBoltActionList : SortedSet<FireBoltAction>
    {
        public FireBoltActionList() :
            base(new StartTickComparer())
        {
            NextActionIndex = 0;
        }

        public FireBoltActionList(IComparer<FireBoltAction> comparer) :
            base(comparer)
        {
            NextActionIndex = 0;
        }

        /// <summary>
        /// call execute for all actions in the list.
        /// </summary>
        public void ExecuteList(float currentTime)
        {
            foreach (FireBoltAction action in this)
            {
                action.Execute(currentTime);
            }
        }

        /// <summary>
        /// pointer to the next action from the queue
        /// </summary>
        public int NextActionIndex { get; set; }
    }

    public class CameraActionList : FireBoltActionList
    {
        public uint EndDiscourseTime { get; set; }
    }
}
