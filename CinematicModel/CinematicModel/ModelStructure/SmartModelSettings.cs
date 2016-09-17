using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel
{
    //TODO makes more sense as an attribute of an Actors and Animations class respectively, but that requires further code change
    /// <summary>
    /// describes how the hierarchy ofthe xImpulse plan will be used to fill in missing information in the read model.
    /// </summary>
    public class SmartModelSettings
    {
        public SmartModelSettings()
        {
            AnimationMaxSearchDepth = 10;
            ActorMaxSearchDepth = 10;
        }

        /// <summary>
        /// how many steps the model will attempt to step up in the xImpulse object hierarchy when 
        /// confronted with an action for which the executing actor has no animation mapping.  
        /// Attempts to use animation mappings for parent objects.
        /// Set to 0 to prevent searching.
        /// </summary>
        [XmlAttribute("maxAnimationSearchDepth")]
        [DefaultValue(10)]
        public uint AnimationMaxSearchDepth { get; set; }

        /// <summary>
        /// how many steps the model will attempt to step up in the xImpulse object hierarchy when 
        /// confronted with an actor whose name is unknown.  Will use a model defined for a parent
        /// object class provided it is within maxActorSearchDepth steps of the actor in the hierarchy
        /// Set to 0 to prevent searching.
        /// </summary>
        [XmlAttribute("maxActorSearchDepth")]
        [DefaultValue(10)]
        public uint ActorMaxSearchDepth { get; set; }
    }
}
