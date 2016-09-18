using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.scripts
{
    public class CinematicModelMetaDataComponent : MonoBehaviour
    {
        public string ModelName;
        public string ActorName;
        public string AbstractActorName;
        public float PointOfInterestScalar;

        public void LoadFromStruct(CinematicModelMetaData existingData)
        {
            this.ModelName = existingData.ModelName;
            this.ActorName = existingData.ActorName;
            this.AbstractActorName = existingData.AbstractActorName;
            this.PointOfInterestScalar = existingData.PointOfInterestScalar;
        }
    }

    public struct CinematicModelMetaData
    {
        public string ModelName;
        public string ActorName;
        public string AbstractActorName;
        public float PointOfInterestScalar;
    }
}