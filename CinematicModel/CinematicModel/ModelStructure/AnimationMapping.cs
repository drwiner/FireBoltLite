using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel
{
    public class AnimationMapping
    {
        [XmlAttribute("animateActionName")]
        public string AnimateActionName { get; set; }
        
        [XmlAttribute("animationName")]
        public string AnimationName { get; set; }

        [XmlAttribute("loopAnimation")]
        public bool LoopAnimation { get; set; }
    }
}
