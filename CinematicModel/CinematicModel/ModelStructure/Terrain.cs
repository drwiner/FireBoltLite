using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel
{
    [Serializable]
    public class Terrain
    {

        public Terrain()
        {
            Location = "(0,0,0)";
        }

        [XmlAttribute("terrainFileName")]
        [DefaultValue("defaultTerrain")]
        public string TerrainFileName { get; set; }

        [XmlAttribute("location")]
        [DefaultValue("(0,0,0)")]
        public string Location { get; set; }
    }
}
