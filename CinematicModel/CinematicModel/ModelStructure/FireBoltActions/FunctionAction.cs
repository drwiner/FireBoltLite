using LN.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.Text;

namespace CinematicModel
{


    public class FunctionAction : FireBoltAction
    {
        public FunctionAction()
        {
        }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlArray("args")]
        [XmlArrayItem("arg")]
        public List<FunctionArgs> functionArgs { get; set; }

    }

    public class FunctionArgs
    {
        [XmlAttribute("argname")]
        public string argName { get; set; }

        [XmlAttribute("argtype")]
        public string argType { get; set; }

        [XmlAttribute("argvalue")]
        public string argValue { get; set; }
    }
   
}
