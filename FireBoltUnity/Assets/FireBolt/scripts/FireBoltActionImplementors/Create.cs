using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM=CinematicModel;
//using UnityEditor;

namespace Assets.scripts
{
    public class Create : FireBoltAction
    {
        string actorName,modelName;
        Vector3 position;
        Vector3? orientation;
		GameObject actor;
        bool defaultedCreate;

        public static bool ValidForConstruction(string actorName, string modelName)
        {
            if (string.IsNullOrEmpty(actorName) || string.IsNullOrEmpty(modelName))
                return false;
            return true;
        }

        public override string ToString ()
        {
            return string.Format ("Create " + actorName);
        }

        public Create(float startTick, string actorName, string modelName, Vector3 position, Vector3? orientation=null, bool defaultedCreate=false) :
            base(startTick, startTick)
        {
            this.startTick = startTick;
            this.actorName = actorName;
            this.modelName = modelName;
            this.position = position;
            this.orientation = orientation;
            this.defaultedCreate = defaultedCreate;
			this.actor = null;
        }

        public override bool Init()
        {
            Debug.Log(string.Format("init create model[{0}] for actor [{1}]",modelName, actorName));
            if (getActorByName(actorName, out actor))
            {
                if (defaultedCreate)
                    actor.SetActive(false);
                else
                    actor.SetActive(true);

                actor.transform.position = position;
                if(orientation.HasValue)
                    actor.transform.rotation = Quaternion.Euler(orientation.Value);
                return true;
            }
            GameObject model = null;
            if (ElPresidente.Instance.GetActiveAssetBundle().Contains(modelName))
            {
                model = ElPresidente.Instance.GetActiveAssetBundle().LoadAsset<GameObject>(modelName);
            }

            if (model == null)
            {
                Debug.LogError(string.Format("could not load asset[{0}] from assetbundle[{1}]", 
                                             modelName, ElPresidente.Instance.GetActiveAssetBundle().name));
                return false;
            }

            Quaternion actorOrientation = orientation.HasValue ?Quaternion.Euler(orientation.Value) : model.transform.rotation;
            actor = GameObject.Instantiate(model, position, actorOrientation) as GameObject;
            actor.name = actorName;

            GameObject instanceContainer;
            if (ElPresidente.createdGameObjects.TryGet("InstantiatedObjects", out instanceContainer))
            {               
                actor.transform.SetParent(instanceContainer.transform, true);
            }
            else
            {
                Debug.Log(string.Format("could not find InstantiatedObjects in createdGameObjects registry.  cannot add [{0}] in the hierarchy", actor));
            }
            //add actor to the main registry for quicker lookups
            ElPresidente.createdGameObjects.Add(actor.name, actor);

            //add a collider so we can raycast against this thing
            if (actor.GetComponent<BoxCollider>() == null)
            {
                BoxCollider collider = actor.AddComponent<BoxCollider>();
                Bounds bounds = getBounds(actor);
                collider.center = new Vector3(0,0.75f,0); //TODO un-hack and find proper center of model                
                collider.size = bounds.max - bounds.min;
            }
            if (defaultedCreate)
            {
                actor.SetActive(false);
            }
            return true;
        }

        private Bounds getBounds(GameObject gameObject)
        {
            Bounds bounds;
            var renderer = gameObject.GetComponent<Renderer>();

            if (renderer != null)
            {
                bounds = renderer.bounds;
            }
            //if the model does not directly have a renderer, accumulate from child bounds
            else
            {
                bounds = new Bounds(gameObject.transform.position, Vector3.zero);
                foreach (var r in gameObject.GetComponentsInChildren<Renderer>())
                {
                    bounds.Encapsulate(r.bounds);
                }
            }
            return bounds;
        }

        public override void Undo()
		{
            Debug.Log ("Undo create");
			if (actor != null)
            {
                actor.SetActive(false);
            }
            if (defaultedCreate)
            {
                actor.transform.position = position;
            }
			    
		}

        public override void Skip()
        {
            // nothing to skip
        }

        public override void Execute(float currentTime)
        {
            //nothing to do
        }

        public override void Stop()
        {
            //nothing to stop
        }
    }
}
