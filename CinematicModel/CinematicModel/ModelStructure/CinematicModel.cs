using System;
using System.Collections.Generic;
using System.Xml.Serialization;


namespace CinematicModel
{
    [Serializable, XmlRoot(ElementName = "cinematicModel", Namespace = "http://liquidnarrative.csc.ncsu.edu/cinematicModel/v0.1")]
    public class CinematicModel
    {
        private Dictionary<string, Actor> actors;
        private Dictionary<string, Animation> animations;

        private float domainDistancePerEngineDistanceX;
        private float domainDistancePerEngineDistanceY;
        private float domainDistancePerEngineDistanceZ;

        public CinematicModel()
        {
            actors = new Dictionary<string, Actor>();
            animations = new Dictionary<string, Animation>();
            domainDistancePerEngineDistanceX = 1f;
            domainDistancePerEngineDistanceY = 1f;
            domainDistancePerEngineDistanceZ = 1f;
        }

        [XmlAttribute("millisPerTick")]
        public uint MillisPerTick { get; set; }

        [XmlAttribute("domainDistancePerEngineDistance")] 
        public string DomainDistancePerEngineDistanceString
        {
            get
            {
                return string.Format("{0},{1},{2}", domainDistancePerEngineDistanceX,
                                                    domainDistancePerEngineDistanceY,
                                                    domainDistancePerEngineDistanceZ);
            }
            set
            {
                string[] dimensions = value.Split(new char[] { ',' });
                float parsed;
                if (float.TryParse(dimensions[0], out parsed))
                {
                    domainDistancePerEngineDistanceX = parsed;
                }
                if (float.TryParse(dimensions[1], out parsed))
                {
                    domainDistancePerEngineDistanceY = parsed;
                }
                if (float.TryParse(dimensions[2], out parsed))
                {
                    domainDistancePerEngineDistanceZ = parsed;
                }
            }
        }

        /// <summary>
        /// x,z plane is floor/terrain
        /// </summary>
        public float DomainDistancePerEngineDistanceX
        {
            get
            {
                return domainDistancePerEngineDistanceX;
            }
        }

        /// <summary>
        /// vertical
        /// </summary>
        public float DomainDistancePerEngineDistanceY
        {
            get
            {
                return domainDistancePerEngineDistanceY;
            }
        }

        /// <summary>
        /// x,z plane is floor/terrain
        /// </summary>
        public float DomainDistancePerEngineDistanceZ
        {
            get
            {
                return domainDistancePerEngineDistanceZ;
            }
        }

        [XmlElement("smartModelSettings")]
        public SmartModelSettings SmartModelSettings { get; set; }

        [XmlElement("terrain")]
        public Terrain Terrain { get; set; }

        [XmlArray(ElementName = "domainActions")]
        [XmlArrayItem(ElementName = "domainAction")]
        public List<DomainAction> DomainActions { get; set; }

        [XmlArray(ElementName = "actors")]
        [XmlArrayItem(ElementName = "actor")]
        public List<Actor> Actors { get; set; }

        [XmlArray(ElementName = "animations")]
        [XmlArrayItem(ElementName = "animation")]
        public List<Animation> Animations { get; set; }

        public Animation FindAnimation(string animationName)
        {
            Animation animation;
            if (animations.TryGetValue(animationName,out animation))
            {
                return animation;
            }
            animation = Animations.Find(x => x.Name == animationName);
            animations.Add(animationName, animation);
            return animation;
        }

        private Actor findActor(string actorName)
        {
            Actor actor;
            if (actors.TryGetValue(actorName, out actor))
            {
                return actor;
            }
            actor = Actors.Find(x => x.Name == actorName);
            actors.Add(actorName,actor);
            return actor;
        }

        public bool TryGetActor(string actorName, out Actor actor)
        {
            actor = findActor(actorName);
            if (actor == null) return false;
            return true;
        }

    }
}
