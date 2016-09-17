using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel
{
    public class Animation
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "fileName")]
        public string FileName { get; set; }
                
        [XmlArray("animationIndices")]
        [XmlArrayItem("animationIndex")]
        public List<AnimationIndex> AnimationIndices { get; set; }
    }
}
