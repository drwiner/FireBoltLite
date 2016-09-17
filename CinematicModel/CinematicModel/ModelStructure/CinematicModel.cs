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

        public CinematicModel()
        {
            actors = new Dictionary<string, Actor>();
            animations = new Dictionary<string, Animation>();
        }

        [XmlAttribute("millisPerTick")]
        public uint MillisPerTick { get; set; }

        [XmlAttribute("domainDistancePerEngineDistance")]
        public float DomainDistancePerEngineDistance { get; set; }

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
