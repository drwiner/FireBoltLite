using UnityEngine;
using CM = CinematicModel;
using UnityEditor;
using System.Collections.Generic;

namespace Assets.scripts
{
    public class AnimateMecanim : FireBoltAction
    {
        private string actorName;
        private GameObject actor;
        private string animName;
        private string stateName;
        private Animator animator;
        private AnimationClip animation;
        private AnimationClip state;
        AnimatorOverrideController overrideController;
        private int stopTriggerHash;
        private bool loop;
        private static readonly string animationToOverride = "_87_a_U1_M_P_idle_Neutral__Fb_p0_No_1";
        private static readonly string stateToOverride = "state";
        bool assignEndState = false;

        private static Dictionary<string, AnimationClip> cachedAnimationClips = new Dictionary<string, AnimationClip>();

        public static bool ValidForConstruction(string actorName, CM.Animation animation)
        {
            if (string.IsNullOrEmpty(actorName) || animation == null || string.IsNullOrEmpty(animation.FileName))
                return false;
            return true;
        }


        public AnimateMecanim(float startTick, float endTick, string actorName, string animName, bool loop, string endingName) :
            base(startTick, endTick)
        {
            this.actorName = actorName;
            this.animName = animName;
			this.loop = loop;
            this.assignEndState = !string.IsNullOrEmpty(endingName);
            this.stateName = endingName; 
            stopTriggerHash = Animator.StringToHash("stop");
        }

        public override bool Init()
        {
            //short circuit if this has clearly been initialized before
           
            if(animator && overrideController && animation && 
                (!assignEndState ||(assignEndState && state)))
            {
                assignAnimations();
                animator.runtimeAnimatorController = overrideController;
                actor.SetActive(true);
                return true;
            }

            if (!findAnimations()) return false;
            animation.wrapMode = loop ? WrapMode.Loop : WrapMode.Once;

            //get the actor this animate action is supposed to affect
            if(actor == null &&
               !getActorByName(actorName, out actor))
            {
                Debug.LogError("actor[" + actorName + "] not found.  cannot animate");
                return false;
            }

            //get the actor's current animator if it exists
            animator = actor.GetComponent<Animator>();
            if (animator == null)
            {
                animator = actor.AddComponent<Animator>();
            }
            

            //find or make an override controller
            if (animator.runtimeAnimatorController is AnimatorOverrideController)
            {
                overrideController = (AnimatorOverrideController) animator.runtimeAnimatorController;
            }
            else
            {
                overrideController = new AnimatorOverrideController();
                overrideController.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("AnimatorControllers/Generic");
                animator.runtimeAnimatorController = overrideController;
            }

            assignAnimations();
            animator.applyRootMotion = false;
            return true;
        }

        private void assignAnimations()
        {
            overrideController[animationToOverride] = animation;
            if (assignEndState)
                overrideController[stateToOverride] = state;
        }

        private bool findAnimations()
        {
            AnimationClip animationClip;
            if (!lookupAnimation(animName, out animationClip))
            {
                return false;
            }

            animation = animationClip;

            AnimationClip stateClip;
            if (!string.IsNullOrEmpty(stateName))
            {
                if (!lookupAnimation(stateName, out stateClip))
                {
                    return false;
                }
                state = stateClip;
            }
            return true;
        }

        private bool lookupAnimation(string animationName, out AnimationClip clip)
        {
            clip = null;
            if (cachedAnimationClips.ContainsKey(animationName))
            {
                clip = cachedAnimationClips[animationName];
            }
            else
            {
                clip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/FireBolt/Resources/Animations/" + animName) as AnimationClip;
                if (clip == null)
                {
                    Debug.LogError(string.Format("unable to find animation [{0}] in animations folder", animationName));
                    return false;
                }

                cachedAnimationClips.Add(animationName, clip);
            }
            
            return true;
        }

        public override void Undo()
		{
		}

        public override void Skip()
        {
            //animator.SetTrigger(stopTriggerHash);
        }

        public override string GetMainActorName()
        {
            return actorName;
        }

        public override void Execute(float currentTime) 
        {
		    //let it roll          
            float at = Mathf.Repeat ((currentTime - startTick)/1000, animation.length);
            animator.CrossFade( "animating", 0, 0, at/animation.length);
	    }

        public override void Stop()
        {
           // animator.SetTrigger(stopTriggerHash);
        }

        public override string ToString()
        {
            return string.Format("animate {0} with {1}", actorName, animName);
        }
    }
}