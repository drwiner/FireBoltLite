using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel
{
    
    public class CreateAction : FireBoltAction
    {
        [XmlAttribute(AttributeName = "originParamName")]
        public string OriginParamName { get; set; }

        [XmlAttribute(AttributeName = "orientationParamName")]
        public string OrientationParamName { get; set; }


    }
}
