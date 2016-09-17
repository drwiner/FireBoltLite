using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel
{
    
    public class TranslateAction : FireBoltAction
    {
        [XmlAttribute(AttributeName = "destinationParamName")]
        public string DestinationParamName { get; set; }
        [XmlAttribute(AttributeName = "originParamName")]
        public string OriginParamName { get; set; }
    }
}
