using UnityEngine;
using System.Collections;
using CM = CinematicModel;
using UnityEditor;
using LN.Utilities;
using System.Reflection;
using System;
using System.Collections.Generic;

namespace Assets.scripts
{
    public class InGameFunction : FireBoltAction
    {

        GameObject hostObject;
        List<Tuple<string,string,string>> tupleList;
        Component ingamescript;
        String objectName;


        public InGameFunction(float startTick, float endTick, string functionName, List<Tuple<string,string,string>> paramNames) :
            base(startTick, endTick)
        {
            //Unique objectName should also include paramNames
            objectName = startTick.ToString() + " " + endTick.ToString() + " " +  functionName;

            //If the objectName is not in the registry of created Game Objects
            if (!ElPresidente.createdGameObjects.TryGet(objectName, out hostObject))
            {
                //This prefab should have as component the functionName script, whatever functionName is. drawWhip_prefab has drawWhip script as component.
                hostObject = GameObject.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/FireBolt/Resources/" + functionName + "_prefab.prefab")) as GameObject;
                //Debug.Log(hostObject.name);

                //Attach to FireBolt object
                GameObject fireBolt;
                if (ElPresidente.createdGameObjects.TryGet("FireBolt", out fireBolt))
                {
                    hostObject.transform.SetParent(fireBolt.transform);
                }

                //Add to ElPresidente registry of created Game Objects
                ElPresidente.createdGameObjects.Add(objectName, hostObject);

                //Record the parameters
                tupleList = paramNames;
                //Keep pointer to functionName script component
                ingamescript = hostObject.GetComponent(functionName);
            }
            

        }

        //Send Messages to functionName script
        public override bool Init()
        {
            ingamescript.SendMessage("Init", tupleList);
            return true;
        }

        public override void Undo()
		{
            ingamescript.SendMessage("Undo");
		}

        public override void Skip()
        {
            ingamescript.SendMessage("Skip");
        }

        public override void Execute(float currentTime) 
        {
            ingamescript.SendMessage("Execute");
	    }

        public override void Stop()
        {
        }
    }
}