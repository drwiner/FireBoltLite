using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel
{
    
    public class AttachAction : FireBoltAction
    {
        [XmlAttribute("parentParamName")]
        public string ParentParamName { get; set; }

        [XmlAttribute("attach")]
        public bool Attach { get; set; }


    }
}
