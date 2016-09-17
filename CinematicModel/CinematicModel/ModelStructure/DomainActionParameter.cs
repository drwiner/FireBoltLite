using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel
{
    public class DomainActionParameter
    {
        /// <summary>
        /// correlates name of parameter with parameter in xImpulse domain model
        /// </summary>
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }
}
